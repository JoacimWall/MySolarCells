"""Tests for SQLite database manager."""

from __future__ import annotations

import json
import os
import sqlite3
import tempfile
from unittest.mock import MagicMock

import pytest

# The conftest mocks homeassistant modules, but we need a real hass mock
# with async_add_executor_job that runs the function synchronously.
from custom_components.my_solar_cells.database import MySolarCellsDatabase


def _make_hass(tmp_dir: str) -> MagicMock:
    """Create a mock hass object with config.path() returning tmp_dir paths."""
    hass = MagicMock()
    hass.config.path = lambda *args: os.path.join(tmp_dir, *args)

    # Make async_add_executor_job run the function synchronously (for sync tests)
    async def _run_executor(fn, *args):
        return fn(*args)

    hass.async_add_executor_job = MagicMock(side_effect=_run_executor)
    return hass


@pytest.fixture
def db_instance(tmp_path):
    """Create and set up a database instance in a temp directory."""
    hass = _make_hass(str(tmp_path))
    db = MySolarCellsDatabase(hass, "test_entry")
    # Run setup synchronously
    db._setup_sync()
    yield db
    db._close_sync()


def _sample_record(
    ts: str = "2024-06-15T12:00:00",
    production_sold: float = 2.0,
    purchased: float = 1.0,
    **kwargs,
) -> dict:
    """Create a sample hourly record."""
    record = {
        "timestamp": ts,
        "purchased": purchased,
        "purchased_cost": purchased * 0.50,
        "production_sold": production_sold,
        "production_sold_profit": production_sold * 0.60,
        "unit_price_buy": 0.50,
        "unit_price_vat_buy": 0.625,
        "unit_price_sold": 0.60,
        "unit_price_vat_sold": 0.75,
        "production_own_use": 3.0,
        "production_own_use_profit": 1.50,
        "battery_charge": 0.0,
        "battery_used": 0.0,
        "battery_used_profit": 0.0,
        "synced": True,
        "sensor_enriched": False,
        "price_level": "NORMAL",
    }
    record.update(kwargs)
    return record


class TestHourlyRecords:
    def test_upsert_and_retrieve(self, db_instance):
        """Upsert a record and retrieve it."""
        record = _sample_record()
        db_instance.upsert_hourly_record("2024-06-15T12:00:00", record)
        db_instance._conn.commit()

        result = db_instance.get_hourly_record("2024-06-15T12:00:00")
        assert result is not None
        assert result["production_sold"] == 2.0
        assert result["purchased"] == 1.0
        assert result["price_level"] == "NORMAL"

    def test_upsert_replaces_existing(self, db_instance):
        """Upserting with same timestamp replaces the record."""
        db_instance.upsert_hourly_record(
            "2024-06-15T12:00:00", _sample_record(production_sold=2.0)
        )
        db_instance.upsert_hourly_record(
            "2024-06-15T12:00:00", _sample_record(production_sold=5.0)
        )
        db_instance._conn.commit()

        result = db_instance.get_hourly_record("2024-06-15T12:00:00")
        assert result["production_sold"] == 5.0

    def test_get_nonexistent_record(self, db_instance):
        """Getting a nonexistent record returns None."""
        result = db_instance.get_hourly_record("2099-01-01T00:00:00")
        assert result is None

    def test_get_records_for_period(self, db_instance):
        """Get records within a date range."""
        for hour in range(6, 18):
            db_instance.upsert_hourly_record(
                f"2024-06-15T{hour:02d}:00:00",
                _sample_record(ts=f"2024-06-15T{hour:02d}:00:00"),
            )
        db_instance._conn.commit()

        # Get records from 10:00 to 14:00 (exclusive)
        results = db_instance.get_records_for_period(
            "2024-06-15T10:00:00", "2024-06-15T14:00:00"
        )
        assert len(results) == 4
        assert results[0]["timestamp"] == "2024-06-15T10:00:00"
        assert results[-1]["timestamp"] == "2024-06-15T13:00:00"

    def test_get_records_for_period_empty(self, db_instance):
        """Empty period returns empty list."""
        results = db_instance.get_records_for_period(
            "2099-01-01T00:00:00", "2099-01-02T00:00:00"
        )
        assert results == []

    def test_get_all_records_sorted(self, db_instance):
        """All records are returned sorted by timestamp."""
        # Insert out of order
        for hour in [15, 10, 12, 8]:
            db_instance.upsert_hourly_record(
                f"2024-06-15T{hour:02d}:00:00",
                _sample_record(ts=f"2024-06-15T{hour:02d}:00:00"),
            )
        db_instance._conn.commit()

        results = db_instance.get_all_records_as_list()
        timestamps = [r["timestamp"] for r in results]
        assert timestamps == sorted(timestamps)
        assert len(results) == 4


