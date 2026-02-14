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
