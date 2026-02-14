import { LitElement, html, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { OverviewData } from "../types";

@customElement("overview-view")
export class OverviewView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _data: OverviewData | null = null;
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
      this._data = await this.hass.callWS({
        type: "my_solar_cells/get_overview",
        entry_id: this.entryId,
      });
    } catch (e: any) {
      this._error = e.message || "Failed to fetch data";
    }
    this._loading = false;
  }

  render(): TemplateResult {
    if (this._loading) {
      return html`<div class="loading">Loading overview...</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">Error: ${this._error}</div>`;
    }
    if (!this._data) {
      return html`<div class="no-data">No data available</div>`;
    }

    const d = this._data;
    return html`
      <div class="card">
        <h3>Database Summary</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">Last Tibber Sync</div>
            <div class="stat-value">${d.last_tibber_sync ? this._formatTimestamp(d.last_tibber_sync) : "Never"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Hourly Records</div>
            <div class="stat-value">${d.hourly_record_count.toLocaleString()}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">First Record</div>
            <div class="stat-value">${d.first_timestamp ? this._formatDate(d.first_timestamp) : "N/A"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Last Record</div>
            <div class="stat-value">${d.last_timestamp ? this._formatDate(d.last_timestamp) : "N/A"}</div>
          </div>
        </div>
      </div>

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
        <h3>Yearly Financial Parameters</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Year</th>
                <th>Tax Reduction</th>
                <th>Grid Comp.</th>
                <th>Transfer Fee</th>
                <th>Energy Tax</th>
                <th>Installed kW</th>
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
      return new Date(ts).toLocaleString("sv-SE");
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
