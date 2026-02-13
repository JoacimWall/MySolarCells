"""Persistent JSON storage for My Solar Cells - uses HA Store helper."""

from __future__ import annotations

import logging
from typing import Any

from homeassistant.core import HomeAssistant
from homeassistant.helpers.storage import Store

from .const import STORAGE_KEY_PREFIX, STORAGE_VERSION

_LOGGER = logging.getLogger(__name__)


class MySolarCellsStorage:
    """Manages persistent storage for hourly energy records and spot prices."""

    def __init__(self, hass: HomeAssistant, entry_id: str) -> None:
        self._store = Store(
            hass,
            STORAGE_VERSION,
            f"{STORAGE_KEY_PREFIX}.{entry_id}",
        )
        self._data: dict[str, Any] = {}

    async def async_load(self) -> None:
        """Load data from storage."""
        stored = await self._store.async_load()
        if stored and isinstance(stored, dict):
            self._data = stored
            self._migrate_timezone_keys()
        else:
            self._data = {
                "hourly_records": {},
                "quarter_hourly_prices": {},
                "last_tibber_sync": None,
                "monthly_cache": {},
                "roi_projection": [],
                "last_sensor_readings": {},
            }

    def _migrate_timezone_keys(self) -> None:
        """Clear records with timezone offsets in keys (v1 bug).

        Old keys looked like '2025-06-15T14:00:00+02:00', new keys are
        UTC without offset like '2025-06-15T12:00:00'. Since the offset
        keys can't be reliably string-compared, we clear them and force
        a fresh import from Tibber.
        """
        hourly = self._data.get("hourly_records", {})
        has_offset_keys = any("+" in k or k.endswith("Z") for k in list(hourly.keys())[:10])
        if has_offset_keys and hourly:
            _LOGGER.warning(
                "Migrating storage: clearing %d records with timezone offsets, "
                "will re-import from Tibber",
                len(hourly),
            )
            self._data["hourly_records"] = {}
            self._data["quarter_hourly_prices"] = {}
            self._data["last_tibber_sync"] = None
            self._data["monthly_cache"] = {}

    async def async_save(self) -> None:
        """Save current data to storage."""
        await self._store.async_save(self._data)

    @property
    def hourly_records(self) -> dict[str, dict]:
        """Get all hourly energy records keyed by ISO timestamp."""
        return self._data.get("hourly_records", {})

    @property
    def quarter_hourly_prices(self) -> dict[str, dict]:
        """Get all quarter-hourly spot prices keyed by ISO timestamp."""
        return self._data.get("quarter_hourly_prices", {})

    @property
    def last_tibber_sync(self) -> str | None:
        """Get the timestamp of the last Tibber sync."""
        return self._data.get("last_tibber_sync")

    @last_tibber_sync.setter
    def last_tibber_sync(self, value: str | None) -> None:
        self._data["last_tibber_sync"] = value

    @property
    def monthly_cache(self) -> dict:
        """Get cached monthly aggregations."""
        return self._data.get("monthly_cache", {})

    @property
    def roi_projection(self) -> list[dict]:
        """Get cached ROI projection."""
        return self._data.get("roi_projection", [])

    @roi_projection.setter
    def roi_projection(self, value: list[dict]) -> None:
        self._data["roi_projection"] = value

    @property
    def last_sensor_readings(self) -> dict[str, float]:
        """Get last known cumulative sensor readings for delta computation."""
        return self._data.get("last_sensor_readings", {})

    @last_sensor_readings.setter
    def last_sensor_readings(self, value: dict[str, float]) -> None:
        self._data["last_sensor_readings"] = value

    def upsert_hourly_record(self, timestamp: str, record: dict) -> None:
        """Insert or update an hourly energy record."""
        if "hourly_records" not in self._data:
            self._data["hourly_records"] = {}
        self._data["hourly_records"][timestamp] = record

    def upsert_quarter_hourly_price(self, timestamp: str, price: dict) -> None:
        """Insert or update a quarter-hourly spot price."""
        if "quarter_hourly_prices" not in self._data:
            self._data["quarter_hourly_prices"] = {}
        self._data["quarter_hourly_prices"][timestamp] = price

    def get_records_for_period(self, start_iso: str, end_iso: str) -> list[dict]:
        """Get hourly records within a date range (inclusive start, exclusive end)."""
        records = []
        for ts, record in self.hourly_records.items():
            if start_iso <= ts < end_iso:
                records.append({**record, "timestamp": ts})
        return sorted(records, key=lambda r: r["timestamp"])

    def get_prices_for_date(self, date_iso: str) -> list[dict]:
        """Get all quarter-hourly prices for a given date (YYYY-MM-DD)."""
        prices = []
        for ts, price in self.quarter_hourly_prices.items():
            if ts.startswith(date_iso):
                prices.append({**price, "starts_at": ts})
        return sorted(prices, key=lambda p: p["starts_at"])

    def get_average_price_for_hour(self, hour_iso: str) -> float | None:
        """Get weighted average of 4 quarter-hourly prices for a given hour.

        hour_iso should be like "2025-06-15T14" (first 13 chars of ISO timestamp).
        """
        matching = []
        for ts, price in self.quarter_hourly_prices.items():
            if ts[:13] == hour_iso[:13]:
                matching.append(price.get("total", 0))
        if not matching:
            return None
        return sum(matching) / len(matching)

    def update_monthly_cache(self, month_key: str, stats: dict) -> None:
        """Update the cached monthly aggregation."""
        if "monthly_cache" not in self._data:
            self._data["monthly_cache"] = {}
        self._data["monthly_cache"][month_key] = stats

    def get_all_records_as_list(self) -> list[dict]:
        """Get all hourly records as a sorted list."""
        records = []
        for ts, record in self.hourly_records.items():
            records.append({**record, "timestamp": ts})
        return sorted(records, key=lambda r: r["timestamp"])

    async def async_remove(self) -> None:
        """Remove storage file."""
        await self._store.async_remove()
