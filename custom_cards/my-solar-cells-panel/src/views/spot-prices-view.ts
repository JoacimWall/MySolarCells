import { LitElement, html, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { SpotPrice } from "../types";

@customElement("spot-prices-view")
export class SpotPricesView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _date = "";
  @state() private _prices: SpotPrice[] = [];
  @state() private _loading = false;
  @state() private _error = "";
  @state() private _fetched = false;

  static styles = [cardStyles, tableStyles];

  connectedCallback() {
    super.connectedCallback();
    this._date = new Date().toISOString().substring(0, 10);
  }

  render(): TemplateResult {
    return html`
      <div class="card">
        <h3>Spot Prices</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Date</label>
            <input
              type="date"
              .value=${this._date}
              @change=${(e: Event) => {
                this._date = (e.target as HTMLInputElement).value;
              }}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading ? "Loading..." : "Load"}
          </button>
        </div>

        ${this._error
          ? html`<div class="no-data">Error: ${this._error}</div>`
          : ""}
        ${this._prices.length > 0 ? this._renderTable() : ""}
        ${this._fetched && this._prices.length === 0 && !this._error
          ? html`<div class="no-data">No prices found for ${this._date}</div>`
          : ""}
        ${!this._fetched && !this._error
          ? html`<div class="no-data">Select a date and click Load</div>`
          : ""}
      </div>
    `;
  }

  private _renderTable(): TemplateResult {
    const totals = this._prices.map((p) => p.total);
    const avg = totals.reduce((a, b) => a + b, 0) / totals.length;
    const min = Math.min(...totals);
    const max = Math.max(...totals);

    return html`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>Total SEK/kWh</th>
              <th>Energy</th>
              <th>Tax</th>
              <th>Level</th>
            </tr>
          </thead>
          <tbody>
            ${this._prices.map(
              (p) => html`
                <tr>
                  <td>${this._formatTime(p.timestamp)}</td>
                  <td>${p.total.toFixed(4)}</td>
                  <td>${p.energy.toFixed(4)}</td>
                  <td>${p.tax.toFixed(4)}</td>
                  <td>${p.level || "-"}</td>
                </tr>
              `
            )}
            <tr class="summary-row">
              <td>Summary</td>
              <td>
                Avg: ${avg.toFixed(4)} / Min: ${min.toFixed(4)} / Max:
                ${max.toFixed(4)}
              </td>
              <td></td>
              <td></td>
              <td>${this._prices.length} entries</td>
            </tr>
          </tbody>
        </table>
      </div>
    `;
  }

  private async _fetch() {
    if (!this.hass || !this.entryId || !this._date) return;
    this._loading = true;
    this._error = "";
    this._fetched = false;
    try {
      const result = await this.hass.callWS({
        type: "my_solar_cells/get_spot_prices",
        entry_id: this.entryId,
        date: this._date,
      });
      this._prices = result.prices;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch data";
      this._prices = [];
    }
    this._fetched = true;
    this._loading = false;
  }

  private _formatTime(ts: string): string {
    try {
      // Extract HH:MM from timestamp
      const timePart = ts.includes("T") ? ts.split("T")[1] : ts;
      return timePart.substring(0, 5);
    } catch {
      return ts;
    }
  }
}
