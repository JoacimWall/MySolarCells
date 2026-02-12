"""Tibber GraphQL client - ported from TibberService.cs."""

from __future__ import annotations

import base64
import logging
from datetime import datetime, timedelta, timezone
from typing import Any

import aiohttp

from .const import TIBBER_API_URL

_LOGGER = logging.getLogger(__name__)

# Tibber GraphQL queries
QUERY_TEST_CONNECTION = "{ viewer { accountType }}"

QUERY_GET_HOMES = (
    "{ viewer { homes { id address { address1 postalCode city country }}}}"
)

QUERY_CONSUMPTION_PRODUCTION = (
    "query getdata($homeid: ID!, $from: String, $first: Int) {"
    " viewer { home(id: $homeid) {"
    " consumption(resolution: HOURLY, after: $from, first: $first) {"
    " nodes { from to cost unitPrice unitPriceVAT consumption consumptionUnit currency }}"
    " production(resolution: HOURLY, after: $from, first: $first) {"
    " nodes { from to profit unitPrice unitPriceVAT production productionUnit currency}}"
    "}}}"
)

QUERY_SPOT_PRICES = (
    "query getdata($homeid: ID!) {"
    " viewer { home(id: $homeid) {"
    " currentSubscription {"
    " priceInfo(resolution: QUARTER_HOURLY) {"
    " today { total energy tax level startsAt currency }"
    " tomorrow { total energy tax level startsAt currency }"
    "}}}}}"
)


def _encode_to_base64(text: str) -> str:
    """Base64-encode a string (mirrors StringHelper.EncodeTo64)."""
    return base64.b64encode(text.encode("utf-8")).decode("utf-8")


def _parse_timestamp(ts_str: str) -> datetime:
    """Parse an ISO timestamp string to a datetime object."""
    # Tibber returns ISO 8601 with timezone offset, e.g. "2025-06-15T14:00:00.000+02:00"
    return datetime.fromisoformat(ts_str)


def _to_utc_key(dt: datetime) -> str:
    """Convert a datetime to a UTC ISO string without offset for use as storage key.

    This ensures consistent string comparison in get_records_for_period.
    Example: "2025-06-15T14:00:00+02:00" -> "2025-06-15T12:00:00"
    """
    utc_dt = dt.astimezone(timezone.utc)
    return utc_dt.strftime("%Y-%m-%dT%H:%M:%S")


class TibberApiError(Exception):
    """Raised when a Tibber API call fails."""


