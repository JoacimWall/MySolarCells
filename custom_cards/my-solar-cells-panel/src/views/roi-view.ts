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
  @state() private _initialLoaded = false;

  // Only used for initial pre-fill, not updated on every keystroke
  private _defaultPriceDev = 5.0;
  private _defaultPanelDeg = 0.25;

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

      .input-group input[type="number"] {
        width: 80px;
      }

      .info-box {
        background: var(--primary-background-color);
        border-left: 3px solid var(--primary-color);
        border-radius: 4px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.85em;
        line-height: 1.5;
        color: var(--secondary-text-color);
      }

      .info-box summary {
        cursor: pointer;
        color: var(--primary-text-color);
        font-weight: 500;
      }

      .info-box ul {
        margin: 8px 0 0 0;
        padding-left: 20px;
      }

      .info-box li {
        margin-bottom: 4px;
      }
    `,
  ];

  updated(changed: Map<string, unknown>) {
    if (
      changed.has("hass") &&
      this.hass &&
      this.entryId &&
      !this._initialLoaded &&
      !this._loading
    ) {
      this._fetchInitial();
    }
  }

  private async _fetchInitial() {
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
      // Values from backend are raw percentages used in formula:
      // price * (1 + value / 100), so 5 means 5%
      this._defaultPriceDev = result.price_development;
      this._defaultPanelDeg = result.panel_degradation;
      this._initialLoaded = true;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch ROI projection";
    }
    this._loading = false;
  }

  private async _onCalculate() {
    if (!this.hass || !this.entryId) return;

    // Read values directly from DOM inputs — these are raw percentages
    const priceInput = this.shadowRoot!.getElementById("price-dev-input") as HTMLInputElement;
    const panelInput = this.shadowRoot!.getElementById("panel-deg-input") as HTMLInputElement;
    const priceDev = parseFloat(priceInput.value) || 0;
    const panelDeg = parseFloat(panelInput.value) || 0;

    this._loading = true;
    this._error = "";
    try {
      const result = await this.hass.callWS({
        type: "my_solar_cells/get_roi_projection",
        entry_id: this.entryId,
        price_development: priceDev,
        panel_degradation: panelDeg,
      });
      this._projection = result.projection;
      this._investment = result.investment;
    } catch (e: any) {
      this._error = e.message || "Failed to recalculate ROI projection";
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
    if (this._loading && !this._initialLoaded) {
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
        <details class="info-box">
          <summary>How is the ROI calculated?</summary>
          <ul>
            <li><strong>Historical years</strong> use actual production and price data from Tibber.
              If the current year is incomplete, missing months are filled with data from the
              previous year or average production estimates.</li>
            <li><strong>Future years</strong> are projected from the last historical year:</li>
            <ul>
              <li>Prices increase each year by the <em>price development</em> percentage.
                E.g. 5% means next year's price = this year's price &times; 1.05.</li>
              <li>Production decreases each year by the <em>panel degradation</em> percentage.
                E.g. 0.25% means next year's production = this year's &times; 0.9975.</li>
            </ul>
            <li><strong>Savings sold</strong> = production sold &times; average sold price</li>
            <li><strong>Savings own use</strong> = own use production &times; average own use price</li>
            <li><strong>Remaining</strong> = investment &minus; cumulative total savings</li>
            <li>The <strong>ROI year</strong> (green row) is when cumulative savings exceed the investment.</li>
            <li>Tax reduction (skattereduktion) is removed from sold price starting 2026.</li>
          </ul>
        </details>
        <div class="table-controls">
          <div class="input-group">
            <label>Price development (%/year)</label>
            <input
              id="price-dev-input"
              type="number"
              step="0.5"
              value=${this._defaultPriceDev}
            />
          </div>
          <div class="input-group">
            <label>Panel degradation (%/year)</label>
            <input
              id="panel-deg-input"
              type="number"
              step="0.05"
              value=${this._defaultPanelDeg}
            />
          </div>
          <button class="btn" @click=${this._onCalculate} ?disabled=${this._loading}>
            ${this._loading ? "Calculating..." : "Calculate"}
          </button>
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
