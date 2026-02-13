"""Tests for the financial calculation engine."""

import pytest

from custom_components.my_solar_cells.financial_engine import (
    CalcParams,
    HistoryStats,
    calculate_daily_stats,
    calculate_period,
    generate_monthly_report,
    get_calc_params_for_year,
    summarize_stats,
)


class TestCalculateDailyStats:
    """Tests for calculate_daily_stats matching CalculateTotalsInternal logic."""

    def test_empty_records(self, sample_calc_params):
        """Test with no records returns zero stats."""
        stats = calculate_daily_stats([], sample_calc_params)
        assert stats.purchased == 0.0
        assert stats.production_sold == 0.0
        assert stats.production_own_use == 0.0

    def test_purchased_calculation(self, sample_hourly_records, sample_calc_params):
        """Test purchased kWh and cost calculation (lines 270-273)."""
        stats = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        # 1.5 + 0.8 + 0.5 = 2.8
        assert stats.purchased == 2.8
        # Spot price: 0.75 + 0.48 + 0.20 = 1.43
        assert stats.purchased_cost == 1.43
        # Transfer fee: 2.8 * 0.30 = 0.84
        assert stats.purchased_transfer_fee_cost == 0.84
        # Energy tax: 2.8 * 0.49 = 1.372 -> 1.37
        assert stats.purchased_tax_cost == 1.37

    def test_production_sold_calculation(self, sample_hourly_records, sample_calc_params):
        """Test production sold kWh and profit (lines 275-281)."""
        stats = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        # 2.0 + 3.5 + 5.0 = 10.5
        assert stats.production_sold == 10.5
        # Spot price profit: 1.20 + 2.10 + 3.00 = 6.30
        assert stats.production_sold_profit == 6.3
        # Grid compensation: 10.5 * 0.078 = 0.819 -> 0.82
        assert stats.production_sold_grid_compensation_profit == 0.82
        # Tax reduction: 10.5 * 0.60 = 6.30
        assert stats.production_sold_tax_reduction_profit == 6.3

    def test_production_own_use_calculation(self, sample_hourly_records, sample_calc_params):
        """Test own use kWh and savings (lines 284-287)."""
        stats = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        # 3.0 + 4.0 + 5.5 = 12.5
        assert stats.production_own_use == 12.5
        # Spot price saved: 1.80 + 2.40 + 3.30 = 7.50
        assert stats.production_own_use_saved == 7.5
        # Transfer fee saved: 12.5 * 0.30 = 3.75
        assert stats.production_own_use_transfer_fee_saved == 3.75
        # Energy tax saved: 12.5 * 0.49 = 6.125 -> 6.12
        assert stats.production_own_use_energy_tax_saved == 6.12

    def test_battery_calculation(self, sample_hourly_records, sample_calc_params):
        """Test battery charge and use (lines 290-296)."""
        stats = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        # Battery used: 0 + 0.5 + 0 = 0.5
        assert stats.battery_used == 0.5
        # Battery charge: 0 + 1.0 + 2.0 = 3.0
        assert stats.battery_charge == 3.0
        # Battery saved (spot): 0 + 0.25 + 0 = 0.25
        assert stats.battery_used_saved == 0.25

    def test_fixed_price_mode(self, sample_hourly_records):
        """Test calculations with fixed price instead of spot."""
        params = CalcParams(
            use_spot_price=False,
            fixed_price=0.55,
            tax_reduction=0.60,
            grid_compensation=0.078,
            transfer_fee=0.30,
            energy_tax=0.49,
        )
        stats = calculate_daily_stats(sample_hourly_records, params)
        # Purchased cost with fixed: 2.8 * 0.55 = 1.54
        assert stats.purchased_cost == 1.54
        # Production sold profit with fixed: 10.5 * 0.55 = 5.775 -> 5.78
        assert stats.production_sold_profit == 5.78
        # Own use saved with fixed: 12.5 * 0.55 = 6.875 -> 6.88
        assert stats.production_own_use_saved == 6.88

    def test_sum_properties(self, sample_hourly_records, sample_calc_params):
        """Test computed sum properties."""
        stats = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        # sum_purchased_cost = 1.43 + 0.84 + 1.37 = 3.64
        assert stats.sum_purchased_cost == 3.64
        # sum_production_sold_profit = 6.30 + 0.82 + 6.30 = 13.42
        assert stats.sum_production_sold_profit == 13.42
        # sum_production_own_use_saved = 7.50 + 3.75 + 6.12 = 17.37
        assert stats.sum_production_own_use_saved == 17.37


