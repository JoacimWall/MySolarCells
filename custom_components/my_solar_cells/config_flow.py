"""Config flow for My Solar Cells integration - 5-step setup."""

from __future__ import annotations

import logging
from datetime import datetime
from typing import Any

import aiohttp
import voluptuous as vol

from homeassistant import config_entries
from homeassistant.core import callback
from homeassistant.helpers import selector

from .const import (
    CONF_AMORTIZATION_MONTHLY,
    CONF_API_KEY,
    CONF_BATTERY_CHARGE_SENSOR,
    CONF_BATTERY_DISCHARGE_SENSOR,
    CONF_ENERGY_TAX,
    CONF_FIXED_PRICE,
    CONF_GRID_COMPENSATION,
    CONF_GRID_EXPORT_SENSOR,
    CONF_GRID_IMPORT_SENSOR,
    CONF_HOME_ID,
    CONF_HOME_NAME,
    CONF_IMPORT_ONLY_SPOT_PRICE,
    CONF_INSTALLATION_DATE,
    CONF_INSTALLED_KW,
    CONF_INTEREST_RATE,
    CONF_INVESTMENT_AMOUNT,
    CONF_LOAN_AMOUNT,
    CONF_PANEL_DEGRADATION,
    CONF_PRICE_DEVELOPMENT,
    CONF_PRODUCTION_SENSOR,
    CONF_TAX_REDUCTION,
    CONF_TRANSFER_FEE,
    CONF_USE_SPOT_PRICE,
    DEFAULT_AMORTIZATION_MONTHLY,
    DEFAULT_ENERGY_TAX,
    DEFAULT_FIXED_PRICE,
    DEFAULT_GRID_COMPENSATION,
    DEFAULT_INSTALLED_KW,
    DEFAULT_INTEREST_RATE,
    DEFAULT_INVESTMENT_AMOUNT,
    DEFAULT_LOAN_AMOUNT,
    DEFAULT_PANEL_DEGRADATION,
    DEFAULT_PRICE_DEVELOPMENT,
    DEFAULT_TAX_REDUCTION,
    DEFAULT_TRANSFER_FEE,
    DEFAULT_USE_SPOT_PRICE,
    DOMAIN,
)
from .tibber_client import TibberApiError, TibberClient

_LOGGER = logging.getLogger(__name__)


