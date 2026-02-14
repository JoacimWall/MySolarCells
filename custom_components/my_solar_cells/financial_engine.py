"""Financial calculation engine - ported from HistoryDataService.cs."""

from __future__ import annotations

import logging
from copy import copy
from dataclasses import dataclass, field
from datetime import datetime, timezone

_LOGGER = logging.getLogger(__name__)


@dataclass
class CalcParams:
    """Energy calculation parameters (from EnergyCalculationParameter.cs)."""

    tax_reduction: float = 0.60
    grid_compensation: float = 0.078
    transfer_fee: float = 0.30
    energy_tax: float = 0.49
    installed_kw: float = 10.5


@dataclass
class HistoryStats:
    """Financial statistics for a period (from HistoryStats.cs)."""

    title: str = ""

    # Purchased
    purchased: float = 0.0
    purchased_cost: float = 0.0
    purchased_transfer_fee_cost: float = 0.0
    purchased_tax_cost: float = 0.0

    # Production Sold
    production_sold: float = 0.0
    production_sold_profit: float = 0.0
    production_sold_grid_compensation_profit: float = 0.0
    production_sold_tax_reduction_profit: float = 0.0
    production_sold_tax_reduction_comment: str = ""

    # Production Own Use
    production_own_use: float = 0.0
    production_own_use_saved: float = 0.0
    production_own_use_transfer_fee_saved: float = 0.0
    production_own_use_energy_tax_saved: float = 0.0

    # Battery
    battery_used: float = 0.0
    battery_used_saved: float = 0.0
    battery_use_transfer_fee_saved: float = 0.0
    battery_use_energy_tax_saved: float = 0.0
    battery_charge: float = 0.0

    # Investment / Interest
    interest_cost: float = 0.0
    investment: float = 0.0

    # Averages (computed in summarize)
    facts_production_index: float = 0.0
    facts_purchased_cost_avg_per_kwh: float = 0.0
    facts_production_sold_avg_per_kwh_profit: float = 0.0
    facts_production_own_use_avg_per_kwh_saved: float = 0.0
    facts_battery_used_avg_per_kwh_saved: float = 0.0

    # Calc params reference
    calc_params: CalcParams = field(default_factory=CalcParams)

    @property
    def sum_purchased_cost(self) -> float:
        return round(self.purchased_cost + self.purchased_transfer_fee_cost + self.purchased_tax_cost, 2)

    @property
    def sum_production_sold_profit(self) -> float:
        return round(
            self.production_sold_profit
            + self.production_sold_grid_compensation_profit
            + self.production_sold_tax_reduction_profit,
            2,
        )

    @property
    def sum_production_own_use_saved(self) -> float:
        return round(
            self.production_own_use_saved
            + self.production_own_use_transfer_fee_saved
            + self.production_own_use_energy_tax_saved,
            2,
        )

    @property
    def sum_battery_use_saved(self) -> float:
        return round(
            self.battery_used_saved
            + self.battery_use_transfer_fee_saved
            + self.battery_use_energy_tax_saved,
            2,
        )

    @property
    def sum_all_production_sold_and_saved(self) -> float:
        return round(
            self.sum_production_sold_profit
            + self.sum_production_own_use_saved
            + self.sum_battery_use_saved,
            2,
        )

    @property
    def sum_all_production(self) -> float:
        return round(self.production_sold + self.production_own_use + self.battery_charge, 2)

    @property
    def balance(self) -> float:
        """Balance: production profit - consumption cost."""
        return round(self.sum_all_production_sold_and_saved - self.sum_purchased_cost, 2)


@dataclass
class ReportHistoryStats:
    """Report wrapper for history stats with date context."""

    from_date: datetime = field(default_factory=datetime.now)
    history_stats: HistoryStats = field(default_factory=HistoryStats)
    first_production_day: datetime | None = None


def get_calc_params_for_year(
    year: str,
    base_params: CalcParams,
    yearly_params: dict[str, dict[str, float]] | None,
) -> CalcParams:
    """Return CalcParams with year-specific overrides merged onto base_params.

    If yearly_params is None or has no entry for the given year,
    returns base_params unchanged.
    """
    if not yearly_params or year not in yearly_params:
        return base_params

    overrides = yearly_params[year]
    params = copy(base_params)
    for key in (
        "tax_reduction", "grid_compensation", "transfer_fee", "energy_tax",
        "installed_kw",
    ):
        if key in overrides:
            setattr(params, key, overrides[key])
    return params


