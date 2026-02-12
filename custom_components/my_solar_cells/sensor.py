"""Sensor entities for My Solar Cells integration."""

from __future__ import annotations

from typing import Any

from homeassistant.components.sensor import (
    SensorDeviceClass,
    SensorEntity,
    SensorStateClass,
)
from homeassistant.config_entries import ConfigEntry
from homeassistant.const import UnitOfEnergy, UnitOfMass
from homeassistant.core import HomeAssistant
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from .const import DOMAIN
from .coordinator import MySolarCellsCoordinator
from .entity import MySolarCellsEntity

CURRENCY_SEK = "SEK"
CURRENCY_SEK_PER_KWH = "SEK/kWh"


SENSOR_DESCRIPTIONS: list[dict[str, Any]] = [
    # --- Daily sensors ---
    {
        "key": "daily_production_sold",
        "name": "Daily Production Sold",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:solar-power",
    },
    {
        "key": "daily_production_own_use",
        "name": "Daily Production Own Use",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:home-lightning-bolt",
    },
    {
        "key": "daily_purchased",
        "name": "Daily Purchased",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:transmission-tower-import",
    },
    {
        "key": "daily_sold_profit",
        "name": "Daily Sold Profit",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-plus",
    },
    {
        "key": "daily_own_use_saved",
        "name": "Daily Own Use Saved",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:piggy-bank",
    },
    {
        "key": "daily_purchased_cost",
        "name": "Daily Purchased Cost",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-minus",
    },
    {
        "key": "daily_balance",
        "name": "Daily Balance",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:scale-balance",
    },
    # --- Monthly sensors ---
    {
        "key": "monthly_production_sold",
        "name": "Monthly Production Sold",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:solar-power",
    },
    {
        "key": "monthly_production_own_use",
        "name": "Monthly Production Own Use",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:home-lightning-bolt",
    },
    {
        "key": "monthly_purchased",
        "name": "Monthly Purchased",
        "unit": UnitOfEnergy.KILO_WATT_HOUR,
        "device_class": SensorDeviceClass.ENERGY,
        "state_class": SensorStateClass.TOTAL_INCREASING,
        "icon": "mdi:transmission-tower-import",
    },
    {
        "key": "monthly_sold_profit",
        "name": "Monthly Sold Profit",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-plus",
    },
    {
        "key": "monthly_own_use_saved",
        "name": "Monthly Own Use Saved",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:piggy-bank",
    },
    {
        "key": "monthly_purchased_cost",
        "name": "Monthly Purchased Cost",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-minus",
    },
    {
        "key": "monthly_balance",
        "name": "Monthly Balance",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:scale-balance",
    },
    {
        "key": "monthly_tax_reduction",
        "name": "Monthly Tax Reduction",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-refund",
    },
    {
        "key": "monthly_grid_compensation",
        "name": "Monthly Grid Compensation",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-refund",
    },
    # --- Yearly sensors ---
    {
        "key": "yearly_total_savings",
        "name": "Yearly Total Savings",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:finance",
    },
    {
        "key": "yearly_return_percentage",
        "name": "Yearly Return Percentage",
        "unit": "%",
        "device_class": None,
        "state_class": SensorStateClass.MEASUREMENT,
        "icon": "mdi:percent",
    },
    # --- Lifetime / ROI sensors ---
    {
        "key": "total_savings_to_date",
        "name": "Total Savings to Date",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:cash-multiple",
    },
    {
        "key": "remaining_on_investment",
        "name": "Remaining on Investment",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:chart-timeline-variant-shimmer",
    },
    {
        "key": "investment_amount",
        "name": "Investment Amount",
        "unit": CURRENCY_SEK,
        "device_class": SensorDeviceClass.MONETARY,
        "state_class": SensorStateClass.TOTAL,
        "icon": "mdi:bank",
    },
    # --- Spot price sensors ---
    {
        "key": "current_spot_price",
        "name": "Current Spot Price",
        "unit": CURRENCY_SEK_PER_KWH,
        "device_class": None,
        "state_class": SensorStateClass.MEASUREMENT,
        "icon": "mdi:currency-usd",
    },
    {
        "key": "avg_spot_price_today",
        "name": "Average Spot Price Today",
        "unit": CURRENCY_SEK_PER_KWH,
        "device_class": None,
        "state_class": SensorStateClass.MEASUREMENT,
        "icon": "mdi:chart-line",
    },
]


