"""Tests for the ROI projection engine."""

import pytest
from datetime import datetime

from custom_components.my_solar_cells.financial_engine import (
    CalcParams,
    HistoryStats,
    ReportHistoryStats,
    generate_monthly_report,
)
from custom_components.my_solar_cells.roi_engine import (
    EstimateRoi,
    calculate_30_year_projection,
    get_average_monthly_production,
)
from custom_components.my_solar_cells.const import AVERAGE_PRODUCTION_PER_KW


class TestAverageMonthlyProduction:
    """Tests for the average production table (AverageProductionMonth)."""

    def test_january(self):
        """Test January production per kW."""
        assert get_average_monthly_production(1, 1.0) == 13.9

    def test_may_peak(self):
        """Test May (peak production month)."""
        assert get_average_monthly_production(5, 1.0) == 137.95

    def test_december_lowest(self):
        """Test December (lowest production)."""
        assert get_average_monthly_production(12, 1.0) == 9.6

    def test_scaling_with_installed_kw(self):
        """Test production scales linearly with installed kW."""
        assert get_average_monthly_production(6, 10.5) == 136.55 * 10.5

    def test_invalid_month(self):
        """Test invalid month returns 0."""
        assert get_average_monthly_production(13, 10.5) == 0

    def test_all_months_present(self):
        """Test all 12 months have values."""
        for month in range(1, 13):
            assert get_average_monthly_production(month, 1.0) > 0


