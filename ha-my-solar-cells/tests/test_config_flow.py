"""Tests for the config flow."""

import pytest

from custom_components.my_solar_cells.const import DOMAIN


class TestConfigFlow:
    """Tests for the config flow steps."""

    def test_config_flow_class_exists(self):
        """Test that config flow class is defined."""
        from custom_components.my_solar_cells.config_flow import MySolarCellsConfigFlow

        flow = MySolarCellsConfigFlow()
        # Flow should be instantiable
        assert flow is not None

    def test_config_flow_domain(self):
        """Test config flow is registered for correct domain."""
        assert DOMAIN == "my_solar_cells"

    def test_options_flow_exists(self):
        """Test that options flow handler is defined."""
        from custom_components.my_solar_cells.config_flow import MySolarCellsOptionsFlow

        assert MySolarCellsOptionsFlow is not None

    def test_config_flow_has_5_steps(self):
        """Test that config flow module defines all 5 step methods.

        Note: The class inherits from a mocked ConfigFlow, so we check
        the module source for the step definitions.
        """
        import inspect
        from custom_components.my_solar_cells import config_flow

        source = inspect.getsource(config_flow)
        assert "async_step_user" in source
        assert "async_step_home" in source
        assert "async_step_sensors" in source
        assert "async_step_financial" in source
        assert "async_step_investment" in source
