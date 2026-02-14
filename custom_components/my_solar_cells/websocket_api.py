"""WebSocket API for My Solar Cells panel."""

from __future__ import annotations

import logging
from datetime import datetime, timedelta
from typing import Any

import voluptuous as vol

from homeassistant.components import websocket_api
from homeassistant.core import HomeAssistant, callback

from .const import (
    CONF_BATTERY_CHARGE_SENSOR,
    CONF_BATTERY_DISCHARGE_SENSOR,
    CONF_GRID_EXPORT_SENSOR,
    CONF_GRID_IMPORT_SENSOR,
    CONF_INSTALLED_KW,
    CONF_INSTALLATION_DATE,
    CONF_INVESTMENT_AMOUNT,
    CONF_LOAN_AMOUNT,
    CONF_PANEL_DEGRADATION,
    CONF_PRICE_DEVELOPMENT,
    CONF_PRODUCTION_SENSOR,
    DEFAULT_INSTALLED_KW,
    DEFAULT_INVESTMENT_AMOUNT,
    DEFAULT_PANEL_DEGRADATION,
    DEFAULT_PRICE_DEVELOPMENT,
    DOMAIN,
)
from .financial_engine import generate_monthly_report
from .roi_engine import calculate_30_year_projection

_LOGGER = logging.getLogger(__name__)

WS_REGISTERED_KEY = f"{DOMAIN}_ws_registered"


@callback
def async_register_websocket_commands(hass: HomeAssistant) -> None:
    """Register WebSocket commands (idempotent)."""
    if hass.data.get(WS_REGISTERED_KEY):
        return
    hass.data[WS_REGISTERED_KEY] = True

    websocket_api.async_register_command(hass, ws_get_overview)
    websocket_api.async_register_command(hass, ws_get_hourly_energy)
    websocket_api.async_register_command(hass, ws_get_yearly_params)
    websocket_api.async_register_command(hass, ws_set_yearly_params)
    websocket_api.async_register_command(hass, ws_delete_yearly_params)
    websocket_api.async_register_command(hass, ws_get_sensor_config)
    websocket_api.async_register_command(hass, ws_get_period_summaries)
    websocket_api.async_register_command(hass, ws_get_roi_projection)


def _get_coordinator(hass: HomeAssistant, entry_id: str):
    """Get the coordinator for an entry."""
    return hass.data.get(DOMAIN, {}).get(entry_id)


