"""Data update coordinator for My Solar Cells."""

from __future__ import annotations

import logging
from datetime import datetime, timedelta, timezone
from typing import Any

import aiohttp

from homeassistant.core import HomeAssistant
from homeassistant.helpers.update_coordinator import DataUpdateCoordinator, UpdateFailed

from .const import (
    CONF_API_KEY,
    CONF_BATTERY_CHARGE_SENSOR,
    CONF_BATTERY_DISCHARGE_SENSOR,
    CONF_ENERGY_TAX,
    CONF_GRID_COMPENSATION,
    CONF_GRID_EXPORT_SENSOR,
    CONF_GRID_IMPORT_SENSOR,
    CONF_HOME_ID,
    CONF_IMPORT_ONLY_SPOT_PRICE,
    CONF_INSTALLATION_DATE,
    CONF_INSTALLED_KW,
    CONF_INVESTMENT_AMOUNT,
    CONF_LOAN_AMOUNT,
    CONF_PANEL_DEGRADATION,
    CONF_PRICE_DEVELOPMENT,
    CONF_PRODUCTION_SENSOR,
    CONF_TAX_REDUCTION,
    CONF_TIBBER_START_YEAR,
    CONF_TRANSFER_FEE,
    CONF_YEARLY_PARAMS,
    DOMAIN,
    UPDATE_INTERVAL_MINUTES,
)
from .financial_engine import (
    CalcParams,
    calculate_period,
    generate_monthly_report,
)
from .roi_engine import calculate_30_year_projection
from .database import MySolarCellsDatabase
from .tibber_client import TibberApiError, TibberClient

_LOGGER = logging.getLogger(__name__)


def _ensure_utc(dt: datetime) -> datetime:
    """Ensure a datetime is timezone-aware (UTC)."""
    if dt.tzinfo is None:
        return dt.replace(tzinfo=timezone.utc)
    return dt