class MySolarCellsConfigFlow(config_entries.ConfigFlow, domain=DOMAIN):
    """Handle a config flow for My Solar Cells."""

    VERSION = 1

    def __init__(self) -> None:
        """Initialize the config flow."""
        self._data: dict[str, Any] = {}
        self._homes: list[dict] = []

    async def async_step_user(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Step 1: Tibber API key."""
        errors: dict[str, str] = {}

        if user_input is not None:
            api_key = user_input[CONF_API_KEY]
            try:
                async with aiohttp.ClientSession() as session:
                    client = TibberClient(session, api_key)
                    await client.test_connection()
                    self._homes = await client.get_homes()
            except TibberApiError:
                errors["base"] = "invalid_auth"
            except Exception:  # noqa: BLE001
                _LOGGER.exception("Unexpected error connecting to Tibber")
                errors["base"] = "cannot_connect"
            else:
                self._data[CONF_API_KEY] = api_key
                return await self.async_step_home()

        return self.async_show_form(
            step_id="user",
            data_schema=vol.Schema(
                {
                    vol.Required(CONF_API_KEY): str,
                }
            ),
            errors=errors,
        )

    async def async_step_home(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Step 2: Select Tibber home."""
        if user_input is not None:
            self._data[CONF_HOME_ID] = user_input[CONF_HOME_ID]
            # Find home name for display
            for home in self._homes:
                if home["id"] == user_input[CONF_HOME_ID]:
                    self._data[CONF_HOME_NAME] = (
                        f"{home['address1']}, {home['city']}"
                    )
                    break
            self._data[CONF_IMPORT_ONLY_SPOT_PRICE] = user_input.get(
                CONF_IMPORT_ONLY_SPOT_PRICE, False
            )
            return await self.async_step_sensors()

        home_options = {
            home["id"]: f"{home['address1']}, {home['city']}"
            for home in self._homes
        }

        return self.async_show_form(
            step_id="home",
            data_schema=vol.Schema(
                {
                    vol.Required(CONF_HOME_ID): vol.In(home_options),
                    vol.Optional(CONF_IMPORT_ONLY_SPOT_PRICE, default=False): bool,
                }
            ),
        )

    async def async_step_sensors(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Step 3: HA sensor entity selection."""
        if user_input is not None:
            self._data.update(user_input)
            return await self.async_step_financial()

        entity_selector = selector.EntitySelector(
            selector.EntitySelectorConfig(domain="sensor")
        )

        return self.async_show_form(
            step_id="sensors",
            data_schema=vol.Schema(
                {
                    vol.Required(CONF_GRID_EXPORT_SENSOR): entity_selector,
                    vol.Required(CONF_GRID_IMPORT_SENSOR): entity_selector,
                    vol.Required(CONF_PRODUCTION_SENSOR): entity_selector,
                    vol.Optional(CONF_BATTERY_CHARGE_SENSOR): entity_selector,
                    vol.Optional(CONF_BATTERY_DISCHARGE_SENSOR): entity_selector,
                }
            ),
        )

    async def async_step_financial(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Step 4: Financial parameters."""
        if user_input is not None:
            self._data.update(user_input)
            return await self.async_step_investment()

        return self.async_show_form(
            step_id="financial",
            data_schema=vol.Schema(
                {
                    vol.Required(
                        CONF_TAX_REDUCTION, default=DEFAULT_TAX_REDUCTION
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_GRID_COMPENSATION, default=DEFAULT_GRID_COMPENSATION
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_TRANSFER_FEE, default=DEFAULT_TRANSFER_FEE
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_ENERGY_TAX, default=DEFAULT_ENERGY_TAX
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_INSTALLED_KW, default=DEFAULT_INSTALLED_KW
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_USE_SPOT_PRICE, default=DEFAULT_USE_SPOT_PRICE
                    ): bool,
                    vol.Optional(
                        CONF_FIXED_PRICE, default=DEFAULT_FIXED_PRICE
                    ): vol.Coerce(float),
                }
            ),
        )

    async def async_step_investment(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Step 5: Investment and projection parameters."""
        if user_input is not None:
            self._data.update(user_input)

            # Set title from home name
            title = self._data.get(CONF_HOME_NAME, "My Solar Cells")

            return self.async_create_entry(title=title, data=self._data)

        return self.async_show_form(
            step_id="investment",
            data_schema=vol.Schema(
                {
                    vol.Required(
                        CONF_INVESTMENT_AMOUNT, default=DEFAULT_INVESTMENT_AMOUNT
                    ): vol.Coerce(int),
                    vol.Optional(
                        CONF_LOAN_AMOUNT, default=DEFAULT_LOAN_AMOUNT
                    ): vol.Coerce(int),
                    vol.Optional(
                        CONF_INTEREST_RATE, default=DEFAULT_INTEREST_RATE
                    ): vol.Coerce(float),
                    vol.Optional(
                        CONF_AMORTIZATION_MONTHLY, default=DEFAULT_AMORTIZATION_MONTHLY
                    ): vol.Coerce(int),
                    vol.Required(
                        CONF_PANEL_DEGRADATION, default=DEFAULT_PANEL_DEGRADATION
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_PRICE_DEVELOPMENT, default=DEFAULT_PRICE_DEVELOPMENT
                    ): vol.Coerce(float),
                    vol.Required(CONF_INSTALLATION_DATE): str,
                }
            ),
        )

    @staticmethod
    @callback
    def async_get_options_flow(
        config_entry: config_entries.ConfigEntry,
    ) -> MySolarCellsOptionsFlow:
        """Get the options flow handler."""
        return MySolarCellsOptionsFlow(config_entry)


class MySolarCellsOptionsFlow(config_entries.OptionsFlow):
    """Handle options flow for My Solar Cells (edit steps 3-5)."""

    def __init__(self, config_entry: config_entries.ConfigEntry) -> None:
        """Initialize options flow."""
        self._config_entry = config_entry

    async def async_step_init(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Start options flow - sensor configuration."""
        if user_input is not None:
            return await self.async_step_financial(user_input=None)

        data = self._config_entry.data
        entity_selector = selector.EntitySelector(
            selector.EntitySelectorConfig(domain="sensor")
        )

        return self.async_show_form(
            step_id="init",
            data_schema=vol.Schema(
                {
                    vol.Required(
                        CONF_GRID_EXPORT_SENSOR,
                        default=data.get(CONF_GRID_EXPORT_SENSOR, ""),
                    ): entity_selector,
                    vol.Required(
                        CONF_GRID_IMPORT_SENSOR,
                        default=data.get(CONF_GRID_IMPORT_SENSOR, ""),
                    ): entity_selector,
                    vol.Required(
                        CONF_PRODUCTION_SENSOR,
                        default=data.get(CONF_PRODUCTION_SENSOR, ""),
                    ): entity_selector,
                    vol.Optional(
                        CONF_BATTERY_CHARGE_SENSOR,
                        default=data.get(CONF_BATTERY_CHARGE_SENSOR, ""),
                    ): entity_selector,
                    vol.Optional(
                        CONF_BATTERY_DISCHARGE_SENSOR,
                        default=data.get(CONF_BATTERY_DISCHARGE_SENSOR, ""),
                    ): entity_selector,
                }
            ),
        )

    async def async_step_financial(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Options step: Financial parameters."""
        if user_input is not None:
            return await self.async_step_investment(user_input=None)

        data = self._config_entry.data

        return self.async_show_form(
            step_id="financial",
            data_schema=vol.Schema(
                {
                    vol.Required(
                        CONF_TAX_REDUCTION,
                        default=data.get(CONF_TAX_REDUCTION, DEFAULT_TAX_REDUCTION),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_GRID_COMPENSATION,
                        default=data.get(CONF_GRID_COMPENSATION, DEFAULT_GRID_COMPENSATION),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_TRANSFER_FEE,
                        default=data.get(CONF_TRANSFER_FEE, DEFAULT_TRANSFER_FEE),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_ENERGY_TAX,
                        default=data.get(CONF_ENERGY_TAX, DEFAULT_ENERGY_TAX),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_INSTALLED_KW,
                        default=data.get(CONF_INSTALLED_KW, DEFAULT_INSTALLED_KW),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_USE_SPOT_PRICE,
                        default=data.get(CONF_USE_SPOT_PRICE, DEFAULT_USE_SPOT_PRICE),
                    ): bool,
                    vol.Optional(
                        CONF_FIXED_PRICE,
                        default=data.get(CONF_FIXED_PRICE, DEFAULT_FIXED_PRICE),
                    ): vol.Coerce(float),
                }
            ),
        )

    async def async_step_investment(
        self, user_input: dict[str, Any] | None = None
    ) -> config_entries.ConfigFlowResult:
        """Options step: Investment parameters."""
        if user_input is not None:
            # Merge all options data
            new_data = {**self._config_entry.data, **user_input}
            self.hass.config_entries.async_update_entry(
                self._config_entry, data=new_data
            )
            return self.async_create_entry(title="", data={})

        data = self._config_entry.data

        return self.async_show_form(
            step_id="investment",
            data_schema=vol.Schema(
                {
                    vol.Required(
                        CONF_INVESTMENT_AMOUNT,
                        default=data.get(CONF_INVESTMENT_AMOUNT, DEFAULT_INVESTMENT_AMOUNT),
                    ): vol.Coerce(int),
                    vol.Optional(
                        CONF_LOAN_AMOUNT,
                        default=data.get(CONF_LOAN_AMOUNT, DEFAULT_LOAN_AMOUNT),
                    ): vol.Coerce(int),
                    vol.Optional(
                        CONF_INTEREST_RATE,
                        default=data.get(CONF_INTEREST_RATE, DEFAULT_INTEREST_RATE),
                    ): vol.Coerce(float),
                    vol.Optional(
                        CONF_AMORTIZATION_MONTHLY,
                        default=data.get(CONF_AMORTIZATION_MONTHLY, DEFAULT_AMORTIZATION_MONTHLY),
                    ): vol.Coerce(int),
                    vol.Required(
                        CONF_PANEL_DEGRADATION,
                        default=data.get(CONF_PANEL_DEGRADATION, DEFAULT_PANEL_DEGRADATION),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_PRICE_DEVELOPMENT,
                        default=data.get(CONF_PRICE_DEVELOPMENT, DEFAULT_PRICE_DEVELOPMENT),
                    ): vol.Coerce(float),
                    vol.Required(
                        CONF_INSTALLATION_DATE,
                        default=data.get(CONF_INSTALLATION_DATE, ""),
                    ): str,
                }
            ),
        )
