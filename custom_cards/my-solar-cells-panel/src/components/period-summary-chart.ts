import { LitElement, html, css, TemplateResult, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import { PeriodSummariesResponse, PeriodSummary } from "../types";
import { t } from "../localize";

interface BarDef {
  key: keyof PeriodSummary;
  labelKey: "chart.ownUse" | "chart.sold";
  color: string;
  unit: string;
}

const BARS: BarDef[] = [
  { key: "own_use_kwh", labelKey: "chart.ownUse", color: "#4285f4", unit: "kWh" },
  { key: "sold_kwh", labelKey: "chart.sold", color: "#34a853", unit: "kWh" },
  { key: "own_use_sek", labelKey: "chart.ownUse", color: "#8ab4f8", unit: "SEK" },
  { key: "sold_sek", labelKey: "chart.sold", color: "#f4a742", unit: "SEK" },
];

const PERIOD_KEYS: { key: keyof PeriodSummariesResponse; labelKey: "chart.today" | "chart.thisWeek" | "chart.thisMonth" | "chart.thisYear" }[] = [
  { key: "today", labelKey: "chart.today" },
  { key: "this_week", labelKey: "chart.thisWeek" },
  { key: "this_month", labelKey: "chart.thisMonth" },
  { key: "this_year", labelKey: "chart.thisYear" },
];

@customElement("period-summary-chart")
export class PeriodSummaryChart extends LitElement {
  @property({ attribute: false }) hass: any;
  @property({ attribute: false }) data?: PeriodSummariesResponse;

  static styles = css`
    :host {
      display: block;
    }

    .chart-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    @media (max-width: 600px) {
      .chart-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .period-column {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .period-label {
      font-size: 0.85em;
      font-weight: 500;
      color: var(--primary-text-color);
      margin-bottom: 8px;
    }

    .bars {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 120px;
      width: 100%;
      justify-content: center;
    }

    .bar-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      max-width: 36px;
      height: 100%;
      justify-content: flex-end;
    }

    .bar {
      width: 100%;
      border-radius: 3px 3px 0 0;
      min-height: 2px;
      transition: height 0.3s ease;
    }

    .bar-value {
      font-size: 0.65em;
      color: var(--secondary-text-color);
      margin-top: 4px;
      white-space: nowrap;
      text-align: center;
    }

    .legend {
      display: flex;
      justify-content: center;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75em;
      color: var(--secondary-text-color);
    }

    .legend-swatch {
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }
  `;

  render(): TemplateResult | typeof nothing {
    if (!this.data) return nothing;

    const maxKwh = this._getMax("kwh");
    const maxSek = this._getMax("sek");

    return html`
      <div class="chart-container">
        ${PERIOD_KEYS.map((p) => this._renderPeriod(p, this.data![p.key], maxKwh, maxSek))}
      </div>
      <div class="legend">
        ${BARS.map(
          (b) => html`
            <div class="legend-item">
              <div class="legend-swatch" style="background:${b.color}"></div>
              ${t(this.hass, b.labelKey)} (${b.unit})
            </div>
          `
        )}
      </div>
    `;
  }

  private _renderPeriod(
    period: { key: keyof PeriodSummariesResponse; labelKey: "chart.today" | "chart.thisWeek" | "chart.thisMonth" | "chart.thisYear" },
    summary: PeriodSummary,
    maxKwh: number,
    maxSek: number
  ): TemplateResult {
    return html`
      <div class="period-column">
        <div class="period-label">${t(this.hass, period.labelKey)}</div>
        <div class="bars">
          ${BARS.map((bar) => {
            const value = summary[bar.key];
            const max = bar.unit === "kWh" ? maxKwh : maxSek;
            const pct = max > 0 ? (value / max) * 100 : 0;
            return html`
              <div class="bar-wrapper">
                <div
                  class="bar"
                  style="height:${pct}%;background:${bar.color}"
                ></div>
                <div class="bar-value">${this._formatValue(value, bar.unit)}</div>
              </div>
            `;
          })}
        </div>
      </div>
    `;
  }

  private _getMax(unitType: "kwh" | "sek"): number {
    if (!this.data) return 0;
    const keys: (keyof PeriodSummary)[] =
      unitType === "kwh"
        ? ["own_use_kwh", "sold_kwh"]
        : ["own_use_sek", "sold_sek"];
    let max = 0;
    for (const period of PERIOD_KEYS) {
      const summary = this.data[period.key];
      for (const key of keys) {
        if (summary[key] > max) max = summary[key];
      }
    }
    return max;
  }

  private _formatValue(value: number, unit: string): string {
    if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}k`;
    }
    if (value >= 10) {
      return value.toFixed(0);
    }
    return value.toFixed(1);
  }
}
