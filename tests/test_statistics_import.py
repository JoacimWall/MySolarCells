"""Tests for historical statistics import."""

from __future__ import annotations

from datetime import datetime, timezone

import pytest

from custom_components.my_solar_cells.financial_engine import (
    CalcParams,
    calculate_daily_stats,
    get_calc_params_for_year,
)
from custom_components.my_solar_cells.statistics_import import (
    build_statistics_list,
    calculate_hourly_financial_stats,
    enrich_records_with_production_own_use,
)


def _make_record(
    ts: str,
    production_sold: float = 0.0,
    purchased: float = 0.0,
    production_own_use: float = 0.0,
    unit_price_buy: float = 0.50,
    unit_price_sold: float = 0.60,
) -> dict:
    """Create a minimal hourly record for testing."""
    return {
        "timestamp": ts,
        "production_sold": production_sold,
        "production_sold_profit": production_sold * unit_price_sold,
        "purchased": purchased,
        "purchased_cost": purchased * unit_price_buy,
        "production_own_use": production_own_use,
        "production_own_use_profit": production_own_use * unit_price_buy,
        "unit_price_buy": unit_price_buy,
        "unit_price_sold": unit_price_sold,
        "battery_charge": 0.0,
        "battery_used": 0.0,
        "battery_used_profit": 0.0,
    }


@pytest.fixture
def default_params() -> CalcParams:
    return CalcParams(
        tax_reduction=0.60,
        grid_compensation=0.078,
        transfer_fee=0.30,
        energy_tax=0.49,
        use_spot_price=True,
    )


# --- enrich_records_with_production_own_use ---


class TestEnrichProductionOwnUse:
    def test_basic_enrichment(self):
        """production_own_use = max(0, prod_delta - production_sold)."""
        records = {
            "2024-06-15T10:00:00": _make_record(
                "2024-06-15T10:00:00", production_sold=2.0, unit_price_buy=0.50
            ),
            "2024-06-15T11:00:00": _make_record(
                "2024-06-15T11:00:00", production_sold=3.0, unit_price_buy=0.60
            ),
        }
        prod_deltas = {
            "2024-06-15T10:00:00": 5.0,  # 5.0 - 2.0 = 3.0 own use
            "2024-06-15T11:00:00": 4.0,  # 4.0 - 3.0 = 1.0 own use
        }

        count = enrich_records_with_production_own_use(records, prod_deltas)

        assert count == 2
        assert records["2024-06-15T10:00:00"]["production_own_use"] == 3.0
        assert records["2024-06-15T10:00:00"]["production_own_use_profit"] == 3.0 * 0.50
        assert records["2024-06-15T11:00:00"]["production_own_use"] == 1.0
        assert records["2024-06-15T11:00:00"]["production_own_use_profit"] == 1.0 * 0.60

    def test_own_use_never_negative(self):
        """production_own_use should be max(0, ...) — never negative."""
        records = {
            "2024-06-15T10:00:00": _make_record(
                "2024-06-15T10:00:00", production_sold=5.0, unit_price_buy=0.50
            ),
        }
        # prod_delta < production_sold → own_use should be 0
        prod_deltas = {"2024-06-15T10:00:00": 3.0}

        enrich_records_with_production_own_use(records, prod_deltas)

        assert records["2024-06-15T10:00:00"]["production_own_use"] == 0
        assert records["2024-06-15T10:00:00"]["production_own_use_profit"] == 0

    def test_no_matching_deltas(self):
        """Records without matching deltas should not be modified."""
        records = {
            "2024-06-15T10:00:00": _make_record(
                "2024-06-15T10:00:00", production_sold=2.0
            ),
        }
        prod_deltas = {"2024-06-15T11:00:00": 5.0}  # different timestamp

        count = enrich_records_with_production_own_use(records, prod_deltas)

        assert count == 0
        assert records["2024-06-15T10:00:00"]["production_own_use"] == 0

    def test_empty_deltas(self):
        """Empty deltas dict should not enrich anything."""
        records = {
            "2024-06-15T10:00:00": _make_record("2024-06-15T10:00:00"),
        }

        count = enrich_records_with_production_own_use(records, {})

        assert count == 0


# --- calculate_hourly_financial_stats ---


class TestCalculateHourlyFinancialStats:
    def test_basic_calculation(self, default_params):
        """Each record gets its own HistoryStats from calculate_daily_stats."""
        records = [
            _make_record("2024-06-15T10:00:00", production_sold=2.0, purchased=1.0),
            _make_record("2024-06-15T11:00:00", production_sold=3.0, purchased=0.5),
        ]

        result = calculate_hourly_financial_stats(records, default_params, None)

        assert len(result) == 2
        assert "2024-06-15T10:00:00" in result
        assert "2024-06-15T11:00:00" in result

        stats_10 = result["2024-06-15T10:00:00"]
        assert stats_10.production_sold == 2.0
        assert stats_10.purchased == 1.0

        stats_11 = result["2024-06-15T11:00:00"]
        assert stats_11.production_sold == 3.0
        assert stats_11.purchased == 0.5

    def test_yearly_params_override(self, default_params):
        """Per-year parameter overrides should be applied."""
        records = [
            _make_record("2024-06-15T10:00:00", production_sold=10.0),
            _make_record("2025-06-15T10:00:00", production_sold=10.0),
        ]
        yearly_params = {
            "2025": {"tax_reduction": 0.0},  # No tax reduction in 2025
        }

        result = calculate_hourly_financial_stats(
            records, default_params, yearly_params
        )

        stats_2024 = result["2024-06-15T10:00:00"]
        stats_2025 = result["2025-06-15T10:00:00"]

        # 2024 should have tax reduction: 10.0 * 0.60 = 6.0
        assert stats_2024.production_sold_tax_reduction_profit == round(10.0 * 0.60, 2)
        # 2025 should have no tax reduction
        assert stats_2025.production_sold_tax_reduction_profit == 0.0

    def test_empty_records(self, default_params):
        """Empty records list should return empty dict."""
        result = calculate_hourly_financial_stats([], default_params, None)
        assert result == {}

    def test_financial_values_include_fees(self, default_params):
        """sum_purchased_cost should include transfer fee and energy tax."""
        records = [
            _make_record("2024-06-15T10:00:00", purchased=10.0, unit_price_buy=0.50),
        ]

        result = calculate_hourly_financial_stats(records, default_params, None)
        stats = result["2024-06-15T10:00:00"]

        # purchased_cost = 10 * 0.50 = 5.0
        # transfer_fee = 10 * 0.30 = 3.0
        # energy_tax = 10 * 0.49 = 4.9
        # sum_purchased_cost = 5.0 + 3.0 + 4.9 = 12.9
        assert stats.sum_purchased_cost == 12.9