class TestSpotPrices:
    def test_upsert_and_retrieve(self, db_instance):
        """Upsert spot prices and retrieve for a date."""
        for quarter in range(4):
            ts = f"2024-06-15T10:{quarter * 15:02d}:00"
            db_instance.upsert_quarter_hourly_price(ts, {
                "total": 0.50 + quarter * 0.01,
                "energy": 0.40,
                "tax": 0.10,
                "level": "NORMAL",
                "currency": "SEK",
            })
        db_instance._conn.commit()

        prices = db_instance.get_prices_for_date("2024-06-15")
        assert len(prices) == 4
        assert prices[0]["starts_at"] == "2024-06-15T10:00:00"
        assert prices[0]["total"] == 0.50

    def test_get_prices_for_date_empty(self, db_instance):
        """No prices for a date returns empty list."""
        prices = db_instance.get_prices_for_date("2099-01-01")
        assert prices == []

    def test_get_average_price_for_hour(self, db_instance):
        """Average of 4 quarter-hourly prices for an hour."""
        prices = [0.40, 0.50, 0.60, 0.70]
        for i, price in enumerate(prices):
            ts = f"2024-06-15T10:{i * 15:02d}:00"
            db_instance.upsert_quarter_hourly_price(ts, {"total": price})
        db_instance._conn.commit()

        avg = db_instance.get_average_price_for_hour("2024-06-15T10")
        assert avg == pytest.approx(0.55)

    def test_get_average_price_no_data(self, db_instance):
        """No prices for an hour returns None."""
        avg = db_instance.get_average_price_for_hour("2099-01-01T10")
        assert avg is None


class TestMetadata:
    def test_last_tibber_sync(self, db_instance):
        """Get/set last_tibber_sync."""
        assert db_instance.last_tibber_sync is None

        db_instance.last_tibber_sync = "2024-06-15T12:00:00"
        db_instance._conn.commit()
        assert db_instance.last_tibber_sync == "2024-06-15T12:00:00"

    def test_last_tibber_sync_clear(self, db_instance):
        """Setting last_tibber_sync to None removes it."""
        db_instance.last_tibber_sync = "2024-06-15T12:00:00"
        db_instance._conn.commit()

        db_instance.last_tibber_sync = None
        db_instance._conn.commit()
        assert db_instance.last_tibber_sync is None

    def test_last_sensor_readings(self, db_instance):
        """Get/set last_sensor_readings as dict."""
        assert db_instance.last_sensor_readings == {}

        readings = {"production": 100.5, "grid_export": 50.2}
        db_instance.last_sensor_readings = readings
        db_instance._conn.commit()

        result = db_instance.last_sensor_readings
        assert result == readings

    def test_roi_projection(self, db_instance):
        """Get/set roi_projection as list."""
        assert db_instance.roi_projection == []

        projection = [{"year": 2024, "savings": 1000}, {"year": 2025, "savings": 2000}]
        db_instance.roi_projection = projection
        db_instance._conn.commit()

        result = db_instance.roi_projection
        assert result == projection

    def test_monthly_cache(self, db_instance):
        """Get/update monthly_cache."""
        assert db_instance.monthly_cache == {}

        db_instance.update_monthly_cache("2024-06", {"total": 500})
        db_instance._conn.commit()

        cache = db_instance.monthly_cache
        assert cache == {"2024-06": {"total": 500}}

        db_instance.update_monthly_cache("2024-07", {"total": 600})
        db_instance._conn.commit()

        cache = db_instance.monthly_cache
        assert cache == {
            "2024-06": {"total": 500},
            "2024-07": {"total": 600},
        }


