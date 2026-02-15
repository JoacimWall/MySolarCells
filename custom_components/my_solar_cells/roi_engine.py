"""ROI projection engine - ported from RoiService.cs."""

from __future__ import annotations

import logging
from dataclasses import dataclass
from datetime import datetime

from .const import (
    AVERAGE_PRODUCTION_PER_KW,
    ROI_PROJECTION_YEARS,
    TAX_REDUCTION_END_YEAR,
)
from .financial_engine import HistoryStats, ReportHistoryStats

_LOGGER = logging.getLogger(__name__)


@dataclass
class EstimateRoi:
    """ROI estimate for a single year (from EstimateRoi.cs)."""

    year: int = 0
    year_from_start: int = 0
    average_price_sold: float = 0.0
    average_price_own_use: float = 0.0
    production_sold: float = 0.0
    production_own_use: float = 0.0
    year_savings_sold: float = 0.0
    year_savings_own_use: float = 0.0
    remaining_on_investment: float = 0.0
    return_percentage: float = 0.0
    is_roi_year: bool = False

    def to_dict(self) -> dict:
        """Convert to serializable dict."""
        return {
            "year": self.year,
            "year_from_start": self.year_from_start,
            "average_price_sold": self.average_price_sold,
            "average_price_own_use": self.average_price_own_use,
            "production_sold": self.production_sold,
            "production_own_use": self.production_own_use,
            "year_savings_sold": self.year_savings_sold,
            "year_savings_own_use": self.year_savings_own_use,
            "remaining_on_investment": self.remaining_on_investment,
            "return_percentage": self.return_percentage,
            "is_roi_year": self.is_roi_year,
        }


def get_average_monthly_production(month: int, installed_kw: float) -> float:
    """Get average monthly production for a given month and installed capacity.

    Ports AverageProductionMonth.GetSnitMonth() from RoiService.cs (lines 155-187).
    """
    return AVERAGE_PRODUCTION_PER_KW.get(month, 0) * installed_kw


def _average_monthly_data(
    years_months: list[list[ReportHistoryStats]],
) -> list[ReportHistoryStats]:
    """Average monthly production and price data across multiple years.

    For each calendar month (1-12), averages production and prices across
    all years that have data for that month. Returns up to 12 entries.
    """
    month_groups: dict[int, list[HistoryStats]] = {}
    for year_months in years_months:
        for report in year_months:
            month_num = report.from_date.month
            month_groups.setdefault(month_num, []).append(report.history_stats)

    result: list[ReportHistoryStats] = []
    for month_num in sorted(month_groups.keys()):
        stats_list = month_groups[month_num]
        n = len(stats_list)
        avg_stats = HistoryStats(
            production_sold=sum(s.production_sold for s in stats_list) / n,
            production_own_use=sum(s.production_own_use for s in stats_list) / n,
            battery_used=sum(s.battery_used for s in stats_list) / n,
            facts_production_sold_avg_per_kwh_profit=sum(
                s.facts_production_sold_avg_per_kwh_profit for s in stats_list
            ) / n,
            facts_production_own_use_avg_per_kwh_saved=sum(
                s.facts_production_own_use_avg_per_kwh_saved for s in stats_list
            ) / n,
            facts_battery_used_avg_per_kwh_saved=sum(
                s.facts_battery_used_avg_per_kwh_saved for s in stats_list
            ) / n,
            calc_params=stats_list[0].calc_params,
        )
        result.append(ReportHistoryStats(
            from_date=datetime(2000, month_num, 1),
            history_stats=avg_stats,
        ))

    return result