def _get_database(hass: HomeAssistant, entry_id: str):
    """Get the database instance for an entry."""
    coordinator = _get_coordinator(hass, entry_id)
    if coordinator is None:
        return None
    return coordinator.storage


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_overview",
        vol.Required("entry_id"): str,
    }
)
@websocket_api.async_response
async def ws_get_overview(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return overview data for the panel."""
    entry_id = msg["entry_id"]
    db = _get_database(hass, entry_id)
    if db is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    def _fetch():
        return {
            "last_tibber_sync": db.last_tibber_sync,
            "hourly_record_count": db.get_hourly_record_count(),
            "first_timestamp": db.get_first_hourly_timestamp(),
            "last_timestamp": db.get_last_hourly_timestamp(),
            "yearly_params": db.get_all_yearly_params(),
        }

    result = await hass.async_add_executor_job(_fetch)
    connection.send_result(msg["id"], result)


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_hourly_energy",
        vol.Required("entry_id"): str,
        vol.Required("start_date"): str,
        vol.Required("end_date"): str,
        vol.Optional("offset", default=0): int,
        vol.Optional("limit", default=50): int,
    }
)
@websocket_api.async_response
async def ws_get_hourly_energy(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return paginated hourly energy records."""
    entry_id = msg["entry_id"]
    db = _get_database(hass, entry_id)
    if db is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    start = msg["start_date"]
    end = msg["end_date"]
    offset = msg["offset"]
    limit = min(msg["limit"], 200)  # cap at 200

    def _fetch():
        records, total = db.get_records_for_period_paginated(start, end, offset, limit)
        return {"records": records, "total_count": total}

    result = await hass.async_add_executor_job(_fetch)
    connection.send_result(msg["id"], result)


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_yearly_params",
        vol.Required("entry_id"): str,
    }
)
@websocket_api.async_response
async def ws_get_yearly_params(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return all yearly financial parameters."""
    entry_id = msg["entry_id"]
    db = _get_database(hass, entry_id)
    if db is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    def _fetch():
        return {
            "yearly_params": db.get_all_yearly_params(),
            "first_timestamp": db.get_first_hourly_timestamp(),
            "last_timestamp": db.get_last_hourly_timestamp(),
        }

    result = await hass.async_add_executor_job(_fetch)
    connection.send_result(msg["id"], result)


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/set_yearly_params",
        vol.Required("entry_id"): str,
        vol.Required("year"): int,
        vol.Required("tax_reduction"): vol.Coerce(float),
        vol.Required("grid_compensation"): vol.Coerce(float),
        vol.Required("transfer_fee"): vol.Coerce(float),
        vol.Required("energy_tax"): vol.Coerce(float),
        vol.Required("installed_kw"): vol.Coerce(float),
    }
)
@websocket_api.async_response
async def ws_set_yearly_params(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Save financial parameters for a specific year."""
    entry_id = msg["entry_id"]
    coordinator = _get_coordinator(hass, entry_id)
    if coordinator is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    db = coordinator.storage
    params = {
        "tax_reduction": msg["tax_reduction"],
        "grid_compensation": msg["grid_compensation"],
        "transfer_fee": msg["transfer_fee"],
        "energy_tax": msg["energy_tax"],
        "installed_kw": msg["installed_kw"],
    }

    def _save():
        db.set_yearly_params(msg["year"], params)

    await hass.async_add_executor_job(_save)
    await db.async_save()
    coordinator._yearly_params = await hass.async_add_executor_job(db.get_all_yearly_params)
    await coordinator.async_request_refresh()
    connection.send_result(msg["id"], {"success": True})


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/delete_yearly_params",
        vol.Required("entry_id"): str,
        vol.Required("year"): int,
    }
)
@websocket_api.async_response
async def ws_delete_yearly_params(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Delete financial parameters for a specific year."""
    entry_id = msg["entry_id"]
    coordinator = _get_coordinator(hass, entry_id)
    if coordinator is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    db = coordinator.storage

    def _delete():
        db.delete_yearly_params(msg["year"])

    await hass.async_add_executor_job(_delete)
    await db.async_save()
    coordinator._yearly_params = await hass.async_add_executor_job(db.get_all_yearly_params)
    await coordinator.async_request_refresh()
    connection.send_result(msg["id"], {"success": True})


_SENSOR_ROLES = [
    ("production", CONF_PRODUCTION_SENSOR, "Total solar production"),
    ("grid_export", CONF_GRID_EXPORT_SENSOR, "Grid export (sold)"),
    ("grid_import", CONF_GRID_IMPORT_SENSOR, "Grid import (purchased)"),
    ("battery_charge", CONF_BATTERY_CHARGE_SENSOR, "Battery charge"),
    ("battery_discharge", CONF_BATTERY_DISCHARGE_SENSOR, "Battery discharge"),
]


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_sensor_config",
        vol.Required("entry_id"): str,
    }
)
@websocket_api.async_response
async def ws_get_sensor_config(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return configured sensor entity IDs, current states, and last stored readings."""
    entry_id = msg["entry_id"]
    coordinator = _get_coordinator(hass, entry_id)
    if coordinator is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    config = coordinator._config
    last_readings = await hass.async_add_executor_job(
        lambda: coordinator.storage.last_sensor_readings
    )

    sensors = []
    for role, conf_key, description in _SENSOR_ROLES:
        entity_id = config.get(conf_key, "")
        current_state = None
        if entity_id:
            state_obj = hass.states.get(entity_id)
            if state_obj:
                current_state = state_obj.state

        sensors.append({
            "role": role,
            "description": description,
            "entity_id": entity_id or None,
            "current_state": current_state,
            "last_stored_reading": last_readings.get(role),
        })

    connection.send_result(msg["id"], {"sensors": sensors})


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_period_summaries",
        vol.Required("entry_id"): str,
    }
)
@websocket_api.async_response
async def ws_get_period_summaries(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return energy summaries for today, this week, this month, this year."""
    entry_id = msg["entry_id"]
    db = _get_database(hass, entry_id)
    if db is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    def _fetch():
        now = datetime.now()
        today_start = now.replace(hour=0, minute=0, second=0, microsecond=0)
        # Monday of current week
        week_start = today_start - timedelta(days=today_start.weekday())
        month_start = today_start.replace(day=1)
        year_start = today_start.replace(month=1, day=1)
        # End is tomorrow start to include all of today
        end = today_start + timedelta(days=1)

        end_iso = end.isoformat()
        return {
            "today": db.get_period_summary(today_start.isoformat(), end_iso),
            "this_week": db.get_period_summary(week_start.isoformat(), end_iso),
            "this_month": db.get_period_summary(month_start.isoformat(), end_iso),
            "this_year": db.get_period_summary(year_start.isoformat(), end_iso),
        }

    result = await hass.async_add_executor_job(_fetch)
    connection.send_result(msg["id"], result)


@websocket_api.websocket_command(
    {
        vol.Required("type"): f"{DOMAIN}/get_roi_projection",
        vol.Required("entry_id"): str,
        vol.Optional("price_development"): vol.Coerce(float),
        vol.Optional("panel_degradation"): vol.Coerce(float),
    }
)
@websocket_api.async_response
async def ws_get_roi_projection(
    hass: HomeAssistant, connection: websocket_api.ActiveConnection, msg: dict[str, Any]
) -> None:
    """Return the 30-year ROI projection data."""
    entry_id = msg["entry_id"]
    coordinator = _get_coordinator(hass, entry_id)
    if coordinator is None:
        connection.send_error(msg["id"], "not_found", "Entry not found")
        return

    config = coordinator._config
    configured_price_dev = config.get(CONF_PRICE_DEVELOPMENT, DEFAULT_PRICE_DEVELOPMENT)
    configured_panel_deg = config.get(CONF_PANEL_DEGRADATION, DEFAULT_PANEL_DEGRADATION)

    custom_price_dev = msg.get("price_development")
    custom_panel_deg = msg.get("panel_degradation")
    has_custom = custom_price_dev is not None or custom_panel_deg is not None

    if has_custom:
        price_dev = custom_price_dev if custom_price_dev is not None else configured_price_dev
        panel_deg = custom_panel_deg if custom_panel_deg is not None else configured_panel_deg
        investment = config.get(CONF_INVESTMENT_AMOUNT, 0) + config.get(CONF_LOAN_AMOUNT, 0)
        installed_kw = config.get(CONF_INSTALLED_KW, DEFAULT_INSTALLED_KW)
        install_date_str = config.get(CONF_INSTALLATION_DATE, "")
        first_prod_day = None
        if install_date_str:
            try:
                first_prod_day = datetime.fromisoformat(install_date_str)
            except (ValueError, TypeError):
                _LOGGER.warning("Invalid installation_date: %s", install_date_str)

        _LOGGER.debug(
            "ROI recalculate requested: price_dev=%s, panel_deg=%s, "
            "investment=%s, installed_kw=%s, first_prod_day=%s",
            price_dev, panel_deg, investment, installed_kw, first_prod_day,
        )

        def _recalculate():
            all_records = coordinator.storage.get_all_records_as_list()
            _LOGGER.debug("ROI recalculate: %d hourly records loaded", len(all_records))
            yearly_overview, monthly_by_year = generate_monthly_report(
                all_records, coordinator._calc_params, investment, coordinator._yearly_params
            )
            _LOGGER.debug(
                "ROI recalculate: generate_monthly_report returned %d yearly, %d monthly groups",
                len(yearly_overview), len(monthly_by_year),
            )
            projection = calculate_30_year_projection(
                yearly_overview,
                monthly_by_year,
                price_development=price_dev,
                panel_degradation=panel_deg,
                investment=investment,
                installed_kw=installed_kw,
                first_production_day=first_prod_day,
            )
            _LOGGER.debug("ROI recalculate: projection has %d entries", len(projection))
            return [r.to_dict() for r in projection]

        try:
            projection = await hass.async_add_executor_job(_recalculate)
        except Exception:
            _LOGGER.exception("ROI recalculation failed")
            connection.send_error(msg["id"], "recalculation_failed", "ROI recalculation failed, check logs")
            return
    else:
        investment = config.get(CONF_INVESTMENT_AMOUNT, DEFAULT_INVESTMENT_AMOUNT)
        projection = await hass.async_add_executor_job(lambda: coordinator.storage.roi_projection)

    connection.send_result(msg["id"], {
        "projection": projection,
        "investment": config.get(CONF_INVESTMENT_AMOUNT, 0) + config.get(CONF_LOAN_AMOUNT, 0),
        "price_development": configured_price_dev,
        "panel_degradation": configured_panel_deg,
    })