def calculate_daily_stats(
    hourly_records: list[dict],
    calc_params: CalcParams,
) -> HistoryStats:
    """Calculate financial stats for a single day's hourly records.

    Ports CalculateTotalsInternal() from HistoryDataService.cs (lines 201-325).
    """
    stats = HistoryStats(calc_params=calc_params)

    if not hourly_records:
        return stats

    # --- Purchased (lines 270-273) ---
    stats.purchased = round(sum(r.get("purchased", 0) for r in hourly_records), 2)
    stats.purchased_cost = round(sum(r.get("purchased_cost", 0) for r in hourly_records), 2)
    stats.purchased_transfer_fee_cost = round(stats.purchased * calc_params.transfer_fee, 2)
    stats.purchased_tax_cost = round(stats.purchased * calc_params.energy_tax, 2)

    # --- Production Sold (lines 275-281) ---
    stats.production_sold = round(sum(r.get("production_sold", 0) for r in hourly_records), 2)
    stats.production_sold_profit = round(
        sum(r.get("production_sold_profit", 0) for r in hourly_records), 2
    )
    stats.production_sold_grid_compensation_profit = round(
        stats.production_sold * calc_params.grid_compensation, 2
    )
    stats.production_sold_tax_reduction_profit = round(
        stats.production_sold * calc_params.tax_reduction, 2
    )

    # --- Production Own Use (lines 284-287) ---
    stats.production_own_use = round(sum(r.get("production_own_use", 0) for r in hourly_records), 2)
    stats.production_own_use_saved = round(
        sum(r.get("production_own_use_profit", 0) for r in hourly_records), 2
    )
    stats.production_own_use_transfer_fee_saved = round(
        stats.production_own_use * calc_params.transfer_fee, 2
    )
    stats.production_own_use_energy_tax_saved = round(
        stats.production_own_use * calc_params.energy_tax, 2
    )

    # --- Battery Used (lines 290-296) ---
    stats.battery_used = round(sum(r.get("battery_used", 0) for r in hourly_records), 2)
    stats.battery_used_saved = round(
        sum(r.get("battery_used_profit", 0) for r in hourly_records), 2
    )
    stats.battery_use_transfer_fee_saved = round(stats.battery_used * calc_params.transfer_fee, 2)
    stats.battery_use_energy_tax_saved = round(stats.battery_used * calc_params.energy_tax, 2)

    # --- Battery Charge ---
    stats.battery_charge = round(sum(r.get("battery_charge", 0) for r in hourly_records), 2)

    return stats


def summarize_stats(stats_list: list[HistoryStats], total_days: float) -> HistoryStats:
    """Summarize multiple HistoryStats into one aggregate.

    Ports SummarizeToOneRoiStats() from HistoryDataService.cs (lines 124-199).
    """
    if not stats_list:
        return HistoryStats()

    result = HistoryStats(
        calc_params=stats_list[0].calc_params,
        purchased=round(sum(s.purchased for s in stats_list), 2),
        purchased_cost=round(sum(s.purchased_cost for s in stats_list), 2),
        purchased_transfer_fee_cost=round(sum(s.purchased_transfer_fee_cost for s in stats_list), 2),
        purchased_tax_cost=round(sum(s.purchased_tax_cost for s in stats_list), 2),
        production_sold=round(sum(s.production_sold for s in stats_list), 2),
        production_sold_profit=round(sum(s.production_sold_profit for s in stats_list), 2),
        production_sold_grid_compensation_profit=round(
            sum(s.production_sold_grid_compensation_profit for s in stats_list), 2
        ),
        production_sold_tax_reduction_profit=round(
            sum(s.production_sold_tax_reduction_profit for s in stats_list), 2
        ),
        production_own_use=round(sum(s.production_own_use for s in stats_list), 2),
        production_own_use_saved=round(sum(s.production_own_use_saved for s in stats_list), 2),
        production_own_use_transfer_fee_saved=round(
            sum(s.production_own_use_transfer_fee_saved for s in stats_list), 2
        ),
        production_own_use_energy_tax_saved=round(
            sum(s.production_own_use_energy_tax_saved for s in stats_list), 2
        ),
        battery_used=round(sum(s.battery_used for s in stats_list), 2),
        battery_used_saved=round(sum(s.battery_used_saved for s in stats_list), 2),
        battery_use_transfer_fee_saved=round(
            sum(s.battery_use_transfer_fee_saved for s in stats_list), 2
        ),
        battery_use_energy_tax_saved=round(sum(s.battery_use_energy_tax_saved for s in stats_list), 2),
        battery_charge=round(sum(s.battery_charge for s in stats_list), 2),
        interest_cost=round(sum(s.interest_cost for s in stats_list), 2),
        investment=stats_list[-1].investment,
    )

    # Averages (per kWh prices)
    if total_days > 0:
        result.facts_production_index = round(result.sum_all_production / total_days, 2)

    if result.purchased > 0:
        result.facts_purchased_cost_avg_per_kwh = round(
            result.sum_purchased_cost / result.purchased, 2
        )

    if result.production_sold > 0:
        result.facts_production_sold_avg_per_kwh_profit = round(
            result.sum_production_sold_profit / result.production_sold, 2
        )

    if result.production_own_use > 0:
        result.facts_production_own_use_avg_per_kwh_saved = round(
            result.sum_production_own_use_saved / result.production_own_use, 2
        )

    if result.battery_used > 0:
        result.facts_battery_used_avg_per_kwh_saved = round(
            result.sum_battery_use_saved / result.battery_used, 2
        )

    return result


