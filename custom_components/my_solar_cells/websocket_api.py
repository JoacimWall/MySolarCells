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
    CONF_PRODUCTION_SENSOR,
    DOMAIN,
)

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
    websocket_api.async_register_command(hass, ws_get_sensor_config)
    websocket_api.async_register_command(hass, ws_get_period_summaries)


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
        return db.get_all_yearly_params()

    params = await hass.async_add_executor_job(_fetch)
    connection.send_result(msg["id"], {"yearly_params": params})


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
