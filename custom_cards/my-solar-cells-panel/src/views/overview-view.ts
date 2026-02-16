import { LitElement, html, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { OverviewData, PeriodSummariesResponse } from "../types";
import { t, getLocale } from "../localize";
import "../components/period-summary-chart";

@customElement("overview-view")
export class OverviewView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _data: OverviewData | null = null;
  @state() private _periodData: PeriodSummariesResponse | null = null;
  @state() private _loading = false;
  @state() private _error = "";

  static styles = [cardStyles, tableStyles];

  updated(changed: Map<string, unknown>) {
    if (changed.has("hass") && this.hass && this.entryId && !this._data && !this._loading) {
      this._fetchData();
    }
  }

  private async _fetchData() {
    if (!this.hass || !this.entryId) return;
    this._loading = true;
    this._error = "";
    try {
      const [overview, periods] = await Promise.all([
        this.hass.callWS({
          type: "my_solar_cells/get_overview",
          entry_id: this.entryId,
        }),
        this.hass.callWS({
          type: "my_solar_cells/get_period_summaries",
          entry_id: this.entryId,
        }),
      ]);
      this._data = overview;
      this._periodData = periods;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch data";
    }
    this._loading = false;
  }

  render(): TemplateResult {
    if (this._loading) {
      return html`<div class="loading">${t(this.hass, "overview.loadingOverview")}</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">${t(this.hass, "common.error")}: ${this._error}</div>`;
    }
    if (!this._data) {
      return html`<div class="no-data">${t(this.hass, "overview.noData")}</div>`;
    }

    const d = this._data;
    return html`
      <div class="card">
        <h3>${t(this.hass, "overview.dbSummary")}</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">${t(this.hass, "overview.lastTibberSync")}</div>
            <div class="stat-value">${d.last_tibber_sync ? this._formatTimestamp(d.last_tibber_sync) : t(this.hass, "common.never")}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${t(this.hass, "overview.hourlyRecords")}</div>
            <div class="stat-value">${d.hourly_record_count.toLocaleString(getLocale(this.hass))}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${t(this.hass, "overview.firstRecord")}</div>
            <div class="stat-value">${d.first_timestamp ? this._formatDate(d.first_timestamp) : t(this.hass, "common.na")}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">${t(this.hass, "overview.lastRecord")}</div>
            <div class="stat-value">${d.last_timestamp ? this._formatDate(d.last_timestamp) : t(this.hass, "common.na")}</div>
          </div>
        </div>
      </div>

      ${this._periodData
        ? html`
            <div class="card">
              <h3>${t(this.hass, "overview.energySummary")}</h3>
              <period-summary-chart .hass=${this.hass} .data=${this._periodData}></period-summary-chart>
            </div>
          `
        : nothing}

      ${this._renderYearlyParams(d.yearly_params)}
    `;
  }

  private _renderYearlyParams(
    params: Record<string, any>
  ): TemplateResult | typeof nothing {
    const years = Object.keys(params).sort();
    if (years.length === 0) return nothing;

    return html`
      <div class="card">
        <h3>${t(this.hass, "overview.yearlyParams")}</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>${t(this.hass, "common.year")}</th>
                <th>${t(this.hass, "overview.taxReduction")}</th>
                <th>${t(this.hass, "overview.gridComp")}</th>
                <th>${t(this.hass, "overview.transferFee")}</th>
                <th>${t(this.hass, "overview.energyTax")}</th>
                <th>${t(this.hass, "overview.installedKw")}</th>
              </tr>
            </thead>
            <tbody>
              ${years.map((year) => {
                const p = params[year];
                return html`
                  <tr>
                    <td>${year}</td>
                    <td>${this._fmt(p.tax_reduction)}</td>
                    <td>${this._fmt(p.grid_compensation)}</td>
                    <td>${this._fmt(p.transfer_fee)}</td>
                    <td>${this._fmt(p.energy_tax)}</td>
                    <td>${p.installed_kw != null ? p.installed_kw : "-"}</td>
                  </tr>
                `;
              })}
            </tbody>
          </table>
        </div>
      </div>
    `;
  }

  private _fmt(val: number | undefined): string {
    return val != null ? val.toFixed(3) : "-";
  }

  private _formatTimestamp(ts: string): string {
    try {
      return new Date(ts).toLocaleString(getLocale(this.hass));
    } catch {
      return ts;
    }
  }

  private _formatDate(ts: string): string {
    try {
      return ts.substring(0, 10);
    } catch {
      return ts;
    }
  }
}
