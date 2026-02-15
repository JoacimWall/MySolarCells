"""Tests for the battery simulation engine."""

import pytest

from custom_components.my_solar_cells.simulation_engine import (
    simulate_battery_add,
    simulate_battery_remove,
)


def _make_record(
    purchased=0.0,
    purchased_cost=0.0,
    production_sold=0.0,
    production_sold_profit=0.0,
    production_own_use=0.0,
    production_own_use_profit=0.0,
    battery_charge=0.0,
    battery_used=0.0,
    battery_used_profit=0.0,
    unit_price_buy=0.50,
    unit_price_sold=0.60,
    timestamp="2025-06-15T12:00:00+02:00",
):
    return {
        "timestamp": timestamp,
        "purchased": purchased,
        "purchased_cost": purchased_cost,
        "production_sold": production_sold,
        "production_sold_profit": production_sold_profit,
        "production_own_use": production_own_use,
        "production_own_use_profit": production_own_use_profit,
        "battery_charge": battery_charge,
        "battery_used": battery_used,
        "battery_used_profit": battery_used_profit,
        "unit_price_buy": unit_price_buy,
        "unit_price_sold": unit_price_sold,
    }


class TestSimulateBatteryAdd:
    """Tests for simulate_battery_add."""

    def test_charges_from_excess_production(self):
        """Battery charges when there is excess production_sold."""
        records = [_make_record(production_sold=3.0, production_sold_profit=1.80)]
        result = simulate_battery_add(records, max_battery_kwh=10.0)
        assert result[0]["battery_charge"] == 3.0
        assert result[0]["production_sold"] == 0.0

    def test_discharges_to_offset_purchased(self):
        """Battery discharges to cover purchased demand."""
        # First charge, then discharge
        records = [
            _make_record(
                production_sold=5.0,
                production_sold_profit=3.0,
                timestamp="2025-06-15T10:00:00+02:00",
            ),
            _make_record(
                purchased=3.0,
                purchased_cost=1.5,
                timestamp="2025-06-15T20:00:00+02:00",
            ),
        ]
        result = simulate_battery_add(records, max_battery_kwh=10.0)
        # First record: charged 5 kWh
        assert result[0]["battery_charge"] == 5.0
        assert result[0]["production_sold"] == 0.0
        # Second record: discharged 3 kWh (battery had 5, demand is 3, 5 > 3)
        assert result[1]["battery_used"] == 3.0
        assert result[1]["purchased"] == 0.0

    def test_does_not_exceed_capacity(self):
        """Battery does not charge beyond max capacity (all-or-nothing)."""
        records = [
            _make_record(
                production_sold=8.0,
                production_sold_profit=4.8,
                timestamp="2025-06-15T10:00:00+02:00",
            ),
            _make_record(
                production_sold=5.0,
                production_sold_profit=3.0,
                timestamp="2025-06-15T11:00:00+02:00",
            ),
        ]
        result = simulate_battery_add(records, max_battery_kwh=10.0)
        # First record: 8 kWh charges (0 + 8 <= 10)
        assert result[0]["battery_charge"] == 8.0
        # Second record: 5 kWh would exceed (8 + 5 = 13 > 10), so no charge
        assert result[1]["battery_charge"] == 0.0
        assert result[1]["production_sold"] == 5.0

    def test_does_not_mutate_input(self):
        """Original records should not be modified."""
        records = [_make_record(production_sold=3.0, production_sold_profit=1.80)]
        original_sold = records[0]["production_sold"]
        simulate_battery_add(records, max_battery_kwh=10.0)
        assert records[0]["production_sold"] == original_sold

    def test_battery_state_persists_across_records(self):
        """Battery charge level persists across hourly records."""
        records = [
            _make_record(
                production_sold=4.0,
                production_sold_profit=2.4,
                timestamp="2025-06-15T10:00:00+02:00",
            ),
            _make_record(
                production_sold=4.0,
                production_sold_profit=2.4,
                timestamp="2025-06-15T11:00:00+02:00",
            ),
            _make_record(
                purchased=3.0,
                purchased_cost=1.5,
                timestamp="2025-06-15T20:00:00+02:00",
            ),
        ]
        result = simulate_battery_add(records, max_battery_kwh=10.0)
        # First: charge 4 (total: 4)
        assert result[0]["battery_charge"] == 4.0
        # Second: charge 4 (total: 8)
        assert result[1]["battery_charge"] == 4.0
        # Third: discharge 3 (8 > 3, so discharge)
        assert result[2]["battery_used"] == 3.0
        assert result[2]["purchased"] == 0.0

    def test_no_discharge_when_charge_equals_demand(self):
        """Battery does NOT discharge when charge equals demand (strict >)."""
        records = [
            _make_record(
                production_sold=3.0,
                production_sold_profit=1.8,
                timestamp="2025-06-15T10:00:00+02:00",
            ),
            _make_record(
                purchased=3.0,
                purchased_cost=1.5,
                timestamp="2025-06-15T20:00:00+02:00",
            ),
        ]
        result = simulate_battery_add(records, max_battery_kwh=10.0)
        # Charged 3. Then demand is 3, but 3 > 3 is False => no discharge
        assert result[1]["battery_used"] == 0.0
        assert result[1]["purchased"] == 3.0

    def test_empty_records(self):
        """Empty input returns empty output."""
        result = simulate_battery_add([], max_battery_kwh=10.0)
        assert result == []


class TestSimulateBatteryRemove:
    """Tests for simulate_battery_remove."""

    def test_converts_battery_charge_to_sold(self):
        """battery_charge becomes production_sold."""
        records = [_make_record(battery_charge=2.0, production_sold=1.0, production_sold_profit=0.6)]
        result = simulate_battery_remove(records)
        assert result[0]["production_sold"] == 3.0
        assert result[0]["battery_charge"] == 0.0

    def test_converts_battery_used_to_purchased(self):
        """battery_used becomes purchased."""
        records = [_make_record(battery_used=1.5, battery_used_profit=0.75, purchased=0.5, purchased_cost=0.25)]
        result = simulate_battery_remove(records)
        assert result[0]["purchased"] == 2.0
        assert result[0]["battery_used"] == 0.0
        assert result[0]["battery_used_profit"] == 0.0

    def test_clears_battery_fields(self):
        """Both battery fields are zeroed out."""
        records = [_make_record(battery_charge=3.0, battery_used=2.0, battery_used_profit=1.0)]
        result = simulate_battery_remove(records)
        assert result[0]["battery_charge"] == 0.0
        assert result[0]["battery_used"] == 0.0
        assert result[0]["battery_used_profit"] == 0.0

    def test_does_not_mutate_input(self):
        """Original records should not be modified."""
        records = [_make_record(battery_charge=2.0)]
        original_charge = records[0]["battery_charge"]
        simulate_battery_remove(records)
        assert records[0]["battery_charge"] == original_charge

    def test_recalculates_profit_with_spot_prices(self):
        """Financial values use spot prices for recalculation."""
        records = [_make_record(
            battery_charge=2.0,
            unit_price_sold=0.60,
            battery_used=1.0,
            unit_price_buy=0.50,
        )]
        result = simulate_battery_remove(records)
        # Sold profit increased by 2.0 * 0.60 = 1.20
        assert abs(result[0]["production_sold_profit"] - 1.20) < 0.01
        # Purchased cost increased by 1.0 * 0.50 = 0.50
        assert abs(result[0]["purchased_cost"] - 0.50) < 0.01

    def test_empty_records(self):
        """Empty input returns empty output."""
        result = simulate_battery_remove([])
        assert result == []
