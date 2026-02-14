import { LitElement, html, css, TemplateResult } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import type { EstimateRoi } from "../types";

@customElement("roi-view")
export class RoiView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _projection: EstimateRoi[] = [];
  @state() private _investment = 0;
  @state() private _loading = false;
  @state() private _error = "";
  @state() private _fetched = false;

  static styles = [
    cardStyles,
    tableStyles,
    css`
      .roi-row {
        background: rgba(76, 175, 80, 0.15);
        font-weight: 600;
      }

      td.number {
        text-align: right;
        font-variant-numeric: tabular-nums;
      }

      th.number {
        text-align: right;
      }

      .investment-info {
        font-size: 0.95em;
        color: var(--secondary-text-color);
        margin-bottom: 12px;
      }

      .investment-info strong {
        color: var(--primary-text-color);
      }
    `,
  ];

  updated(changed: Map<string, unknown>) {
    if (
      changed.has("hass") &&
      this.hass &&
      this.entryId &&
      !this._fetched &&
      !this._loading
    ) {
      this._fetchData();
    }
  }

  private async _fetchData() {
    if (!this.hass || !this.entryId) return;
    this._loading = true;
    this._error = "";
    try {
      const result = await this.hass.callWS({
        type: "my_solar_cells/get_roi_projection",
        entry_id: this.entryId,
      });
      this._projection = result.projection;
      this._investment = result.investment;
      this._fetched = true;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch ROI projection";
    }
    this._loading = false;
  }

  private _fmtInt(v: number): string {
    return Math.round(v).toLocaleString("sv-SE");
  }

  private _fmtSek(v: number): string {
    return v.toLocaleString("sv-SE", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
  }

  private _fmtPct(v: number): string {
    return v.toLocaleString("sv-SE", {
      minimumFractionDigits: 1,
      maximumFractionDigits: 1,
    });
  }

  render(): TemplateResult {
    if (this._loading) {
      return html`<div class="loading">Loading ROI projection...</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">Error: ${this._error}</div>`;
    }
    if (!this._projection.length) {
      return html`<div class="no-data">No ROI projection data available.</div>`;
    }

    return html`
      <div class="card">
        <h3>ROI Projection</h3>
        <div class="investment-info">
          Investment: <strong>${this._fmtSek(this._investment)} SEK</strong>
        </div>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th class="number">#</th>
                <th class="number">Year</th>
                <th class="number">Avg price sold</th>
                <th class="number">Avg price own use</th>
                <th class="number">Prod. sold (kWh)</th>
                <th class="number">Prod. own use (kWh)</th>
                <th class="number">Savings sold (SEK)</th>
                <th class="number">Savings own use (SEK)</th>
                <th class="number">Return %</th>
                <th class="number">Remaining (SEK)</th>
              </tr>
            </thead>
            <tbody>
              ${this._projection.map((r) => this._renderRow(r))}
            </tbody>
          </table>
        </div>
      </div>
    `;
  }

  private _renderRow(r: EstimateRoi): TemplateResult {
    return html`
      <tr class=${r.is_roi_year ? "roi-row" : ""}>
        <td class="number">${r.year_from_start}</td>
        <td class="number">${r.year}</td>
        <td class="number">${this._fmtSek(r.average_price_sold)}</td>
        <td class="number">${this._fmtSek(r.average_price_own_use)}</td>
        <td class="number">${this._fmtInt(r.production_sold)}</td>
        <td class="number">${this._fmtInt(r.production_own_use)}</td>
        <td class="number">${this._fmtSek(r.year_savings_sold)}</td>
        <td class="number">${this._fmtSek(r.year_savings_own_use)}</td>
        <td class="number">${this._fmtPct(r.return_percentage)}</td>
        <td class="number">${this._fmtSek(r.remaining_on_investment)}</td>
      </tr>
    `;
  }
}