def calculate_period(
    hourly_records: list[dict],
    calc_params: CalcParams,
    yearly_params: dict[str, dict[str, float]] | None = None,
) -> HistoryStats:
    """Calculate stats for a period by computing daily stats and summarizing.

    Groups records by date, computes daily stats, then summarizes.
    If yearly_params is provided, per-year overrides are applied to calc_params.
    """
    if not hourly_records:
        return HistoryStats(calc_params=calc_params)

    # Group records by date
    daily_groups: dict[str, list[dict]] = {}
    for record in hourly_records:
        ts = record.get("timestamp", "")
        if isinstance(ts, str):
            try:
                dt = datetime.fromisoformat(ts)
            except (ValueError, TypeError):
                continue
        else:
            dt = ts
        date_key = dt.strftime("%Y-%m-%d")
        daily_groups.setdefault(date_key, []).append(record)

    # Calculate daily stats
    daily_stats = []
    for date_key, records in sorted(daily_groups.items()):
        year = date_key[:4]
        params = get_calc_params_for_year(year, calc_params, yearly_params)
        daily_stats.append(calculate_daily_stats(records, params))

    total_days = len(daily_stats) if daily_stats else 1
    return summarize_stats(daily_stats, total_days)


def generate_monthly_report(
    hourly_records: list[dict],
    calc_params: CalcParams,
    investment: float = 0,
    yearly_params: dict[str, dict[str, float]] | None = None,
) -> tuple[list[ReportHistoryStats], list[list[ReportHistoryStats]]]:
    """Generate monthly and yearly aggregated reports.

    Ports GenerateTotalPerMonthReport() from HistoryDataService.cs (lines 25-101).

    Returns:
        Tuple of (yearly_overview, monthly_by_year).
    """
    if not hourly_records:
        return [], []

    # Group records by month
    monthly_groups: dict[str, list[dict]] = {}
    for record in hourly_records:
        ts = record.get("timestamp", "")
        if isinstance(ts, str):
            try:
                dt = datetime.fromisoformat(ts)
            except (ValueError, TypeError):
                continue
        else:
            dt = ts
        month_key = dt.strftime("%Y-%m")
        monthly_groups.setdefault(month_key, []).append(record)

    # Calculate per-month stats
    monthly_reports: list[ReportHistoryStats] = []
    for month_key in sorted(monthly_groups.keys()):
        records = monthly_groups[month_key]
        stats = calculate_period(records, calc_params, yearly_params)
        stats.investment = investment
        year, month = month_key.split("-")
        from_date = datetime(int(year), int(month), 1, tzinfo=timezone.utc)
        stats.title = from_date.strftime("%B")
        monthly_reports.append(
            ReportHistoryStats(
                from_date=from_date,
                history_stats=stats,
            )
        )

    # Group months by year
    yearly_months: dict[int, list[ReportHistoryStats]] = {}
    for report in monthly_reports:
        year = report.from_date.year
        yearly_months.setdefault(year, []).append(report)

    # Create yearly summaries
    yearly_overview: list[ReportHistoryStats] = []
    monthly_by_year: list[list[ReportHistoryStats]] = []

    for year in sorted(yearly_months.keys()):
        months = yearly_months[year]
        total_days = sum(
            (m.from_date.replace(month=m.from_date.month % 12 + 1, day=1) - m.from_date).days
            if m.from_date.month < 12
            else (m.from_date.replace(year=m.from_date.year + 1, month=1, day=1) - m.from_date).days
            for m in months
        )
        year_stats = summarize_stats(
            [m.history_stats for m in months], total_days
        )
        year_stats.investment = investment
        year_stats.title = str(year)

        # Tax reduction cap (lines 87-97): if production_sold > purchased,
        # cap tax reduction to purchased * tax_reduction
        if year_stats.production_sold > year_stats.purchased:
            year_params = get_calc_params_for_year(str(year), calc_params, yearly_params)
            old_value = year_stats.production_sold_tax_reduction_profit
            year_stats.production_sold_tax_reduction_profit = round(
                year_stats.purchased * year_params.tax_reduction, 2
            )
            missed = round(old_value - year_stats.production_sold_tax_reduction_profit, 2)
            year_stats.production_sold_tax_reduction_comment = (
                f"Tax reduction capped: production ({year_stats.production_sold} kWh) "
                f"> purchased ({year_stats.purchased} kWh). Missed: {missed} SEK"
            )

        yearly_overview.append(
            ReportHistoryStats(
                from_date=months[0].from_date,
                history_stats=year_stats,
            )
        )
        monthly_by_year.append(months)

    return yearly_overview, monthly_by_year