class TestSummarizeStats:
    """Tests for summarize_stats matching SummarizeToOneRoiStats logic."""

    def test_summarize_single_day(self, sample_hourly_records, sample_calc_params):
        """Test summarizing a single day's stats."""
        daily = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        result = summarize_stats([daily], 1.0)
        assert result.purchased == daily.purchased
        assert result.production_sold == daily.production_sold

    def test_summarize_averages(self, sample_hourly_records, sample_calc_params):
        """Test that averages are computed correctly."""
        daily = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        result = summarize_stats([daily, daily], 2.0)
        # Values should be doubled
        assert result.purchased == round(daily.purchased * 2, 2)
        # Production index = total production / days
        expected_prod_index = round(result.sum_all_production / 2.0, 2)
        assert result.facts_production_index == expected_prod_index

    def test_avg_price_per_kwh(self, sample_hourly_records, sample_calc_params):
        """Test average price per kWh calculations."""
        daily = calculate_daily_stats(sample_hourly_records, sample_calc_params)
        result = summarize_stats([daily], 1.0)
        if result.purchased > 0:
            expected = round(result.sum_purchased_cost / result.purchased, 2)
            assert result.facts_purchased_cost_avg_per_kwh == expected
        if result.production_sold > 0:
            expected = round(result.sum_production_sold_profit / result.production_sold, 2)
            assert result.facts_production_sold_avg_per_kwh_profit == expected


class TestCalculatePeriod:
    """Tests for calculate_period."""

    def test_period_calculation(self, sample_hourly_records, sample_calc_params):
        """Test period calculation groups by date correctly."""
        result = calculate_period(sample_hourly_records, sample_calc_params)
        # All records are on the same day, so should be same as daily
        assert result.purchased == 2.8
        assert result.production_sold == 10.5


class TestGenerateMonthlyReport:
    """Tests for generate_monthly_report with tax reduction cap."""

    def test_monthly_report_structure(self, sample_yearly_records, sample_calc_params):
        """Test that monthly report generates correct structure."""
        yearly, monthly = generate_monthly_report(
            sample_yearly_records, sample_calc_params, investment=200000
        )
        # Should have 1 year
        assert len(yearly) == 1
        assert yearly[0].from_date.year == 2024
        # Should have 12 months in that year
        assert len(monthly) == 1
        assert len(monthly[0]) == 12

    def test_tax_reduction_cap(self, sample_calc_params):
        """Test tax reduction cap when production > purchased (lines 87-97)."""
        # Create records where production_sold > purchased
        records = [
            {
                "timestamp": "2024-06-15T12:00:00+01:00",
                "purchased": 10.0,
                "purchased_cost": 5.0,
                "production_sold": 100.0,
                "production_sold_profit": 60.0,
                "production_own_use": 50.0,
                "production_own_use_profit": 25.0,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
                "unit_price_buy": 0.50,
                "unit_price_sold": 0.60,
                "unit_price_vat_buy": 0.0,
                "unit_price_vat_sold": 0.0,
                "price_level": "NORMAL",
                "synced": True,
            }
        ]
        yearly, _monthly = generate_monthly_report(records, sample_calc_params, 200000)
        assert len(yearly) == 1
        # Tax reduction should be capped: purchased * tax_reduction = 10.0 * 0.60 = 6.0
        assert yearly[0].history_stats.production_sold_tax_reduction_profit == 6.0
        assert "capped" in yearly[0].history_stats.production_sold_tax_reduction_comment


