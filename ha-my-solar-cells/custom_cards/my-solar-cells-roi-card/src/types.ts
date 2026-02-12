export interface RoiProjectionEntry {
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

export interface MySolarCellsRoiCardConfig {
  type: string;
  entity: string;
  show_chart?: boolean;
  show_table?: boolean;
  title?: string;
}

export interface HassEntity {
  state: string;
  attributes: {
    projection?: RoiProjectionEntry[];
    investment_amount?: number;
    total_savings_to_date?: number;
    [key: string]: unknown;
  };
}