# --- build_statistics_list ---


class TestBuildStatisticsList:
    def test_cumulative_sum_monotonically_increasing(self, default_params):
        """Cumulative sum should never decrease."""
        records = [
            _make_record("2024-06-15T10:00:00", production_sold=2.0),
            _make_record("2024-06-15T11:00:00", production_sold=3.0),
            _make_record("2024-06-15T12:00:00", production_sold=1.0),
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "production_sold")

        sums = [cumsum for _, _, cumsum in result]
        assert sums == [2.0, 5.0, 6.0]
        # Verify monotonically increasing
        for i in range(1, len(sums)):
            assert sums[i] >= sums[i - 1]

    def test_daily_state_resets_at_midnight(self, default_params):
        """daily_state should reset to 0 at day boundaries."""
        records = [
            _make_record("2024-06-15T22:00:00", production_sold=2.0),
            _make_record("2024-06-15T23:00:00", production_sold=3.0),
            _make_record("2024-06-16T00:00:00", production_sold=1.0),
            _make_record("2024-06-16T01:00:00", production_sold=4.0),
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "production_sold")

        states = [state for _, state, _ in result]
        # Day 1: 2.0, 5.0
        # Day 2: resets → 1.0, 5.0
        assert states == [2.0, 5.0, 1.0, 5.0]

        # But cumulative sum never resets
        sums = [cumsum for _, _, cumsum in result]
        assert sums == [2.0, 5.0, 6.0, 10.0]

    def test_datetime_output_is_utc(self, default_params):
        """Output datetimes should be UTC."""
        records = [_make_record("2024-06-15T10:00:00", production_sold=1.0)]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "production_sold")

        dt = result[0][0]
        assert dt.tzinfo == timezone.utc
        assert dt == datetime(2024, 6, 15, 10, 0, 0, tzinfo=timezone.utc)

    def test_empty_stats(self, default_params):
        """Empty input should produce empty output."""
        result = build_statistics_list({}, "production_sold")
        assert result == []

    def test_balance_can_be_negative(self, default_params):
        """Balance (profit - cost) can be negative; cumsum should still work."""
        records = [
            _make_record(
                "2024-06-15T10:00:00",
                production_sold=0.0,
                purchased=10.0,
                unit_price_buy=1.0,
            ),
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "balance")

        # balance = production_profit - purchased_cost, should be negative
        assert len(result) == 1
        _, state, cumsum = result[0]
        assert state < 0
        assert cumsum < 0

    def test_sorted_output(self, default_params):
        """Output should be sorted by timestamp even if input is unordered."""
        records = [
            _make_record("2024-06-15T12:00:00", production_sold=3.0),
            _make_record("2024-06-15T10:00:00", production_sold=1.0),
            _make_record("2024-06-15T11:00:00", production_sold=2.0),
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "production_sold")

        dts = [dt for dt, _, _ in result]
        assert dts == sorted(dts)

    def test_multi_day_cumulative(self, default_params):
        """Cumulative sum should span across multiple days."""
        records = [
            _make_record("2024-06-15T10:00:00", production_sold=5.0),
            _make_record("2024-06-16T10:00:00", production_sold=3.0),
            _make_record("2024-06-17T10:00:00", production_sold=7.0),
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        result = build_statistics_list(hourly_stats, "production_sold")

        states = [state for _, state, _ in result]
        sums = [cumsum for _, _, cumsum in result]

        # Each day has one record, so daily_state == the record's value
        assert states == [5.0, 3.0, 7.0]
        # Cumulative keeps growing
        assert sums == [5.0, 8.0, 15.0]


class TestGracefulHandling:
    """Test that the system handles missing data gracefully."""

    def test_records_without_production_own_use(self, default_params):
        """Records without production_own_use should default to 0."""
        records = [
            {
                "timestamp": "2024-06-15T10:00:00",
                "production_sold": 5.0,
                "production_sold_profit": 3.0,
                "purchased": 1.0,
                "purchased_cost": 0.5,
                "unit_price_buy": 0.50,
                "unit_price_sold": 0.60,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            },
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        stats = hourly_stats["2024-06-15T10:00:00"]

        # production_own_use should default to 0 when not in record
        assert stats.production_own_use == 0.0
        assert stats.sum_production_own_use_saved == 0.0

    def test_records_missing_optional_fields(self, default_params):
        """Minimal record with only required fields should not crash."""
        records = [
            {
                "timestamp": "2024-06-15T10:00:00",
            },
        ]

        hourly_stats = calculate_hourly_financial_stats(records, default_params, None)
        assert "2024-06-15T10:00:00" in hourly_stats

        stats = hourly_stats["2024-06-15T10:00:00"]
        assert stats.production_sold == 0.0
        assert stats.purchased == 0.0