class TestYearAwareCalculations:
    """Tests for per-year financial parameter overrides."""

    def test_get_calc_params_no_overrides(self, sample_calc_params):
        """Without yearly_params, returns base_params unchanged."""
        result = get_calc_params_for_year("2024", sample_calc_params, None)
        assert result is sample_calc_params

    def test_get_calc_params_empty_dict(self, sample_calc_params):
        """Empty yearly_params dict returns base_params unchanged."""
        result = get_calc_params_for_year("2024", sample_calc_params, {})
        assert result is sample_calc_params

    def test_get_calc_params_missing_year(self, sample_calc_params):
        """Year not in yearly_params returns base_params unchanged."""
        yearly = {"2023": {"energy_tax": 0.45}}
        result = get_calc_params_for_year("2024", sample_calc_params, yearly)
        assert result is sample_calc_params

    def test_get_calc_params_with_override(self, sample_calc_params):
        """Year in yearly_params returns new CalcParams with overrides."""
        yearly = {"2023": {"energy_tax": 0.45, "transfer_fee": 0.28}}
        result = get_calc_params_for_year("2023", sample_calc_params, yearly)
        # Should be a different object
        assert result is not sample_calc_params
        # Overridden values
        assert result.energy_tax == 0.45
        assert result.transfer_fee == 0.28
        # Non-overridden values preserved
        assert result.tax_reduction == sample_calc_params.tax_reduction
        assert result.grid_compensation == sample_calc_params.grid_compensation
        assert result.installed_kw == sample_calc_params.installed_kw

    def test_get_calc_params_does_not_mutate_base(self, sample_calc_params):
        """Overrides must not mutate the original base_params."""
        yearly = {"2023": {"energy_tax": 0.45}}
        get_calc_params_for_year("2023", sample_calc_params, yearly)
        assert sample_calc_params.energy_tax == 0.49

    def test_calculate_period_with_yearly_params(self, sample_calc_params):
        """calculate_period uses per-year params for daily calculations."""
        # Records in 2023 and 2024 with different years
        records = [
            {
                "timestamp": "2023-06-15T12:00:00+02:00",
                "purchased": 10.0,
                "purchased_cost": 5.0,
                "production_sold": 5.0,
                "production_sold_profit": 3.0,
                "production_own_use": 5.0,
                "production_own_use_profit": 2.5,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            },
            {
                "timestamp": "2024-06-15T12:00:00+02:00",
                "purchased": 10.0,
                "purchased_cost": 5.0,
                "production_sold": 5.0,
                "production_sold_profit": 3.0,
                "production_own_use": 5.0,
                "production_own_use_profit": 2.5,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            },
        ]
        yearly = {
            "2023": {"energy_tax": 0.45, "transfer_fee": 0.28},
            "2024": {"energy_tax": 0.49, "transfer_fee": 0.30},
        }
        result = calculate_period(records, sample_calc_params, yearly)
        # 2023 day: purchased_tax = 10.0 * 0.45 = 4.50
        # 2024 day: purchased_tax = 10.0 * 0.49 = 4.90
        # Total: 9.40
        assert result.purchased_tax_cost == 9.4
        # 2023 day: purchased_transfer = 10.0 * 0.28 = 2.80
        # 2024 day: purchased_transfer = 10.0 * 0.30 = 3.00
        # Total: 5.80
        assert result.purchased_transfer_fee_cost == 5.8

    def test_calculate_period_none_yearly_params(self, sample_hourly_records, sample_calc_params):
        """calculate_period with yearly_params=None works like before."""
        result_without = calculate_period(sample_hourly_records, sample_calc_params)
        result_with_none = calculate_period(sample_hourly_records, sample_calc_params, None)
        assert result_without.purchased_tax_cost == result_with_none.purchased_tax_cost
        assert result_without.purchased_transfer_fee_cost == result_with_none.purchased_transfer_fee_cost

    def test_generate_monthly_report_with_yearly_params(self, sample_calc_params):
        """generate_monthly_report uses year-specific params for tax cap."""
        # Create records where production_sold > purchased for 2023
        records = [
            {
                "timestamp": "2023-06-15T12:00:00+01:00",
                "purchased": 10.0,
                "purchased_cost": 5.0,
                "production_sold": 100.0,
                "production_sold_profit": 60.0,
                "production_own_use": 50.0,
                "production_own_use_profit": 25.0,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            }
        ]
        yearly = {"2023": {"tax_reduction": 0.50}}
        yearly_overview, _ = generate_monthly_report(
            records, sample_calc_params, 200000, yearly
        )
        assert len(yearly_overview) == 1
        # Tax cap should use 2023 override: 10.0 * 0.50 = 5.0
        assert yearly_overview[0].history_stats.production_sold_tax_reduction_profit == 5.0

    def test_multi_year_records_use_respective_params(self, sample_calc_params):
        """Records spanning multiple years each use their year's params."""
        records = [
            {
                "timestamp": "2023-03-15T12:00:00+01:00",
                "purchased": 5.0,
                "purchased_cost": 2.5,
                "production_sold": 3.0,
                "production_sold_profit": 1.8,
                "production_own_use": 2.0,
                "production_own_use_profit": 1.0,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            },
            {
                "timestamp": "2024-03-15T12:00:00+01:00",
                "purchased": 5.0,
                "purchased_cost": 2.5,
                "production_sold": 3.0,
                "production_sold_profit": 1.8,
                "production_own_use": 2.0,
                "production_own_use_profit": 1.0,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            },
        ]
        yearly = {
            "2023": {"grid_compensation": 0.07},
            "2024": {"grid_compensation": 0.09},
        }
        result = calculate_period(records, sample_calc_params, yearly)
        # 2023: 3.0 * 0.07 = 0.21
        # 2024: 3.0 * 0.09 = 0.27
        # Total: 0.48
        assert result.production_sold_grid_compensation_profit == 0.48
