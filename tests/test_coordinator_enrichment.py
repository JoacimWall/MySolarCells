"""Tests for coordinator delta-based sensor enrichment."""

from __future__ import annotations

import importlib
import sys
from types import SimpleNamespace
from unittest.mock import MagicMock

import pytest


def _setup_coordinator_module():
    """Import coordinator with a real DataUpdateCoordinator stub.

    The conftest mocks DataUpdateCoordinator as a MagicMock, which prevents
    MySolarCellsCoordinator from being a real class. Here we replace it with
    a minimal stub so the class body executes normally.
    """
    class _StubDataUpdateCoordinator:
        def __init__(self, hass, *args, **kwargs):
            self.hass = hass

        def __init_subclass__(cls, **kwargs):
            pass

        def __class_getitem__(cls, item):
            return cls

    class _StubUpdateFailed(Exception):
        pass

    update_coord_mod = sys.modules["homeassistant.helpers.update_coordinator"]
    update_coord_mod.DataUpdateCoordinator = _StubDataUpdateCoordinator
    update_coord_mod.UpdateFailed = _StubUpdateFailed

    # Ensure the package and its dependencies are loaded first.
    # This populates sys.modules with .const, .storage, etc. so relative
    # imports in coordinator.py don't re-trigger __init__.py.
    import custom_components.my_solar_cells.const  # noqa: F401
    import custom_components.my_solar_cells.database  # noqa: F401
    import custom_components.my_solar_cells.financial_engine  # noqa: F401
    import custom_components.my_solar_cells.roi_engine  # noqa: F401
    import custom_components.my_solar_cells.tibber_client  # noqa: F401

    # Remove the cached coordinator (which was a MagicMock) and the
    # __init__ module that imported it, so reimport picks up the stub.
    pkg = "custom_components.my_solar_cells"
    for mod_name in [f"{pkg}.coordinator", pkg]:
        sys.modules.pop(mod_name, None)

    # Reimport the package (its __init__.py will reimport coordinator
    # with the real DataUpdateCoordinator stub)
    mod = importlib.import_module(f"{pkg}.coordinator")
    return mod


_coord_mod = _setup_coordinator_module()
MySolarCellsCoordinator = _coord_mod.MySolarCellsCoordinator

from custom_components.my_solar_cells.financial_engine import CalcParams


def _make_coordinator(
    sensor_config: dict | None = None,
    stored_readings: dict | None = None,
):
    """Create a coordinator instance for testing delta methods."""
    hass = MagicMock()
    session = MagicMock()
    storage = MagicMock()
    storage.last_sensor_readings = stored_readings or {}

    config = {
        "api_key": "test",
        "home_id": "home1",
        **(sensor_config or {}),
    }

    coord = MySolarCellsCoordinator(hass, session, storage, config, "entry1")
    return coord


def _set_sensor_state(coord, entity_id: str, value: str) -> None:
    """Configure a mocked HA sensor state."""
    state_obj = MagicMock()
    state_obj.state = value

    # Build/update the state lookup map
    if not hasattr(coord, "_test_states"):
        coord._test_states = {}
    coord._test_states[entity_id] = state_obj

    states = coord._test_states
    coord.hass.states.get.side_effect = lambda eid: states.get(eid)


