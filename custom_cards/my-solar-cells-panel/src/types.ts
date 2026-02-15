export interface HourlyRecord {
  timestamp: string;
  purchased: number;
  purchased_cost: number;
  production_sold: number;
  production_sold_profit: number;
  unit_price_buy: number;
  unit_price_vat_buy: number;
  unit_price_sold: number;
  unit_price_vat_sold: number;
  production_own_use: number;
  production_own_use_profit: number;
  battery_charge: number;
  battery_used: number;
  battery_used_profit: number;
  synced: number;
  sensor_enriched: number;
  price_level: string;
}

export interface OverviewData {
  last_tibber_sync: string | null;
  hourly_record_count: number;
  first_timestamp: string | null;
  last_timestamp: string | null;
  yearly_params: Record<string, YearlyParams>;
}

export interface YearlyParams {
  tax_reduction?: number;
  grid_compensation?: number;
  transfer_fee?: number;
  energy_tax?: number;
  installed_kw?: number;
}

export interface HourlyEnergyResponse {
  records: HourlyRecord[];
  total_count: number;
}

export interface YearlyParamsResponse {
  yearly_params: Record<string, YearlyParams>;
  first_timestamp: string | null;
  last_timestamp: string | null;
}

export interface PeriodSummary {
  own_use_kwh: number;
  own_use_sek: number;
  sold_kwh: number;
  sold_sek: number;
}

export interface PeriodSummariesResponse {
  today: PeriodSummary;
  this_week: PeriodSummary;
  this_month: PeriodSummary;
  this_year: PeriodSummary;
}

export interface EstimateRoi {
  year: number;
  year_from_start: number;
  average_price_sold: number;
  average_price_own_use: number;
  production_sold: number;
  production_own_use: number;
  year_savings_sold: number;
  year_savings_own_use: number;
  remaining_on_investment: number;
  return_percentage: number;
  is_roi_year: boolean;
}

export interface RoiProjectionResponse {
  projection: EstimateRoi[];
  investment: number;
  price_development: number;
  panel_degradation: number;
}

export interface HistoryStatsResponse {
  title: string;
  // Purchased
  purchased: number;
  purchased_cost: number;
  purchased_transfer_fee_cost: number;
  purchased_tax_cost: number;
  // Production Sold
  production_sold: number;
  production_sold_profit: number;
  production_sold_grid_compensation_profit: number;
  production_sold_tax_reduction_profit: number;
  production_sold_tax_reduction_comment: string;
  // Production Own Use
  production_own_use: number;
  production_own_use_saved: number;
  production_own_use_transfer_fee_saved: number;
  production_own_use_energy_tax_saved: number;
  // Battery
  battery_used: number;
  battery_used_saved: number;
  battery_use_transfer_fee_saved: number;
  battery_use_energy_tax_saved: number;
  battery_charge: number;
  // Investment / Interest
  interest_cost: number;
  investment: number;
  // Peak power
  peak_purchased: number;
  peak_purchased_and_own_usage: number;
  peak_energy_reduction: number;
  peak_purchased_cost: number;
  peak_energy_reduction_saved: number;
  // Averages
  facts_production_index: number;
  facts_purchased_cost_avg_per_kwh: number;
  facts_production_sold_avg_per_kwh_profit: number;
  facts_production_own_use_avg_per_kwh_saved: number;
  facts_battery_used_avg_per_kwh_saved: number;
  // Computed sums
  sum_purchased_cost: number;
  sum_production_sold_profit: number;
  sum_production_own_use_saved: number;
  sum_battery_use_saved: number;
  sum_all_production_sold_and_saved: number;
  sum_all_production: number;
  sum_all_consumption: number;
  balance: number;
  // Calc params
  calc_params: {
    tax_reduction: number;
    grid_compensation: number;
    transfer_fee: number;
    energy_tax: number;
  };
}
