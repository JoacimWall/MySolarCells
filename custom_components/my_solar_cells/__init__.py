"""The My Solar Cells integration."""

from __future__ import annotations

import logging
from pathlib import Path

import aiohttp

from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant
from homeassistant.helpers.aiohttp_client import async_get_clientsession

from .const import DOMAIN, PLATFORMS
from .coordinator import MySolarCellsCoordinator
from .storage import MySolarCellsStorage

_LOGGER = logging.getLogger(__name__)

CARD_JS = "my-solar-cells-roi-card.js"
CARD_URL = f"/{DOMAIN}/{CARD_JS}"
CARD_DIR = Path(__file__).parent


async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Set up My Solar Cells from a config entry."""
    hass.data.setdefault(DOMAIN, {})

    session = async_get_clientsession(hass)

    # Initialize storage
    storage = MySolarCellsStorage(hass, entry.entry_id)
    await storage.async_load()

    # Create coordinator
    coordinator = MySolarCellsCoordinator(
        hass=hass,
        session=session,
        storage=storage,
        config_data=dict(entry.data),
        entry_id=entry.entry_id,
    )

    # Do first refresh
    await coordinator.async_config_entry_first_refresh()

    hass.data[DOMAIN][entry.entry_id] = coordinator

    await hass.config_entries.async_forward_entry_setups(entry, PLATFORMS)

    # Register Lovelace card resource
    await _register_card(hass)

    return True


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a My Solar Cells config entry."""
    unload_ok = await hass.config_entries.async_unload_platforms(entry, PLATFORMS)

    if unload_ok:
        hass.data[DOMAIN].pop(entry.entry_id, None)

    return unload_ok


async def _register_card(hass: HomeAssistant) -> None:
    """Register the custom Lovelace card as a static resource."""
    # Serve the JS file from the integration's own directory
    hass.http.register_static_path(
        f"/{DOMAIN}", str(CARD_DIR), cache_headers=False
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