class MySolarCellsCoordinator(DataUpdateCoordinator[dict[str, Any]]):
    """Coordinator to manage data updates for My Solar Cells."""

    def __init__(
        self,
        hass: HomeAssistant,
        session: aiohttp.ClientSession,
        storage: MySolarCellsDatabase,
        config_data: dict[str, Any],
        entry_id: str,
    ) -> None:
        """Initialize the coordinator."""
        super().__init__(
            hass,
            _LOGGER,
            name=DOMAIN,
            update_interval=timedelta(minutes=UPDATE_INTERVAL_MINUTES),
        )
        self._session = session
        self._storage = storage
        self._config = config_data
        self._entry_id = entry_id
        self._client = TibberClient(session, config_data[CONF_API_KEY])
        self._last_hourly_update: datetime | None = None
        self._initial_import_done = False
        self._statistics_import_done = False
        self._last_sensor_readings: dict[str, float] = {}

        # Per-year financial parameter overrides — loaded from database
        self._yearly_params = storage.get_all_yearly_params() or None

        # Build calc params from config
        self._calc_params = CalcParams(
            tax_reduction=config_data.get(CONF_TAX_REDUCTION, 0.60),
            grid_compensation=config_data.get(CONF_GRID_COMPENSATION, 0.078),
            transfer_fee=config_data.get(CONF_TRANSFER_FEE, 0.30),
            energy_tax=config_data.get(CONF_ENERGY_TAX, 0.49),
            installed_kw=config_data.get(CONF_INSTALLED_KW, 10.5),
        )

    @property
    def storage(self) -> MySolarCellsDatabase:
        """Return the storage instance."""
        return self._storage

    async def _async_update_data(self) -> dict[str, Any]:
        """Fetch and process data."""
        try:
            now = datetime.now(tz=timezone.utc)

            # Initial historical import from Tibber
            if not self._initial_import_done:
                await self._do_initial_import()
                await self._backfill_production_own_use()
                self._initial_import_done = True

            # Always fetch spot prices (every 15 min)
            await self._update_spot_prices()

            # Hourly update for consumption/production
            should_hourly_update = (
                self._last_hourly_update is None
                or (now - self._last_hourly_update).total_seconds() >= 3600
            )
            if should_hourly_update:
                await self._update_consumption_production()
                self._last_hourly_update = now

            # Calculate all sensor values
            return await self._calculate_sensor_data()

        except TibberApiError as err:
            raise UpdateFailed(f"Tibber API error: {err}") from err
        except Exception as err:
            _LOGGER.exception("Unexpected error updating My Solar Cells data")
            raise UpdateFailed(f"Update failed: {err}") from err

    def _get_configured_start_date(self) -> datetime:
        """Get the configured start date for Tibber imports."""
        tibber_start_year = self._config.get(CONF_TIBBER_START_YEAR)
        if tibber_start_year:
            try:
                return datetime(int(tibber_start_year), 1, 1, tzinfo=timezone.utc)
            except (ValueError, TypeError):
                pass

        install_date_str = self._config.get(CONF_INSTALLATION_DATE, "")
        if install_date_str:
            try:
                return _ensure_utc(datetime.fromisoformat(install_date_str))
            except (ValueError, TypeError):
                pass

        return datetime.now(tz=timezone.utc) - timedelta(days=365)

    async def _do_initial_import(self) -> None:
        """Run initial historical import from Tibber.

        Uses gap detection: if the database has far fewer records than
        expected for the time span, re-imports from the configured start
        date to fill gaps.  Otherwise continues from last_tibber_sync.
        """
        configured_start = self._get_configured_start_date()
        last_sync = self._storage.last_tibber_sync

        if last_sync:
            sync_dt = _ensure_utc(datetime.fromisoformat(last_sync))
            record_count = self._storage.get_hourly_record_count()
            expected_hours = max(1, (sync_dt - configured_start).total_seconds() / 3600)
            coverage = record_count / expected_hours

            if coverage >= 0.5:
                start = sync_dt
                _LOGGER.info(
                    "Continuing import from last sync %s (coverage %.0f%%)",
                    start.isoformat(), coverage * 100,
                )
            else:
                start = configured_start
                _LOGGER.warning(
                    "Low data coverage (%.0f%%, %d records for ~%.0f expected hours). "
                    "Re-importing from %s",
                    coverage * 100, record_count, expected_hours,
                    start.isoformat(),
                )
        else:
            start = configured_start
            _LOGGER.info("First import from %s", start.isoformat())

        home_id = self._config[CONF_HOME_ID]
        import_only_spot = self._config.get(CONF_IMPORT_ONLY_SPOT_PRICE, False)

        try:
            records = await self._client.get_consumption_production(
                home_id, start, import_only_spot
            )

            stored_count = 0
            for record in records:
                ts = record.get("timestamp", "")
                if not ts:
                    continue
                # Don't overwrite existing synced records
                existing = self._storage.get_hourly_record(ts)
                if existing and existing.get("synced"):
                    continue
                # Enrich price_level from spot data
                if not record.get("price_level"):
                    level = self._storage.get_price_level_for_hour(ts)
                    if level:
                        record["price_level"] = level
                self._storage.upsert_hourly_record(ts, record)
                stored_count += 1

            # Only advance sync marker to the latest record timestamp
            if records:
                latest_ts = max(
                    (r.get("timestamp", "") for r in records if r.get("timestamp")),
                    default="",
                )
                if latest_ts:
                    self._storage.last_tibber_sync = latest_ts

            await self._storage.async_save()
            _LOGGER.info(
                "Historical import complete: %d from Tibber, %d new stored",
                len(records), stored_count,
            )
        except TibberApiError as err:
            _LOGGER.error("Historical import failed: %s", err)

    async def _backfill_production_own_use(self) -> None:
        """Backfill production_own_use for unenriched records using HA recorder statistics.

        Queries the recorder for hourly 'change' statistics of the production
        sensor, then calculates own_use = production_delta - production_sold
        for each unenriched hourly_energy record.
        """
        production_entity = self._config.get(CONF_PRODUCTION_SENSOR)
        if not production_entity:
            _LOGGER.debug("No production sensor configured, skipping backfill")
            return

        unenriched = await self.hass.async_add_executor_job(
            self._storage.get_unenriched_records
        )
        if not unenriched:
            _LOGGER.debug("No unenriched records to backfill")
            return

        _LOGGER.info(
            "Backfilling production_own_use for %d unenriched records using recorder",
            len(unenriched),
        )

        try:
            from homeassistant.components.recorder.statistics import (
                statistics_during_period,
            )
        except ImportError:
            _LOGGER.warning("Recorder statistics not available, skipping backfill")
            return

        # Determine time range from unenriched records
        first_ts = unenriched[0]["timestamp"]
        last_ts = unenriched[-1]["timestamp"]
        try:
            start_dt = _ensure_utc(datetime.fromisoformat(first_ts))
            end_dt = _ensure_utc(datetime.fromisoformat(last_ts)) + timedelta(hours=1)
        except (ValueError, TypeError):
            _LOGGER.warning("Could not parse unenriched timestamps, skipping backfill")
            return

        try:
            stats = await self.hass.async_add_executor_job(
                statistics_during_period,
                self.hass,
                start_dt,
                end_dt,
                {production_entity},
                "hour",
                None,
                {"change"},
            )
        except Exception:
            _LOGGER.exception("Failed to query recorder statistics for backfill")
            return

        stat_rows = stats.get(production_entity, [])
        if not stat_rows:
            _LOGGER.info("No recorder statistics found for %s", production_entity)
            return

        # Build lookup: truncated ISO hour → change value
        hour_deltas: dict[str, float] = {}
        for row in stat_rows:
            change = row.get("change")
            if change is None:
                continue
            # row["start"] is epoch seconds (float)
            row_dt = datetime.fromtimestamp(row["start"], tz=timezone.utc)
            hour_key = row_dt.strftime("%Y-%m-%dT%H")
            hour_deltas[hour_key] = change

        enriched_count = 0
        for record in unenriched:
            ts = record["timestamp"]
            hour_key = ts[:13]  # "2025-06-15T14"
            production_delta = hour_deltas.get(hour_key)
            if production_delta is None:
                continue

            grid_export = record.get("production_sold", 0)
            own_use = max(0, production_delta - grid_export)
            record["production_own_use"] = own_use
            record["production_own_use_profit"] = (
                own_use * record.get("unit_price_buy", 0)
            )
            record["sensor_enriched"] = 1
            self._storage.upsert_hourly_record(ts, record)
            enriched_count += 1

        if enriched_count:
            await self._storage.async_save()
            _LOGGER.info(
                "Backfill complete: enriched %d of %d records with production_own_use",
                enriched_count, len(unenriched),
            )

    async def _update_spot_prices(self) -> None:
        """Fetch today + tomorrow spot prices at quarter-hourly resolution."""
        home_id = self._config[CONF_HOME_ID]
        try:
            prices = await self._client.get_spot_prices(home_id)
            for price in prices.get("today", []):
                ts = price.get("starts_at", "")
                if ts:
                    self._storage.upsert_quarter_hourly_price(ts, price)
            for price in prices.get("tomorrow", []):
                ts = price.get("starts_at", "")
                if ts:
                    self._storage.upsert_quarter_hourly_price(ts, price)
            await self._storage.async_save()
        except TibberApiError as err:
            _LOGGER.warning("Failed to update spot prices: %s", err)

    async def _update_consumption_production(self) -> None:
        """Fetch recent consumption/production from Tibber."""
        last_sync = self._storage.last_tibber_sync
        if last_sync:
            start = _ensure_utc(datetime.fromisoformat(last_sync))
        else:
            start = datetime.now(tz=timezone.utc) - timedelta(days=1)

        home_id = self._config[CONF_HOME_ID]
        import_only_spot = self._config.get(CONF_IMPORT_ONLY_SPOT_PRICE, False)

        try:
            records = await self._client.get_consumption_production(
                home_id, start, import_only_spot
            )

            # Store base Tibber records
            for record in records:
                ts = record.get("timestamp", "")
                if ts:
                    existing = self._storage.get_hourly_record(ts)
                    if not existing or not existing.get("synced"):
                        # Enrich price_level from spot data
                        if not record.get("price_level"):
                            level = self._storage.get_price_level_for_hour(ts)
                            if level:
                                record["price_level"] = level
                        self._storage.upsert_hourly_record(ts, record)

            # Compute sensor deltas and enrich the most recent record
            sensor_deltas = self._compute_sensor_deltas()
            if sensor_deltas and records:
                most_recent = max(records, key=lambda r: r.get("timestamp", ""))
                ts = most_recent.get("timestamp", "")
                if ts:
                    stored = self._storage.get_hourly_record(ts) or most_recent
                    self._enrich_record_with_deltas(stored, sensor_deltas)
                    stored["sensor_enriched"] = 1
                    self._storage.upsert_hourly_record(ts, stored)

            # Only advance sync marker to latest record, not to now
            if records:
                latest_ts = max(
                    (r.get("timestamp", "") for r in records if r.get("timestamp")),
                    default="",
                )
                if latest_ts:
                    self._storage.last_tibber_sync = latest_ts
            await self._storage.async_save()
        except TibberApiError as err:
            _LOGGER.warning("Failed to update consumption/production: %s", err)

    def _compute_sensor_deltas(self) -> dict[str, float]:
        """Compute hourly deltas from cumulative HA sensor readings.

        On first call after restart, loads last known readings from storage.
        Returns a dict of deltas keyed by sensor role (e.g. "production", "grid_export").
        """
        # Load from persistent storage on first call
        if not self._last_sensor_readings:
            stored = self._storage.last_sensor_readings
            if stored:
                self._last_sensor_readings = dict(stored)

        sensor_map = {
            "production": self._config.get(CONF_PRODUCTION_SENSOR),
            "grid_export": self._config.get(CONF_GRID_EXPORT_SENSOR),
            "grid_import": self._config.get(CONF_GRID_IMPORT_SENSOR),
            "battery_charge": self._config.get(CONF_BATTERY_CHARGE_SENSOR),
            "battery_discharge": self._config.get(CONF_BATTERY_DISCHARGE_SENSOR),
        }

        deltas: dict[str, float] = {}

        for role, entity_id in sensor_map.items():
            if not entity_id:
                continue

            state_obj = self.hass.states.get(entity_id)
            if not state_obj or state_obj.state in ("unknown", "unavailable"):
                continue

            try:
                current = float(state_obj.state)
            except (ValueError, TypeError):
                continue

            previous = self._last_sensor_readings.get(role)
            self._last_sensor_readings[role] = current

            if previous is None:
                # First reading — store baseline, no delta
                _LOGGER.debug(
                    "Sensor %s (%s): first reading %.3f, storing baseline",
                    role, entity_id, current,
                )
                continue

            delta = current - previous
            if delta < 0:
                # Sensor reset (e.g. counter wrapped or was cleared)
                _LOGGER.debug(
                    "Sensor %s (%s): negative delta %.3f (reset?), skipping",
                    role, entity_id, delta,
                )
                continue

            deltas[role] = delta

        # Persist for restart recovery
        self._storage.last_sensor_readings = dict(self._last_sensor_readings)

        return deltas

    def _enrich_record_with_deltas(
        self, record: dict, deltas: dict[str, float]
    ) -> None:
        """Enrich a Tibber record with pre-computed sensor deltas.

        Uses delta values (hourly changes) instead of raw cumulative sensor states.
        """
        production_delta = deltas.get("production")
        grid_export_delta = deltas.get("grid_export")
        grid_import_delta = deltas.get("grid_import")
        battery_charge_delta = deltas.get("battery_charge")
        battery_discharge_delta = deltas.get("battery_discharge")

        # Override grid export if HA sensor delta is available
        if grid_export_delta is not None:
            record["production_sold"] = grid_export_delta
            record["production_sold_profit"] = (
                grid_export_delta * record.get("unit_price_sold", 0)
            )

        # Calculate own use from production delta
        if production_delta is not None:
            grid_export = (
                grid_export_delta
                if grid_export_delta is not None
                else record.get("production_sold", 0)
            )
            own_use = max(0, production_delta - grid_export)
            record["production_own_use"] = own_use
            record["production_own_use_profit"] = (
                own_use * record.get("unit_price_buy", 0)
            )

        # Override grid import if HA sensor delta is available
        if grid_import_delta is not None:
            record["purchased"] = grid_import_delta
            record["purchased_cost"] = (
                grid_import_delta * record.get("unit_price_buy", 0)
            )

        # Battery sensors
        if battery_charge_delta is not None:
            record["battery_charge"] = battery_charge_delta

        if battery_discharge_delta is not None:
            record["battery_used"] = battery_discharge_delta
            record["battery_used_profit"] = (
                battery_discharge_delta * record.get("unit_price_buy", 0)
            )

    async def _calculate_sensor_data(self) -> dict[str, Any]:
        """Calculate all sensor values from stored data."""
        now = datetime.now(tz=timezone.utc)
        today_str = now.strftime("%Y-%m-%d")
        month_start_str = now.strftime("%Y-%m-01")
        year_start_str = now.strftime("%Y-01-01")
        tomorrow_str = (now + timedelta(days=1)).strftime("%Y-%m-%d")

        # Get records for different periods
        daily_records = self._storage.get_records_for_period(
            f"{today_str}T00:00:00", f"{tomorrow_str}T00:00:00"
        )

        # Next month start for monthly range
        if now.month == 12:
            next_month = datetime(now.year + 1, 1, 1)
        else:
            next_month = datetime(now.year, now.month + 1, 1)
        monthly_records = self._storage.get_records_for_period(
            f"{month_start_str}T00:00:00", next_month.strftime("%Y-%m-%dT00:00:00")
        )

        next_year = datetime(now.year + 1, 1, 1)
        yearly_records = self._storage.get_records_for_period(
            f"{year_start_str}T00:00:00", next_year.strftime("%Y-%m-%dT00:00:00")
        )

        all_records = self._storage.get_all_records_as_list()

        # Calculate stats for each period
        daily_stats = calculate_period(daily_records, self._calc_params, self._yearly_params)
        monthly_stats = calculate_period(monthly_records, self._calc_params, self._yearly_params)
        yearly_stats = calculate_period(yearly_records, self._calc_params, self._yearly_params)
        lifetime_stats = calculate_period(all_records, self._calc_params, self._yearly_params)

        # Generate yearly overview for ROI
        investment = self._config.get(CONF_INVESTMENT_AMOUNT, 0) + self._config.get(CONF_LOAN_AMOUNT, 0)
        yearly_overview, monthly_by_year = generate_monthly_report(
            all_records, self._calc_params, investment, self._yearly_params
        )

        # Calculate ROI projection
        install_date_str = self._config.get(CONF_INSTALLATION_DATE, "")
        first_prod_day = None
        if install_date_str:
            try:
                first_prod_day = _ensure_utc(datetime.fromisoformat(install_date_str))
            except (ValueError, TypeError):
                pass

        # Check for user-customized ROI params (persisted from panel UI)
        saved_roi_params = self._storage.last_roi_params
        roi_price_dev = saved_roi_params.get(
            "price_development",
            self._config.get(CONF_PRICE_DEVELOPMENT, 1.05),
        )
        roi_panel_deg = saved_roi_params.get(
            "panel_degradation",
            self._config.get(CONF_PANEL_DEGRADATION, 0.25),
        )

        roi_projection = calculate_30_year_projection(
            yearly_overview,
            monthly_by_year,
            price_development=roi_price_dev,
            panel_degradation=roi_panel_deg,
            investment=investment,
            installed_kw=self._config.get(CONF_INSTALLED_KW, 10.5),
            first_production_day=first_prod_day,
        )

        # Cache ROI projection
        self._storage.roi_projection = [r.to_dict() for r in roi_projection]

        # Find payback year
        payback_year = None
        for entry in roi_projection:
            if entry.is_roi_year:
                payback_year = entry.year
                break

        # Spot prices
        today_prices = self._storage.get_prices_for_date(today_str)
        tomorrow_prices = self._storage.get_prices_for_date(tomorrow_str)

        # Current spot price (find the active 15-min slot)
        current_spot_price = 0.0
        current_price_level = ""
        now_iso = now.isoformat()
        for price in reversed(today_prices):
            if price.get("starts_at", "") <= now_iso:
                current_spot_price = price.get("total", 0)
                current_price_level = price.get("level", "")
                break

        # Average spot price today
        avg_spot_today = 0.0
        if today_prices:
            avg_spot_today = round(
                sum(p.get("total", 0) for p in today_prices) / len(today_prices), 4
            )

        # Total savings to date
        total_savings = lifetime_stats.sum_all_production_sold_and_saved
        remaining = round(investment - total_savings, 0) if investment > 0 else 0

        return {
            # Daily
            "daily_production_sold": daily_stats.production_sold,
            "daily_production_own_use": daily_stats.production_own_use,
            "daily_purchased": daily_stats.purchased,
            "daily_sold_profit": daily_stats.sum_production_sold_profit,
            "daily_own_use_saved": daily_stats.sum_production_own_use_saved,
            "daily_purchased_cost": daily_stats.sum_purchased_cost,
            "daily_balance": daily_stats.balance,
            # Monthly
            "monthly_production_sold": monthly_stats.production_sold,
            "monthly_production_own_use": monthly_stats.production_own_use,
            "monthly_purchased": monthly_stats.purchased,
            "monthly_sold_profit": monthly_stats.sum_production_sold_profit,
            "monthly_own_use_saved": monthly_stats.sum_production_own_use_saved,
            "monthly_purchased_cost": monthly_stats.sum_purchased_cost,
            "monthly_balance": monthly_stats.balance,
            "monthly_tax_reduction": monthly_stats.production_sold_tax_reduction_profit,
            "monthly_grid_compensation": monthly_stats.production_sold_grid_compensation_profit,
            # Yearly
            "yearly_total_savings": yearly_stats.sum_all_production_sold_and_saved,
            "yearly_return_percentage": (
                round((yearly_stats.sum_all_production_sold_and_saved / investment) * 100, 1)
                if investment > 0 else 0
            ),
            # Lifetime / ROI
            "total_savings_to_date": total_savings,
            "remaining_on_investment": remaining,
            "roi_payback_year": payback_year,
            "investment_amount": investment,
            "roi_projection": [r.to_dict() for r in roi_projection],
            # Spot prices
            "current_spot_price": current_spot_price,
            "current_price_level": current_price_level,
            "avg_spot_price_today": avg_spot_today,
            "spot_prices_today": today_prices,
            "spot_prices_tomorrow": tomorrow_prices,
        }