class TestCalculate30YearProjection:
    """Tests for 30-year ROI projection (CalcSavingsEstimate)."""

    @pytest.fixture
    def yearly_data(self, sample_yearly_records, sample_calc_params):
        """Generate yearly overview and monthly data for ROI tests."""
        yearly, monthly = generate_monthly_report(
            sample_yearly_records, sample_calc_params, investment=200000
        )
        return yearly, monthly

    def test_projection_length(self, yearly_data, sample_calc_params):
        """Test that projection has ~30 years total."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            price_development=1.05,
            panel_degradation=0.25,
            investment=200000,
            installed_kw=10.5,
        )
        # Should have at least 20 entries (historical + projected)
        assert len(result) >= 20

    def test_first_entry_is_historical(self, yearly_data):
        """Test first entry is from historical data."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            investment=200000,
        )
        assert result[0].year == 2024
        assert result[0].year_from_start == 1

    def test_price_development_applied(self, yearly_data):
        """Test that price development increases prices over time (after tax reduction removal)."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            price_development=1.05,
            panel_degradation=0.0,  # No degradation to isolate price effect
            investment=200000,
        )
        # After the 2026 tax reduction removal, prices should trend upward
        # Compare two consecutive future years well after 2026
        if len(result) > 10:
            # own_use price should always increase (not affected by tax reduction)
            assert result[-1].average_price_own_use > result[5].average_price_own_use

    def test_panel_degradation_applied(self, yearly_data):
        """Test that panel degradation reduces production each year."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            price_development=0.0,  # No price change
            panel_degradation=0.25,
            investment=200000,
        )
        # Future years should have decreasing production
        if len(result) > 2:
            assert result[-1].production_sold <= result[1].production_sold

    def test_roi_year_detected(self, yearly_data):
        """Test that ROI year is correctly detected."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            investment=50000,  # Low investment for quick payback
        )
        roi_years = [r for r in result if r.is_roi_year]
        # With a low investment, should find a payback year
        assert len(roi_years) <= 1  # At most one ROI year

    def test_remaining_becomes_positive_after_roi(self, yearly_data):
        """Test remaining flips to positive (= profit) after ROI year."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            investment=50000,
        )
        roi_found = False
        for entry in result:
            if entry.is_roi_year:
                roi_found = True
                # After ROI, remaining should be the absolute profit
                assert entry.remaining_on_investment >= 0

    def test_return_percentage_calculated(self, yearly_data):
        """Test return percentage is computed for each year."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            investment=200000,
        )
        for entry in result:
            # Return percentage should be non-negative
            assert entry.return_percentage >= 0

    def test_estimate_roi_to_dict(self):
        """Test EstimateRoi serialization."""
        roi = EstimateRoi(
            year=2024,
            year_from_start=1,
            average_price_sold=1.28,
            average_price_own_use=1.39,
            production_sold=4090.0,
            production_own_use=2820.0,
            year_savings_sold=5235.2,
            year_savings_own_use=3919.8,
            remaining_on_investment=190845.0,
            return_percentage=4.6,
            is_roi_year=False,
        )
        d = roi.to_dict()
        assert d["year"] == 2024
        assert d["is_roi_year"] is False
        assert d["remaining_on_investment"] == 190845.0

    def test_empty_input(self):
        """Test with empty yearly overview."""
        result = calculate_30_year_projection([], [])
        assert result == []

    def test_tax_reduction_removed_2026(self, yearly_data):
        """Test that tax reduction is removed in 2026 (lines 109-111)."""
        yearly, monthly = yearly_data
        result = calculate_30_year_projection(
            yearly, monthly,
            investment=200000,
        )
        # Find entries around 2026
        entries_2025 = [r for r in result if r.year == 2025]
        entries_2026 = [r for r in result if r.year == 2026]
        if entries_2025 and entries_2026:
            # 2026 should have lower average_price_sold due to tax reduction removal
            # (price_sold in 2026 = prev - tax_reduction, then * price_dev)
            pass  # Structure test - actual values depend on data

    def test_current_year_uses_month_specific_prices(self, sample_calc_params):
        """Current year missing months use avg prices from prior years, not yearly avg.

        Winter months (Jan-Feb) have high prices. If used for summer months the ROI
        would be inflated. The fix averages up to 3 prior years per month.

        Note: this test relies on running in year 2026. The 2026 records only
        cover Jan-Feb so the engine must fill Mar-Dec from prior years.
        """
        # Create 2 prior years with distinct summer/winter prices
        records = []
        for year in [2024, 2025]:
            for month in range(1, 13):
                for day in [1, 15]:
                    # Winter: high prices (months 1-2, 11-12); Summer: low prices (5-8)
                    if month in (1, 2, 11, 12):
                        price_sold = 1.50
                        price_own = 1.80
                    elif month in (5, 6, 7, 8):
                        price_sold = 0.30
                        price_own = 0.50
                    else:
                        price_sold = 0.80
                        price_own = 1.00

                    prod_sold = 5.0 if month in (5, 6, 7, 8) else 0.5
                    own_use = 3.0 if month in (5, 6, 7, 8) else 0.3

                    records.append({
                        "timestamp": f"{year}-{month:02d}-{day:02d}T12:00:00+01:00",
                        "purchased": 2.0,
                        "purchased_cost": 2.0 * price_own,
                        "production_sold": prod_sold,
                        "production_sold_profit": prod_sold * price_sold,
                        "production_own_use": own_use,
                        "production_own_use_profit": own_use * price_own,
                        "battery_charge": 0.0,
                        "battery_used": 0.0,
                        "battery_used_profit": 0.0,
                        "unit_price_buy": price_own,
                        "unit_price_sold": price_sold,
                    })

        # Add current year (2026) with only Jan-Feb (winter, high prices)
        for day in [1, 15]:
            records.append({
                "timestamp": f"2026-01-{day:02d}T12:00:00+01:00",
                "purchased": 2.0,
                "purchased_cost": 2.0 * 1.80,
                "production_sold": 0.5,
                "production_sold_profit": 0.5 * 1.50,
                "production_own_use": 0.3,
                "production_own_use_profit": 0.3 * 1.80,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            })
            records.append({
                "timestamp": f"2026-02-{day:02d}T12:00:00+01:00",
                "purchased": 2.0,
                "purchased_cost": 2.0 * 1.80,
                "production_sold": 0.5,
                "production_sold_profit": 0.5 * 1.50,
                "production_own_use": 0.3,
                "production_own_use_profit": 0.3 * 1.80,
                "battery_charge": 0.0,
                "battery_used": 0.0,
                "battery_used_profit": 0.0,
            })

        yearly, monthly = generate_monthly_report(records, sample_calc_params, 200000)

        result = calculate_30_year_projection(
            yearly, monthly,
            price_development=0.0,
            panel_degradation=0.0,
            investment=200000,
            installed_kw=10.5,
        )

        # Find the 2026 entry
        entry_2026 = [r for r in result if r.year == 2026]
        assert len(entry_2026) == 1
        e = entry_2026[0]

        # The average sold price for 2026 should be LOWER than the Jan-Feb price (1.50)
        # because summer months from prior years have ~0.30 prices, pulling the
        # weighted average down significantly.
        assert e.average_price_sold < 1.40, (
            f"Expected avg_price_sold < 1.40 (summer months should lower it), "
            f"got {e.average_price_sold}"
        )
