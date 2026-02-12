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
    CONF_FIXED_PRICE,
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
    CONF_TRANSFER_FEE,
    CONF_USE_SPOT_PRICE,
    DOMAIN,
    UPDATE_INTERVAL_MINUTES,
)
from .financial_engine import (
    CalcParams,
    calculate_period,
    generate_monthly_report,
)
from .roi_engine import calculate_30_year_projection
from .storage import MySolarCellsStorage
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
        storage: MySolarCellsStorage,
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

        # Build calc params from config
        self._calc_params = CalcParams(
            tax_reduction=config_data.get(CONF_TAX_REDUCTION, 0.60),
            grid_compensation=config_data.get(CONF_GRID_COMPENSATION, 0.078),
            transfer_fee=config_data.get(CONF_TRANSFER_FEE, 0.30),
            energy_tax=config_data.get(CONF_ENERGY_TAX, 0.49),
            installed_kw=config_data.get(CONF_INSTALLED_KW, 10.5),
            use_spot_price=config_data.get(CONF_USE_SPOT_PRICE, True),
            fixed_price=config_data.get(CONF_FIXED_PRICE, 0.0),
        )

    @property
    def storage(self) -> MySolarCellsStorage:
        """Return the storage instance."""
        return self._storage

    async def _async_update_data(self) -> dict[str, Any]:
        """Fetch and process data."""
        try:
            now = datetime.now(tz=timezone.utc)

            # Initial historical import from Tibber
            if not self._initial_import_done:
                await self._do_initial_import()
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

    async def _do_initial_import(self) -> None:
        """Run initial historical import from Tibber."""
        last_sync = self._storage.last_tibber_sync
        if last_sync:
            start = _ensure_utc(datetime.fromisoformat(last_sync))
        else:
            install_date_str = self._config.get(CONF_INSTALLATION_DATE, "")
            if install_date_str:
                try:
                    start = _ensure_utc(datetime.fromisoformat(install_date_str))
                except (ValueError, TypeError):
                    start = datetime.now(tz=timezone.utc) - timedelta(days=365)
            else:
                start = datetime.now(tz=timezone.utc) - timedelta(days=365)

        _LOGGER.info("Starting historical import from %s", start.isoformat())

        home_id = self._config[CONF_HOME_ID]
        import_only_spot = self._config.get(CONF_IMPORT_ONLY_SPOT_PRICE, False)

        try:
            records = await self._client.get_consumption_production(
                home_id, start, import_only_spot
            )
            for record in records:
                ts = record.get("timestamp", "")
                if ts:
                    self._storage.upsert_hourly_record(ts, record)

            self._storage.last_tibber_sync = datetime.now(tz=timezone.utc).isoformat()
            await self._storage.async_save()
            _LOGGER.info("Historical import complete: %d records", len(records))
        except TibberApiError as err:
            _LOGGER.error("Historical import failed: %s", err)

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

            # Enrich with HA sensor data for own use calculation
            for record in records:
                self._enrich_record_from_ha_sensors(record)
                ts = record.get("timestamp", "")
                if ts:
                    # Only update if not already set with richer data
                    existing = self._storage.hourly_records.get(ts)
                    if not existing or not existing.get("synced"):
                        self._storage.upsert_hourly_record(ts, record)

            self._storage.last_tibber_sync = datetime.now(tz=timezone.utc).isoformat()
            await self._storage.async_save()
        except TibberApiError as err:
            _LOGGER.warning("Failed to update consumption/production: %s", err)

    def _enrich_record_from_ha_sensors(self, record: dict) -> None:
        """Enrich a Tibber record with HA sensor data for own_use calculation.

        Tibber provides grid_export (production_sold) and grid_import (purchased).
        If only a production sensor is configured (no separate grid export sensor),
        own_use is calculated as: total_production - tibber_production_sold.

        If a grid export sensor IS configured, it overrides Tibber's production_sold
        and own_use = total_production - grid_export_sensor.
        """
        production_sensor = self._config.get(CONF_PRODUCTION_SENSOR)
        export_sensor = self._config.get(CONF_GRID_EXPORT_SENSOR)
        import_sensor = self._config.get(CONF_GRID_IMPORT_SENSOR)
        battery_charge_sensor = self._config.get(CONF_BATTERY_CHARGE_SENSOR)
        battery_discharge_sensor = self._config.get(CONF_BATTERY_DISCHARGE_SENSOR)

        if production_sensor:
            prod_state = self.hass.states.get(production_sensor)
            if prod_state and prod_state.state not in ("unknown", "unavailable"):
                try:
                    total_prod = float(prod_state.state)

                    # Determine grid export: prefer HA sensor, fall back to Tibber data
                    if export_sensor:
                        export_state = self.hass.states.get(export_sensor)
                        if export_state and export_state.state not in ("unknown", "unavailable"):
                            grid_export = float(export_state.state)
                        else:
                            grid_export = record.get("production_sold", 0)
                    else:
                        # Use Tibber's production_sold as grid export
                        grid_export = record.get("production_sold", 0)

                    own_use = max(0, total_prod - grid_export)
                    record["production_own_use"] = own_use

                    # Calculate own use profit based on buy price
                    if self._calc_params.use_spot_price:
                        record["production_own_use_profit"] = own_use * record.get("unit_price_buy", 0)
                    else:
                        record["production_own_use_profit"] = own_use * self._calc_params.fixed_price
                except (ValueError, TypeError):
                    pass

        # Override grid import from HA sensor if configured
        if import_sensor:
            import_state = self.hass.states.get(import_sensor)
            if import_state and import_state.state not in ("unknown", "unavailable"):
                try:
                    purchased = float(import_state.state)
                    record["purchased"] = purchased
                    if self._calc_params.use_spot_price:
                        record["purchased_cost"] = purchased * record.get("unit_price_buy", 0)
                    else:
                        record["purchased_cost"] = purchased * self._calc_params.fixed_price
                except (ValueError, TypeError):
                    pass

        if battery_charge_sensor:
            state = self.hass.states.get(battery_charge_sensor)
            if state:
                try:
                    record["battery_charge"] = float(state.state)
                except (ValueError, TypeError):
                    pass

        if battery_discharge_sensor:
            state = self.hass.states.get(battery_discharge_sensor)
            if state:
                try:
                    battery_used = float(state.state)
                    record["battery_used"] = battery_used
                    if self._calc_params.use_spot_price:
                        record["battery_used_profit"] = battery_used * record.get("unit_price_buy", 0)
                    else:
                        record["battery_used_profit"] = battery_used * self._calc_params.fixed_price
                except (ValueError, TypeError):
                    pass

    async def _calculate_sensor_data(self) -> dict[str, Any]:
        """Calculate all sensor values from stored data."""
        now = datetime.now()
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
        daily_stats = calculate_period(daily_records, self._calc_params)
        monthly_stats = calculate_period(monthly_records, self._calc_params)
        yearly_stats = calculate_period(yearly_records, self._calc_params)
        lifetime_stats = calculate_period(all_records, self._calc_params)

        # Generate yearly overview for ROI
        investment = self._config.get(CONF_INVESTMENT_AMOUNT, 0) + self._config.get(CONF_LOAN_AMOUNT, 0)
        yearly_overview, monthly_by_year = generate_monthly_report(
            all_records, self._calc_params, investment
        )

        # Calculate ROI projection
        install_date_str = self._config.get(CONF_INSTALLATION_DATE, "")
        first_prod_day = None
        if install_date_str:
            try:
                first_prod_day = _ensure_utc(datetime.fromisoformat(install_date_str))
            except (ValueError, TypeError):
                pass

        roi_projection = calculate_30_year_projection(
            yearly_overview,
            monthly_by_year,
            price_development=self._config.get(CONF_PRICE_DEVELOPMENT, 1.05),
            panel_degradation=self._config.get(CONF_PANEL_DEGRADATION, 0.25),
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
