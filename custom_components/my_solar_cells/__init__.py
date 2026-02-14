"""The My Solar Cells integration."""

from __future__ import annotations

import logging
from pathlib import Path

import aiohttp

from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant
from homeassistant.helpers.aiohttp_client import async_get_clientsession

from .const import CONF_YEARLY_PARAMS, DOMAIN, PLATFORMS, STORAGE_KEY_PREFIX
from .coordinator import MySolarCellsCoordinator
from .database import MySolarCellsDatabase
from .websocket_api import async_register_websocket_commands

_LOGGER = logging.getLogger(__name__)

CARD_JS = "my-solar-cells-roi-card.js"
PANEL_JS = "my-solar-cells-panel.js"
CARD_URL = f"/{DOMAIN}/{CARD_JS}"
PANEL_URL = f"/{DOMAIN}/{PANEL_JS}"
CARD_DIR = Path(__file__).parent


async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Set up My Solar Cells from a config entry."""
    hass.data.setdefault(DOMAIN, {})

    session = async_get_clientsession(hass)

    # Delete old JSON storage file if it exists (replaced by SQLite)
    old_json_path = hass.config.path(f".storage/{STORAGE_KEY_PREFIX}.{entry.entry_id}")
    _delete_old_json_storage(old_json_path)

    # Initialize SQLite database
    database = MySolarCellsDatabase(hass, entry.entry_id)
    await database.async_setup()

    # Sync per-year financial params from config entry to database
    yearly_params = entry.data.get(CONF_YEARLY_PARAMS)
    if yearly_params and isinstance(yearly_params, dict):
        for year_str, params in yearly_params.items():
            try:
                database.set_yearly_params(int(year_str), params)
            except (ValueError, TypeError):
                pass
        await database.async_save()

    # Create coordinator
    coordinator = MySolarCellsCoordinator(
        hass=hass,
        session=session,
        storage=database,
        config_data=dict(entry.data),
        entry_id=entry.entry_id,
    )

    # Do first refresh
    await coordinator.async_config_entry_first_refresh()

    hass.data[DOMAIN][entry.entry_id] = coordinator

    await hass.config_entries.async_forward_entry_setups(entry, PLATFORMS)

    # Register Lovelace card resource
    await _register_card(hass)

    # Register WebSocket commands and sidebar panel
    async_register_websocket_commands(hass)
    await _register_panel(hass, entry)

    return True


def _delete_old_json_storage(path: str) -> None:
    """Delete the old JSON storage file if it exists."""
    import os

    if os.path.exists(path):
        try:
            os.remove(path)
            _LOGGER.info("Deleted old JSON storage file: %s", path)
        except OSError:
            _LOGGER.warning("Failed to delete old JSON storage file: %s", path)


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a My Solar Cells config entry."""
    unload_ok = await hass.config_entries.async_unload_platforms(entry, PLATFORMS)

    if unload_ok:
        coordinator = hass.data[DOMAIN].pop(entry.entry_id, None)
        if coordinator:
            await coordinator.storage.async_close()

        # Remove sidebar panel when last entry unloads
        if not hass.data.get(DOMAIN):
            try:
                hass.components.frontend.async_remove_panel("my-solar-cells")
            except Exception:  # noqa: BLE001
                pass

    return unload_ok


async def async_remove_entry(hass: HomeAssistant, entry: ConfigEntry) -> None:
    """Clean up all data when the integration is removed."""
    _LOGGER.warning("Removing My Solar Cells: cleaning up all data")

    # Remove SQLite database file
    database = MySolarCellsDatabase(hass, entry.entry_id)
    await database.async_remove()
    _LOGGER.warning("Removed database file")

    # Delete old JSON storage file too if still present
    old_json_path = hass.config.path(f".storage/{STORAGE_KEY_PREFIX}.{entry.entry_id}")
    _delete_old_json_storage(old_json_path)

    _LOGGER.warning("My Solar Cells cleanup complete")


async def _register_panel(hass: HomeAssistant, entry: ConfigEntry) -> None:
    """Register the sidebar panel for browsing database data."""
    try:
        hass.components.panel_custom.async_register_panel(
            frontend_url_path="my-solar-cells",
            webcomponent_name="my-solar-cells-panel",
            sidebar_title="Solar Data",
            sidebar_icon="mdi:solar-power-variant",
            module_url=PANEL_URL,
            embed_iframe=False,
            require_admin=False,
            config={"entry_id": entry.entry_id},
        )
    except Exception:  # noqa: BLE001
        _LOGGER.warning("Could not register Solar Data panel")


async def _register_card(hass: HomeAssistant) -> None:
    """Register the custom Lovelace card as a static resource."""
    from homeassistant.components.http import StaticPathConfig

    # Serve the JS file from the integration's own directory
    await hass.http.async_register_static_paths(
        [StaticPathConfig(f"/{DOMAIN}", str(CARD_DIR), False)]
    )

    # Add as a Lovelace resource if not already registered
    await _add_lovelace_resource(hass, CARD_URL)


async def _add_lovelace_resource(hass: HomeAssistant, url: str) -> None:
    """Add a Lovelace resource if it's not already registered."""
    try:
        from homeassistant.components.lovelace import DOMAIN as LOVELACE_DOMAIN
        from homeassistant.components.lovelace.resources import (
            ResourceStorageCollection,
        )

        lovelace_data = hass.data.get(LOVELACE_DOMAIN)
        if lovelace_data and hasattr(lovelace_data, "resources"):
            resources = lovelace_data.resources
            if isinstance(resources, ResourceStorageCollection):
                # Check if already registered
                for item in resources.async_items():
                    if item.get("url") == url:
                        return
                await resources.async_create_item(
                    {"res_type": "module", "url": url}
                )
                _LOGGER.info("Registered Lovelace resource: %s", url)
                return

        _LOGGER.warning(
            "Could not auto-register card resource. Add manually: "
            "Settings -> Dashboards -> Resources -> Add %s as JavaScript Module",
            url,
        )
    except Exception:  # noqa: BLE001
        _LOGGER.warning(
            "Could not auto-register card resource. Add manually: "
            "Settings -> Dashboards -> Resources -> Add %s as JavaScript Module",
            url,
        )