class TestYearlyParams:
    def test_set_and_get(self, db_instance):
        """Set and retrieve yearly params for a year."""
        params = {
            "tax_reduction": 0.60,
            "grid_compensation": 0.10,
            "transfer_fee": 0.35,
            "energy_tax": 0.50,
            "installed_kw": 12.0,
        }
        db_instance.set_yearly_params(2024, params)
        db_instance._conn.commit()

        result = db_instance.get_yearly_params(2024)
        assert result is not None
        assert result["tax_reduction"] == 0.60
        assert result["grid_compensation"] == 0.10
        assert result["transfer_fee"] == 0.35
        assert result["energy_tax"] == 0.50
        assert result["installed_kw"] == 12.0

    def test_get_nonexistent_year(self, db_instance):
        """Getting params for a year with no data returns None."""
        result = db_instance.get_yearly_params(2099)
        assert result is None

    def test_get_all_yearly_params(self, db_instance):
        """Get all yearly params as dict keyed by year string."""
        db_instance.set_yearly_params(2023, {
            "tax_reduction": 0.60, "energy_tax": 0.42,
        })
        db_instance.set_yearly_params(2024, {
            "tax_reduction": 0.50, "energy_tax": 0.49,
        })
        db_instance._conn.commit()

        result = db_instance.get_all_yearly_params()
        assert "2023" in result
        assert "2024" in result
        assert result["2023"]["tax_reduction"] == 0.60
        assert result["2023"]["energy_tax"] == 0.42
        assert result["2024"]["tax_reduction"] == 0.50

    def test_get_all_yearly_params_empty(self, db_instance):
        """No yearly params returns empty dict."""
        result = db_instance.get_all_yearly_params()
        assert result == {}

    def test_upsert_replaces(self, db_instance):
        """Setting params for same year replaces previous values."""
        db_instance.set_yearly_params(2024, {"tax_reduction": 0.60})
        db_instance.set_yearly_params(2024, {"tax_reduction": 0.50})
        db_instance._conn.commit()

        result = db_instance.get_yearly_params(2024)
        assert result["tax_reduction"] == 0.50

    def test_delete_yearly_params(self, db_instance):
        """Delete removes params for a year."""
        db_instance.set_yearly_params(2024, {"tax_reduction": 0.60})
        db_instance._conn.commit()

        db_instance.delete_yearly_params(2024)
        db_instance._conn.commit()

        assert db_instance.get_yearly_params(2024) is None

    def test_partial_params(self, db_instance):
        """Only set fields are returned, None fields are omitted."""
        db_instance.set_yearly_params(2024, {
            "tax_reduction": 0.55,
        })
        db_instance._conn.commit()

        result = db_instance.get_yearly_params(2024)
        assert result is not None
        assert result["tax_reduction"] == 0.55
        # Fields not set should not be in the result
        assert "grid_compensation" not in result

class TestLifecycle:
    def test_setup_creates_db_file(self, tmp_path):
        """async_setup creates the database file."""
        hass = _make_hass(str(tmp_path))
        db = MySolarCellsDatabase(hass, "lifecycle_test")
        db._setup_sync()

        db_path = os.path.join(str(tmp_path), ".storage", "my_solar_cells_lifecycle_test.db")
        assert os.path.exists(db_path)
        db._close_sync()

    def test_remove_deletes_db_file(self, tmp_path):
        """async_remove deletes the database file."""
        hass = _make_hass(str(tmp_path))
        db = MySolarCellsDatabase(hass, "remove_test")
        db._setup_sync()

        db_path = db._db_path
        assert os.path.exists(db_path)

        db._remove_sync()
        assert not os.path.exists(db_path)

    def test_data_persists_across_connections(self, tmp_path):
        """Data written persists after close and reopen."""
        hass = _make_hass(str(tmp_path))

        # Write data
        db1 = MySolarCellsDatabase(hass, "persist_test")
        db1._setup_sync()
        db1.upsert_hourly_record("2024-06-15T12:00:00", _sample_record())
        db1.last_tibber_sync = "2024-06-15T12:00:00"
        db1._save_sync()
        db1._close_sync()

        # Reopen and verify
        db2 = MySolarCellsDatabase(hass, "persist_test")
        db2._setup_sync()
        record = db2.get_hourly_record("2024-06-15T12:00:00")
        assert record is not None
        assert record["production_sold"] == 2.0
        assert db2.last_tibber_sync == "2024-06-15T12:00:00"
        db2._close_sync()
