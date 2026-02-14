import { LitElement, html, TemplateResult } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { HourlyRecord } from "../types";

const PAGE_SIZE = 50;

@customElement("hourly-energy-view")
export class HourlyEnergyView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _startDate = "";
  @state() private _endDate = "";
  @state() private _records: HourlyRecord[] = [];
  @state() private _totalCount = 0;
  @state() private _offset = 0;
  @state() private _loading = false;
  @state() private _error = "";

  static styles = [cardStyles, tableStyles];

  connectedCallback() {
    super.connectedCallback();
    // Default to today
    const today = new Date().toISOString().substring(0, 10);
    this._startDate = today;
    this._endDate = today;
  }

  render(): TemplateResult {
    return html`
      <div class="card">
        <h3>Hourly Energy Records</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Start Date</label>
            <input
              type="date"
              .value=${this._startDate}
              @change=${(e: Event) => {
                this._startDate = (e.target as HTMLInputElement).value;
              }}
            />
          </div>
          <div class="input-group">
            <label>End Date</label>
            <input
              type="date"
              .value=${this._endDate}
              @change=${(e: Event) => {
                this._endDate = (e.target as HTMLInputElement).value;
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
        ${this._records.length > 0 ? this._renderTable() : ""}
        ${!this._loading && this._records.length === 0 && !this._error
          ? html`<div class="no-data">
              Select a date range and click Load
            </div>`
          : ""}
      </div>
    `;
  }

  private _renderTable(): TemplateResult {
    const totalPages = Math.ceil(this._totalCount / PAGE_SIZE);
    const currentPage = Math.floor(this._offset / PAGE_SIZE) + 1;

    return html`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Purchased kWh</th>
              <th>Cost SEK</th>
              <th>Sold kWh</th>
              <th>Profit SEK</th>
              <th>Own Use kWh</th>
              <th>Saved SEK</th>
              <th>Price Level</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(
              (r) => html`
                <tr>
                  <td>${this._formatTs(r.timestamp)}</td>
                  <td>${r.purchased.toFixed(3)}</td>
                  <td>${r.purchased_cost.toFixed(2)}</td>
                  <td>${r.production_sold.toFixed(3)}</td>
                  <td>${r.production_sold_profit.toFixed(2)}</td>
                  <td>${r.production_own_use.toFixed(3)}</td>
                  <td>${r.production_own_use_profit.toFixed(2)}</td>
                  <td>${r.price_level || "-"}</td>
                </tr>
              `
            )}
          </tbody>
        </table>
      </div>
      <div class="pagination">
        <button
          class="btn"
          ?disabled=${this._offset === 0 || this._loading}
          @click=${this._prevPage}
        >
          Prev
        </button>
        <span>Page ${currentPage} of ${totalPages} (${this._totalCount} records)</span>
        <button
          class="btn"
          ?disabled=${this._offset + PAGE_SIZE >= this._totalCount || this._loading}
          @click=${this._nextPage}
        >
          Next
        </button>
      </div>
    `;
  }

  private async _fetch() {
    if (!this.hass || !this.entryId || !this._startDate || !this._endDate)
      return;
    this._loading = true;
    this._error = "";
    try {
      // Build ISO start/end covering full days
      const start = `${this._startDate}T00:00:00`;
      // End date is exclusive, so add one day
      const endDate = new Date(this._endDate);
      endDate.setDate(endDate.getDate() + 1);
      const end = `${endDate.toISOString().substring(0, 10)}T00:00:00`;

      const result = await this.hass.callWS({
        type: "my_solar_cells/get_hourly_energy",
        entry_id: this.entryId,
        start_date: start,
        end_date: end,
        offset: this._offset,
        limit: PAGE_SIZE,
      });
      this._records = result.records;
      this._totalCount = result.total_count;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch data";
      this._records = [];
      this._totalCount = 0;
    }
    this._loading = false;
  }

  private _prevPage() {
    this._offset = Math.max(0, this._offset - PAGE_SIZE);
    this._fetch();
  }

  private _nextPage() {
    this._offset += PAGE_SIZE;
    this._fetch();
  }

  private _formatTs(ts: string): string {
    try {
      // Show local date+time compactly
      return ts.replace("T", " ").substring(0, 19);
    } catch {
      return ts;
    }
  }
}