async def async_setup_entry(
    hass: HomeAssistant,
    entry: ConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up My Solar Cells sensors from a config entry."""
    coordinator: MySolarCellsCoordinator = hass.data[DOMAIN][entry.entry_id]

    entities: list[SensorEntity] = []

    # Standard sensors from descriptions
    for desc in SENSOR_DESCRIPTIONS:
        entities.append(
            MySolarCellsSensor(
                coordinator=coordinator,
                entry_id=entry.entry_id,
                key=desc["key"],
                name=desc["name"],
                unit=desc.get("unit"),
                device_class=desc.get("device_class"),
                state_class=desc.get("state_class"),
                icon=desc.get("icon"),
            )
        )

    # ROI payback year sensor (with extra attributes)
    entities.append(
        MySolarCellsRoiSensor(
            coordinator=coordinator,
            entry_id=entry.entry_id,
        )
    )

    # Current price level sensor (text)
    entities.append(
        MySolarCellsPriceLevelSensor(
            coordinator=coordinator,
            entry_id=entry.entry_id,
        )
    )

    async_add_entities(entities)


class MySolarCellsSensor(MySolarCellsEntity, SensorEntity):
    """A My Solar Cells sensor entity."""

    def __init__(
        self,
        coordinator: MySolarCellsCoordinator,
        entry_id: str,
        key: str,
        name: str,
        unit: str | None = None,
        device_class: SensorDeviceClass | None = None,
        state_class: SensorStateClass | None = None,
        icon: str | None = None,
    ) -> None:
        """Initialize the sensor."""
        super().__init__(coordinator, entry_id, key, name)
        self._attr_native_unit_of_measurement = unit
        self._attr_device_class = device_class
        self._attr_state_class = state_class
        self._attr_icon = icon

    @property
    def native_value(self) -> float | int | None:
        """Return the sensor value."""
        if self.coordinator.data is None:
            return None
        return self.coordinator.data.get(self._key)


class MySolarCellsRoiSensor(MySolarCellsEntity, SensorEntity):
    """ROI payback year sensor with projection data as attributes."""

    def __init__(
        self,
        coordinator: MySolarCellsCoordinator,
        entry_id: str,
    ) -> None:
        """Initialize the ROI sensor."""
        super().__init__(coordinator, entry_id, "roi_payback_year", "ROI Payback Year")
        self._attr_icon = "mdi:calendar-check"

    @property
    def native_value(self) -> int | None:
        """Return the payback year."""
        if self.coordinator.data is None:
            return None
        return self.coordinator.data.get("roi_payback_year")

    @property
    def extra_state_attributes(self) -> dict[str, Any]:
        """Return the 30-year projection as an attribute."""
        if self.coordinator.data is None:
            return {}
        return {
            "projection": self.coordinator.data.get("roi_projection", []),
            "investment_amount": self.coordinator.data.get("investment_amount", 0),
            "total_savings_to_date": self.coordinator.data.get("total_savings_to_date", 0),
        }


class MySolarCellsPriceLevelSensor(MySolarCellsEntity, SensorEntity):
    """Current price level sensor (text: CHEAP/NORMAL/EXPENSIVE etc.)."""

    def __init__(
        self,
        coordinator: MySolarCellsCoordinator,
        entry_id: str,
    ) -> None:
        """Initialize the price level sensor."""
        super().__init__(
            coordinator, entry_id, "current_price_level", "Current Price Level"
        )
        self._attr_icon = "mdi:tag"

    @property
    def native_value(self) -> str | None:
        """Return the current price level."""
        if self.coordinator.data is None:
            return None
        return self.coordinator.data.get("current_price_level")

    @property
    def extra_state_attributes(self) -> dict[str, Any]:
        """Return spot price summary as attributes."""
        if self.coordinator.data is None:
            return {}
        today = self.coordinator.data.get("spot_prices_today", [])
        tomorrow = self.coordinator.data.get("spot_prices_tomorrow", [])
        attrs: dict[str, Any] = {
            "prices_today_count": len(today),
            "prices_tomorrow_count": len(tomorrow),
            "avg_spot_price_today": self.coordinator.data.get("avg_spot_price_today", 0),
        }
        # Min/max for today
        if today:
            totals = [p.get("total", 0) for p in today]
            attrs["min_price_today"] = round(min(totals), 4)
            attrs["max_price_today"] = round(max(totals), 4)
        return attrs