class TestComputeSensorDeltas:
    """Tests for _compute_sensor_deltas()."""

    def test_first_call_stores_baseline_no_delta(self):
        """First reading should store baseline and return no deltas."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
        )
        _set_sensor_state(coord, "sensor.solar_production", "100.0")

        deltas = coord._compute_sensor_deltas()

        assert deltas == {}
        assert coord._last_sensor_readings["production"] == 100.0

    def test_second_call_computes_delta(self):
        """Second reading should compute correct delta."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
        )
        _set_sensor_state(coord, "sensor.solar_production", "100.0")
        coord._compute_sensor_deltas()  # baseline

        _set_sensor_state(coord, "sensor.solar_production", "105.5")
        deltas = coord._compute_sensor_deltas()

        assert deltas == {"production": 5.5}

    def test_negative_delta_skipped_on_sensor_reset(self):
        """Negative delta (sensor reset) should be skipped."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
        )
        _set_sensor_state(coord, "sensor.solar_production", "500.0")
        coord._compute_sensor_deltas()

        # Sensor resets to 0
        _set_sensor_state(coord, "sensor.solar_production", "0.0")
        deltas = coord._compute_sensor_deltas()

        assert deltas == {}
        assert coord._last_sensor_readings["production"] == 0.0

    def test_loads_from_storage_on_restart(self):
        """After restart, should load last readings from storage."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
            stored_readings={"production": 1000.0},
        )
        _set_sensor_state(coord, "sensor.solar_production", "1002.5")

        deltas = coord._compute_sensor_deltas()

        assert deltas == {"production": 2.5}

    def test_multiple_sensors(self):
        """Should compute deltas for all configured sensors."""
        coord = _make_coordinator(
            sensor_config={
                "production_sensor": "sensor.solar_production",
                "grid_export_sensor": "sensor.grid_export",
                "grid_import_sensor": "sensor.grid_import",
            },
            stored_readings={
                "production": 1000.0,
                "grid_export": 500.0,
                "grid_import": 2000.0,
            },
        )
        _set_sensor_state(coord, "sensor.solar_production", "1005.0")
        _set_sensor_state(coord, "sensor.grid_export", "502.0")
        _set_sensor_state(coord, "sensor.grid_import", "2003.0")

        deltas = coord._compute_sensor_deltas()

        assert deltas == {
            "production": 5.0,
            "grid_export": 2.0,
            "grid_import": 3.0,
        }

    def test_unavailable_sensor_skipped(self):
        """Sensors in unavailable state should be skipped."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
            stored_readings={"production": 100.0},
        )
        state_obj = MagicMock()
        state_obj.state = "unavailable"
        coord.hass.states.get.return_value = state_obj

        deltas = coord._compute_sensor_deltas()

        assert deltas == {}

    def test_unconfigured_sensor_skipped(self):
        """Sensors not configured should not appear in deltas."""
        coord = _make_coordinator(sensor_config={})

        deltas = coord._compute_sensor_deltas()

        assert deltas == {}

    def test_persists_readings_to_storage(self):
        """Should persist last readings to storage for restart recovery."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
        )
        _set_sensor_state(coord, "sensor.solar_production", "100.0")
        coord._compute_sensor_deltas()

        # Verify the storage setter was called with the readings
        coord._storage.__setattr__("last_sensor_readings", {"production": 100.0})
        assert coord._last_sensor_readings["production"] == 100.0

    def test_zero_delta(self):
        """Zero delta (no change) should be included."""
        coord = _make_coordinator(
            sensor_config={"production_sensor": "sensor.solar_production"},
            stored_readings={"production": 100.0},
        )
        _set_sensor_state(coord, "sensor.solar_production", "100.0")

        deltas = coord._compute_sensor_deltas()

        assert deltas == {"production": 0.0}


class TestEnrichRecordWithDeltas:
    """Tests for _enrich_record_with_deltas()."""

    def test_production_and_export_deltas(self):
        """Should compute own_use = production - grid_export."""
        coord = _make_coordinator()
        record = {
            "unit_price_buy": 0.50,
            "unit_price_sold": 0.60,
            "production_sold": 1.0,
        }
        deltas = {"production": 5.0, "grid_export": 2.0}

        coord._enrich_record_with_deltas(record, deltas)

        assert record["production_sold"] == 2.0
        assert record["production_sold_profit"] == pytest.approx(1.2)
        assert record["production_own_use"] == 3.0
        assert record["production_own_use_profit"] == pytest.approx(1.5)

    def test_production_delta_without_export_sensor(self):
        """Without export sensor, own_use uses Tibber's production_sold."""
        coord = _make_coordinator()
        record = {
            "unit_price_buy": 0.50,
            "production_sold": 1.5,
        }
        deltas = {"production": 4.0}

        coord._enrich_record_with_deltas(record, deltas)

        assert record["production_own_use"] == 2.5
        assert record["production_own_use_profit"] == pytest.approx(1.25)

    def test_grid_import_delta(self):
        """Should override purchased with delta value."""
        coord = _make_coordinator()
        record = {
            "unit_price_buy": 0.50,
            "purchased": 10.0,
        }
        deltas = {"grid_import": 3.0}

        coord._enrich_record_with_deltas(record, deltas)

        assert record["purchased"] == 3.0
        assert record["purchased_cost"] == pytest.approx(1.5)

    def test_battery_deltas(self):
        """Should apply battery charge and discharge deltas."""
        coord = _make_coordinator()
        record = {"unit_price_buy": 0.50}
        deltas = {"battery_charge": 1.5, "battery_discharge": 0.8}

        coord._enrich_record_with_deltas(record, deltas)

        assert record["battery_charge"] == 1.5
        assert record["battery_used"] == 0.8
        assert record["battery_used_profit"] == pytest.approx(0.4)

    def test_empty_deltas_no_changes(self):
        """Empty deltas should not modify the record."""
        coord = _make_coordinator()
        record = {"production_sold": 1.0, "purchased": 2.0}
        original = dict(record)

        coord._enrich_record_with_deltas(record, {})

        assert record == original

    def test_own_use_clamped_to_zero(self):
        """Own use should never go negative."""
        coord = _make_coordinator()
        record = {"unit_price_buy": 0.50, "unit_price_sold": 0.60}
        deltas = {"production": 1.0, "grid_export": 3.0}

        coord._enrich_record_with_deltas(record, deltas)

        assert record["production_own_use"] == 0.0