class TibberClient:
    """Async client for the Tibber GraphQL API."""

    def __init__(self, session: aiohttp.ClientSession, api_key: str) -> None:
        self._session = session
        self._api_key = api_key

    @property
    def _headers(self) -> dict[str, str]:
        return {
            "Authorization": f"Bearer {self._api_key}",
            "Content-Type": "application/json",
        }

    async def _post(self, query: str, variables: dict[str, Any] | None = None) -> dict:
        """Execute a GraphQL query against Tibber API."""
        payload: dict[str, Any] = {"query": query}
        if variables:
            payload["variables"] = variables

        async with self._session.post(
            TIBBER_API_URL, json=payload, headers=self._headers
        ) as resp:
            if resp.status != 200:
                text = await resp.text()
                raise TibberApiError(f"Tibber API returned {resp.status}: {text}")
            data = await resp.json()
            if "errors" in data:
                raise TibberApiError(f"Tibber API error: {data['errors']}")
            return data

    async def test_connection(self) -> str:
        """Test the API key. Returns the account type string."""
        data = await self._post(QUERY_TEST_CONNECTION)
        return data["data"]["viewer"]["accountType"][0]

    async def get_homes(self) -> list[dict[str, Any]]:
        """Get list of homes associated with the API key.

        Returns list of dicts with keys: id, address1, postal_code, city, country.
        """
        data = await self._post(QUERY_GET_HOMES)
        homes = []
        for home in data["data"]["viewer"]["homes"]:
            addr = home["address"]
            homes.append(
                {
                    "id": home["id"],
                    "address1": addr.get("address1", ""),
                    "postal_code": addr.get("postalCode", ""),
                    "city": addr.get("city", ""),
                    "country": addr.get("country", ""),
                }
            )
        return homes

    async def get_consumption_production(
        self,
        home_id: str,
        start: datetime,
        import_only_spot_price: bool = False,
    ) -> list[dict[str, Any]]:
        """Fetch hourly consumption/production data from Tibber.

        Ports the Sync() method from TibberService.cs with:
        - 3-month batch windows (lines 121-134)
        - DST handling: split 2-hour blocks into two 1-hour records (lines 167-189)
        - Match production to consumption by timestamp (line 243)
        - Sync flag: only mark as synced if 15 min past the hour (lines 269-274)

        Returns list of hourly energy record dicts.
        """
        records: list[dict[str, Any]] = []
        # Ensure start is timezone-aware
        if start.tzinfo is None:
            start = start.replace(tzinfo=timezone.utc)
        now = datetime.now(tz=timezone.utc)
        end = datetime(now.year, now.month, now.day, tzinfo=timezone.utc) + timedelta(days=1)
        current = start

        while current < end:
            # 3-month batch windows
            if current + timedelta(days=90) < end:
                batch_start = current
                batch_end = current + timedelta(days=90)
                current = batch_end
            else:
                batch_start = current
                batch_end = end
                current = end

            diff = batch_end - batch_start
            hours_total = int(diff.total_seconds() / 3600)

            if hours_total <= 0:
                continue

            from_encoded = _encode_to_base64(batch_start.strftime("%Y-%m-%dT%H:%M:%S"))

            variables = {
                "homeid": home_id,
                "from": from_encoded,
                "first": hours_total,
            }

            try:
                data = await self._post(QUERY_CONSUMPTION_PRODUCTION, variables)
            except TibberApiError as err:
                _LOGGER.error("Failed to fetch consumption/production: %s", err)
                continue

            viewer = data["data"]["viewer"]["home"]
            consumption_nodes = viewer.get("consumption", {}).get("nodes", []) or []
            production_nodes = viewer.get("production", {}).get("nodes", []) or []

            # Build production lookup by timestamp
            prod_by_time: dict[str, dict] = {}
            for pnode in production_nodes:
                if pnode.get("from"):
                    prod_by_time[pnode["from"]] = pnode

            # Process consumption nodes with DST handling
            i = 0
            extended_nodes = list(consumption_nodes)
            while i < len(extended_nodes):
                node = extended_nodes[i]
                i += 1

                if not node.get("from") or not node.get("to"):
                    continue

                from_dt = _parse_timestamp(node["from"])
                to_dt = _parse_timestamp(node["to"])

                # DST handling: if span > 1 hour, split into two 1-hour records
                span_hours = (to_dt - from_dt).total_seconds() / 3600
                if span_hours > 1.5:  # DST transition creates 2-hour blocks
                    # Create second half-hour record
                    node2 = {
                        "from": (from_dt + timedelta(hours=1)).isoformat(),
                        "to": node["to"],
                        "cost": (node.get("cost") or 0) / 2,
                        "unitPrice": node.get("unitPrice"),
                        "unitPriceVAT": node.get("unitPriceVAT"),
                        "consumption": (node.get("consumption") or 0) / 2,
                        "consumptionUnit": node.get("consumptionUnit"),
                        "currency": node.get("currency"),
                    }
                    extended_nodes.append(node2)

                    # Fix first record
                    node["to"] = (from_dt + timedelta(hours=1)).isoformat()
                    node["cost"] = (node.get("cost") or 0) / 2
                    node["consumption"] = (node.get("consumption") or 0) / 2

                    from_dt = _parse_timestamp(node["from"])
                    to_dt = _parse_timestamp(node["to"])

                # Build energy record
                consumption_val = float(node.get("consumption") or 0)
                cost_val = float(node.get("cost") or 0)
                unit_price_buy = float(node.get("unitPrice") or 0)
                unit_price_vat_buy = float(node.get("unitPriceVAT") or 0)

                # Match production by timestamp
                prod_node = prod_by_time.get(node["from"], {})
                production_sold = float(prod_node.get("production") or 0)
                production_sold_profit = float(prod_node.get("profit") or 0)
                unit_price_sold = float(prod_node.get("unitPrice") or 0)
                unit_price_vat_sold = float(prod_node.get("unitPriceVAT") or 0)

                # Sync flag: only mark as synced if 15 min past the hour
                now_minus_15 = now - timedelta(minutes=15)
                synced_cutoff = now_minus_15.replace(minute=0, second=0, microsecond=0)
                is_synced = to_dt <= synced_cutoff

                if import_only_spot_price:
                    consumption_val = 0
                    cost_val = 0
                    production_sold = 0
                    production_sold_profit = 0

                record = {
                    "timestamp": _to_utc_key(from_dt),
                    "purchased": consumption_val,
                    "purchased_cost": cost_val,
                    "production_sold": production_sold,
                    "production_sold_profit": production_sold_profit,
                    "production_own_use": 0.0,  # Calculated from HA sensors
                    "production_own_use_profit": 0.0,
                    "battery_charge": 0.0,
                    "battery_used": 0.0,
                    "battery_used_profit": 0.0,
                    "unit_price_buy": unit_price_buy,
                    "unit_price_vat_buy": unit_price_vat_buy,
                    "unit_price_sold": unit_price_sold,
                    "unit_price_vat_sold": unit_price_vat_sold,
                    "price_level": "",
                    "synced": is_synced,
                }
                records.append(record)

        return records

    async def get_spot_prices(self, home_id: str) -> dict[str, Any]:
        """Fetch today + tomorrow spot prices at QUARTER_HOURLY resolution.

        Returns dict with keys: today (list), tomorrow (list).
        Each price entry: {total, energy, tax, level, starts_at, currency}.
        """
        variables = {"homeid": home_id}
        data = await self._post(QUERY_SPOT_PRICES, variables)

        price_info = (
            data["data"]["viewer"]["home"]["currentSubscription"]["priceInfo"]
        )

        def _parse_prices(price_list: list[dict] | None) -> list[dict[str, Any]]:
            if not price_list:
                return []
            result = []
            for p in price_list:
                starts_at_raw = p.get("startsAt", "")
                if starts_at_raw:
                    try:
                        starts_at = _to_utc_key(_parse_timestamp(starts_at_raw))
                    except (ValueError, TypeError):
                        starts_at = starts_at_raw
                else:
                    starts_at = ""
                result.append(
                    {
                        "total": float(p.get("total") or 0),
                        "energy": float(p.get("energy") or 0),
                        "tax": float(p.get("tax") or 0),
                        "level": p.get("level", ""),
                        "starts_at": starts_at,
                        "currency": p.get("currency", "SEK"),
                    }
                )
            return result

        return {
            "today": _parse_prices(price_info.get("today")),
            "tomorrow": _parse_prices(price_info.get("tomorrow")),
        }