def calculate_30_year_projection(
    yearly_overview: list[ReportHistoryStats],
    monthly_by_year: list[list[ReportHistoryStats]],
    price_development: float = 1.05,
    panel_degradation: float = 0.25,
    investment: float = 0,
    installed_kw: float = 10.5,
    first_production_day: datetime | None = None,
) -> list[EstimateRoi]:
    """Calculate 30-year ROI projection.

    Ports CalcSavingsEstimate() from RoiService.cs (lines 17-148).

    Args:
        yearly_overview: List of yearly ReportHistoryStats (one per historical year).
        monthly_by_year: List of monthly lists, one list per historical year.
        price_development: Annual electricity price increase percentage (e.g. 1.05 = 5%).
        panel_degradation: Annual panel degradation percentage (e.g. 0.25%).
        investment: Total investment amount.
        installed_kw: Installed panel capacity in kW.
        first_production_day: Date of first production.

    Returns:
        List of EstimateRoi for each year (historical + projected).
    """
    if not yearly_overview:
        return []

    result: list[EstimateRoi] = []
    total_saving = 0.0
    last_known_investment = investment
    year_count_from_start = 1
    now = datetime.now()

    # --- Historical years (lines 24-97) ---
    for idx, year_report in enumerate(yearly_overview):
        stats = year_report.history_stats

        # If current year, fill missing months with prior year data or average production
        # (lines 27-73)
        if year_report.from_date.year == now.year:
            # Get months for current year
            current_year_months: list[ReportHistoryStats] = []
            if idx < len(monthly_by_year):
                current_year_months = monthly_by_year[idx]

            fake_production = 0.0
            fake_own_use = 0.0

            for month_num in range(1, 13):
                # Check if this month is after the current data or before first production
                after_current = month_num > len(current_year_months)
                before_first = (
                    first_production_day is not None
                    and month_num <= len(current_year_months)
                    and len(current_year_months) > month_num - 1
                    and current_year_months[month_num - 1].from_date < first_production_day
                )

                if before_first or after_current:
                    # Try to get data from previous year
                    prev_year_month = None
                    if len(monthly_by_year) > 1:
                        prev_year_months = monthly_by_year[-2] if idx == len(monthly_by_year) - 1 else (
                            monthly_by_year[idx - 1] if idx > 0 else None
                        )
                        if prev_year_months:
                            for m in prev_year_months:
                                if m.from_date.month == month_num:
                                    prev_year_month = m
                                    break

                    if prev_year_month is not None:
                        fake_production += prev_year_month.history_stats.production_sold
                        fake_own_use += prev_year_month.history_stats.production_own_use
                    else:
                        # Use average production table
                        fake_prod = get_average_monthly_production(month_num, installed_kw)
                        fake_production += fake_prod * 0.5
                        fake_own_use += fake_prod * 0.5

            stats.production_sold += fake_production
            stats.production_own_use += fake_own_use

        # Calculate average own use price (line 74-76)
        if stats.facts_battery_used_avg_per_kwh_saved == 0:
            avg_price_own_use = stats.facts_production_own_use_avg_per_kwh_saved
        else:
            avg_price_own_use = round(
                (stats.facts_battery_used_avg_per_kwh_saved
                 + stats.facts_production_own_use_avg_per_kwh_saved) / 2,
                2,
            )

        # Calculate year savings (lines 78-80)
        year_savings_sold = round(
            stats.facts_production_sold_avg_per_kwh_profit * stats.production_sold, 2
        )
        year_savings_own_use = round(
            (stats.production_own_use + stats.battery_used) * avg_price_own_use, 2
        )
        saving_this_year = year_savings_sold + year_savings_own_use
        total_saving += saving_this_year

        inv = stats.investment if stats.investment > 0 else investment

        result.append(
            EstimateRoi(
                year=year_report.from_date.year,
                year_from_start=year_count_from_start,
                average_price_sold=stats.facts_production_sold_avg_per_kwh_profit,
                average_price_own_use=avg_price_own_use,
                production_sold=stats.production_sold,
                production_own_use=stats.production_own_use + stats.battery_used,
                year_savings_sold=year_savings_sold,
                year_savings_own_use=year_savings_own_use,
                remaining_on_investment=round(inv - total_saving, 0),
                return_percentage=round((saving_this_year / inv) * 100, 1) if inv > 0 else 0,
            )
        )
        year_count_from_start += 1
        last_known_investment = inv

    # --- Future projection (lines 99-146) ---
    if not yearly_overview:
        return result

    start_year = yearly_overview[-1].from_date.year + 1
    end_year = yearly_overview[0].from_date.year + ROI_PROJECTION_YEARS
    calc_params_tax_reduction = yearly_overview[-1].history_stats.calc_params.tax_reduction
    last_historical_year = yearly_overview[-1].from_date.year

    roi_year_set = False

    # Average up to 3 most recent years of monthly data for seasonal projection.
    # Each month is averaged across however many years have data for that month.
    last_year_months: list[ReportHistoryStats] = []
    tax_reduction_in_base = last_historical_year < TAX_REDUCTION_END_YEAR
    if monthly_by_year:
        years_to_use = monthly_by_year[-3:]
        last_year_months = _average_monthly_data(years_to_use)
        # If all source months are from years before 2026, their sold prices
        # include tax reduction and we need to subtract it for future years.
        tax_reduction_in_base = all(
            report.from_date.year < TAX_REDUCTION_END_YEAR
            for year_months in years_to_use
            for report in year_months
        )

    for year in range(start_year, end_year):
        years_ahead = year - last_historical_year

        if last_year_months:
            # Monthly-based projection: apply price development and degradation
            # per month to capture seasonal price/production variation
            yearly_savings_sold = 0.0
            yearly_savings_own_use = 0.0
            yearly_prod_sold = 0.0
            yearly_prod_own_use = 0.0

            price_factor = (1 + price_development / 100) ** years_ahead
            degradation_factor = (1 - panel_degradation / 100) ** years_ahead

            for month_report in last_year_months:
                ms = month_report.history_stats

                # Base prices from the last historical year's month
                base_price_sold = ms.facts_production_sold_avg_per_kwh_profit
                if ms.facts_battery_used_avg_per_kwh_saved == 0:
                    base_price_own_use = ms.facts_production_own_use_avg_per_kwh_saved
                else:
                    base_price_own_use = (
                        ms.facts_battery_used_avg_per_kwh_saved
                        + ms.facts_production_own_use_avg_per_kwh_saved
                    ) / 2

                # Remove tax reduction for years >= 2026 if base includes it
                if tax_reduction_in_base and year >= TAX_REDUCTION_END_YEAR:
                    base_price_sold = base_price_sold - calc_params_tax_reduction

                month_price_sold = base_price_sold * price_factor
                month_price_own_use = base_price_own_use * price_factor

                # Production with panel degradation
                month_prod_sold = ms.production_sold * degradation_factor
                month_prod_own_use = (
                    ms.production_own_use + ms.battery_used
                ) * degradation_factor

                yearly_savings_sold += month_prod_sold * month_price_sold
                yearly_savings_own_use += month_prod_own_use * month_price_own_use
                yearly_prod_sold += month_prod_sold
                yearly_prod_own_use += month_prod_own_use

            # Weighted average prices for display
            avg_price_sold = (
                round(yearly_savings_sold / yearly_prod_sold, 2)
                if yearly_prod_sold > 0 else 0.0
            )
            avg_price_own_use = (
                round(yearly_savings_own_use / yearly_prod_own_use, 2)
                if yearly_prod_own_use > 0 else 0.0
            )

            new_entry = EstimateRoi(
                year=year,
                year_from_start=year_count_from_start,
                average_price_sold=avg_price_sold,
                average_price_own_use=avg_price_own_use,
                production_sold=round(yearly_prod_sold, 2),
                production_own_use=round(yearly_prod_own_use, 2),
                year_savings_sold=round(yearly_savings_sold, 2),
                year_savings_own_use=round(yearly_savings_own_use, 2),
            )
        else:
            # Fallback: yearly-average approach when no monthly data available
            if year == TAX_REDUCTION_END_YEAR:
                average_price_sold = round(
                    result[-1].average_price_sold - calc_params_tax_reduction, 2
                )
            else:
                average_price_sold = result[-1].average_price_sold

            new_entry = EstimateRoi(
                year=year,
                year_from_start=year_count_from_start,
                average_price_sold=round(
                    average_price_sold * (1 + price_development / 100), 2
                ),
                average_price_own_use=round(
                    result[-1].average_price_own_use * (1 + price_development / 100), 2
                ),
                production_sold=round(
                    result[-1].production_sold * (1 - panel_degradation / 100), 2
                ),
                production_own_use=round(
                    result[-1].production_own_use * (1 - panel_degradation / 100), 2
                ),
            )
            new_entry.year_savings_sold = round(
                new_entry.production_sold * new_entry.average_price_sold, 2
            )
            new_entry.year_savings_own_use = round(
                new_entry.production_own_use * new_entry.average_price_own_use, 2
            )

        saving_this_year = round(
            new_entry.year_savings_sold + new_entry.year_savings_own_use, 2
        )
        total_saving += saving_this_year

        # Remaining on investment (lines 132-141)
        new_entry.remaining_on_investment = round(last_known_investment - total_saving, 0)
        if new_entry.remaining_on_investment < 0:
            new_entry.remaining_on_investment = abs(new_entry.remaining_on_investment)
            if not roi_year_set:
                new_entry.is_roi_year = True
                roi_year_set = True

        # Return percentage (line 142)
        if last_known_investment > 0:
            new_entry.return_percentage = round(
                (saving_this_year / last_known_investment) * 100, 1
            )

        result.append(new_entry)
        year_count_from_start += 1

    return result
