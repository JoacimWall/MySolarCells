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
from .statistics_import import (
    SENSORS_TO_IMPORT,
    STATISTICS_IMPORT_VERSION,
    async_import_historical_statistics,
)
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

    # Import historical statistics now that entities are registered
    stored_ver = storage.statistics_import_version
    if stored_ver < STATISTICS_IMPORT_VERSION:
        _LOGGER.warning(
            "Statistics import needed: stored version %d < current %d, scheduling import",
            stored_ver,
            STATISTICS_IMPORT_VERSION,
        )

        async def _do_statistics_import() -> None:
            try:
                _LOGGER.warning("Statistics import task starting")
                success = await async_import_historical_statistics(hass, coordinator)
                _LOGGER.warning("Statistics import returned success=%s", success)
                if success:
                    storage.statistics_import_version = STATISTICS_IMPORT_VERSION
                    await storage.async_save()
                    coordinator._statistics_import_done = True
            except Exception:
                _LOGGER.warning(
                    "Historical statistics import failed", exc_info=True
                )

        hass.async_create_task(_do_statistics_import())
    else:
        _LOGGER.warning(
            "Statistics import skipped: stored version %d >= current %d",
            stored_ver,
            STATISTICS_IMPORT_VERSION,
        )

    # Register Lovelace card resource
    await _register_card(hass)

    return True


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a My Solar Cells config entry."""
    unload_ok = await hass.config_entries.async_unload_platforms(entry, PLATFORMS)

    if unload_ok:
        hass.data[DOMAIN].pop(entry.entry_id, None)

    return unload_ok


async def async_remove_entry(hass: HomeAssistant, entry: ConfigEntry) -> None:
    """Clean up all data when the integration is removed."""
    _LOGGER.warning("Removing My Solar Cells: cleaning up all data")

    # 1. Remove JSON storage file
    storage = MySolarCellsStorage(hass, entry.entry_id)
    await storage.async_load()
    await storage.async_remove()
    _LOGGER.warning("Removed storage file")

    # 2. Clear imported statistics from the recorder
    try:
        from homeassistant.components.recorder import get_instance
        from homeassistant.components.recorder.statistics import clear_statistics
        from homeassistant.helpers import entity_registry as er

        registry = er.async_get(hass)
        statistic_ids = []
        for sensor_key, _, _, _ in SENSORS_TO_IMPORT:
            unique_id = f"{entry.entry_id}_{sensor_key}"
            entity_id = registry.async_get_entity_id("sensor", DOMAIN, unique_id)
            if entity_id:
                statistic_ids.append(entity_id)

        if statistic_ids:
            instance = get_instance(hass)
            await instance.async_add_executor_job(
                clear_statistics, instance, statistic_ids
            )
            _LOGGER.warning(
                "Cleared recorder statistics for %d sensors: %s",
                len(statistic_ids),
                statistic_ids,
            )
    except Exception:
        _LOGGER.warning("Failed to clear recorder statistics", exc_info=True)

    # 3. Dismiss persistent notification
    try:
        from homeassistant.components.persistent_notification import async_dismiss

        async_dismiss(hass, "my_solar_cells_statistics_import")
    except Exception:
        pass

    _LOGGER.warning("My Solar Cells cleanup complete")


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
