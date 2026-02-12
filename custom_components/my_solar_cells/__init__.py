"""The My Solar Cells integration."""

from __future__ import annotations

import logging

import aiohttp

from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant
from homeassistant.helpers.aiohttp_client import async_get_clientsession

from .const import DOMAIN, PLATFORMS
from .coordinator import MySolarCellsCoordinator
from .storage import MySolarCellsStorage

_LOGGER = logging.getLogger(__name__)


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
    _register_card(hass)

    return True


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a My Solar Cells config entry."""
    unload_ok = await hass.config_entries.async_unload_platforms(entry, PLATFORMS)

    if unload_ok:
        hass.data[DOMAIN].pop(entry.entry_id, None)

    return unload_ok


def _register_card(hass: HomeAssistant) -> None:
    """Register the custom Lovelace card resource."""
    # The card JS file is served from custom_cards/my-solar-cells-roi-card/
    # HACS handles the resource registration automatically
    pass
