import { LitElement, html, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { faktaStyles } from "../styles";
import type { HistoryStatsResponse } from "../types";

type PeriodType = "today" | "day" | "week" | "month" | "year";

const PERIOD_LABELS: Record<PeriodType, string> = {
  today: "Idag",
  day: "Dag",
  week: "Vecka",
  month: "M\u00e5nad",
  year: "\u00c5r",
};

const SWEDISH_MONTHS = [
  "JANUARI", "FEBRUARI", "MARS", "APRIL", "MAJ", "JUNI",
  "JULI", "AUGUSTI", "SEPTEMBER", "OKTOBER", "NOVEMBER", "DECEMBER",
];

const SHORT_MONTHS = [
  "JAN.", "FEB.", "MAR.", "APR.", "MAJ", "JUN.",
  "JUL.", "AUG.", "SEP.", "OKT.", "NOV.", "DEC.",
];

@customElement("fakta-view")
export class FaktaView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _period: PeriodType = "week";
  @state() private _currentDate: Date = new Date();
  @state() private _data: HistoryStatsResponse | null = null;
  @state() private _simData: HistoryStatsResponse | null = null;
  @state() private _loading = false;
  @state() private _simLoading = false;
  @state() private _error = "";
  @state() private _simEnabled = false;
  @state() private _simAddBattery = true;
  @state() private _simBatteryKwh = 10;
  @state() private _simRemoveTax = false;
  @state() private _initialLoaded = false;

  static styles = [faktaStyles];

  updated(changed: Map<string, unknown>) {
    if (changed.has("hass") && this.hass && this.entryId && !this._initialLoaded && !this._loading) {
      this._fetchData();
    }
  }

  private _getDateRange(): { start: string; end: string } {
    const d = new Date(this._currentDate);
    let start: Date;
    let end: Date;

    switch (this._period) {
      case "today": {
        const today = new Date();
        start = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        end = new Date(start);
        end.setDate(end.getDate() + 1);
        break;
      }
      case "day": {
        start = new Date(d.getFullYear(), d.getMonth(), d.getDate());
        end = new Date(start);
        end.setDate(end.getDate() + 1);
        break;
      }
      case "week": {
        // Monday of the week
        const day = d.getDay();
        const diff = day === 0 ? 6 : day - 1;
        start = new Date(d.getFullYear(), d.getMonth(), d.getDate() - diff);
        end = new Date(start);
        end.setDate(end.getDate() + 7);
        break;
      }
      case "month": {
        start = new Date(d.getFullYear(), d.getMonth(), 1);
        end = new Date(d.getFullYear(), d.getMonth() + 1, 1);
        break;
      }
      case "year": {
        start = new Date(d.getFullYear(), 0, 1);
        end = new Date(d.getFullYear() + 1, 0, 1);
        break;
      }
    }

    return {
      start: this._toLocalIso(start!),
      end: this._toLocalIso(end!),
    };
  }

  private _toLocalIso(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}T00:00:00`;
  }

  private _getPeriodLabel(): string {
    const d = new Date(this._currentDate);
    switch (this._period) {
      case "today":
        return "IDAG";
      case "day": {
        const dd = String(d.getDate()).padStart(2, "0");
        const mon = SHORT_MONTHS[d.getMonth()];
        return `${dd}/${mon} ${d.getFullYear()}`;
      }
      case "week": {
        const day = d.getDay();
        const diff = day === 0 ? 6 : day - 1;
        const mon = new Date(d.getFullYear(), d.getMonth(), d.getDate() - diff);
        const sun = new Date(mon);
        sun.setDate(sun.getDate() + 6);
        const monStr = `${String(mon.getDate()).padStart(2, "0")}/${SHORT_MONTHS[mon.getMonth()]}`;
        const sunStr = `${String(sun.getDate()).padStart(2, "0")}/${SHORT_MONTHS[sun.getMonth()]}`;
        return `${monStr}-${sunStr}`;
      }
      case "month":
        return `${SWEDISH_MONTHS[d.getMonth()]} ${d.getFullYear()}`;
      case "year":
        return `${d.getFullYear()}`;
    }
  }

  private _navigate(direction: number) {
    const d = new Date(this._currentDate);
    switch (this._period) {
      case "day":
        d.setDate(d.getDate() + direction);
        break;
      case "week":
        d.setDate(d.getDate() + 7 * direction);
        break;
      case "month":
        d.setMonth(d.getMonth() + direction);
        break;
      case "year":
        d.setFullYear(d.getFullYear() + direction);
        break;
    }
    this._currentDate = d;
    this._fetchData();
  }

  private _setPeriod(p: PeriodType) {
    this._period = p;
    this._currentDate = new Date();
    this._simData = null;
    this._fetchData();
  }

  private async _fetchData() {
    if (!this.hass || !this.entryId) return;
    this._loading = true;
    this._error = "";
    try {
      const range = this._getDateRange();
      const result = await this.hass.callWS({
        type: "my_solar_cells/get_fakta_breakdown",
        entry_id: this.entryId,
        start_date: range.start,
        end_date: range.end,
      });
      this._data = result;
      this._initialLoaded = true;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch data";
    }
    this._loading = false;
  }

  private async _simulate() {
    if (!this.hass || !this.entryId) return;
    this._simLoading = true;
    try {
      const range = this._getDateRange();
      const result = await this.hass.callWS({
        type: "my_solar_cells/simulate_fakta",
        entry_id: this.entryId,
        start_date: range.start,
        end_date: range.end,
        add_battery: this._simAddBattery,
        battery_kwh: this._simBatteryKwh,
        remove_tax_reduction: this._simRemoveTax,
      });
      this._simData = result;
      this._simEnabled = true;
    } catch (e: any) {
      this._error = e.message || "Simulation failed";
    }
    this._simLoading = false;
  }

  private _fmtKwh(v: number): string {
    return v.toLocaleString("sv-SE", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " kWh";
  }

  private _fmtSek(v: number): string {
    return v.toLocaleString("sv-SE", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " Sek";
  }

  private _fmtSekPerKwh(v: number): string {
    return v.toLocaleString("sv-SE", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " Sek";
  }

  render(): TemplateResult {
    if (this._loading && !this._initialLoaded) {
      return html`<div class="loading">Loading...</div>`;
    }
    if (this._error && !this._data) {
      return html`<div class="no-data">Error: ${this._error}</div>`;
    }

    const d = this._simEnabled && this._simData ? this._simData : this._data;

    return html`
      ${this._renderPeriodNav()}
      ${d ? this._renderColumns(d) : html`<div class="no-data">No data for this period</div>`}
    `;
  }

  private _renderPeriodNav(): TemplateResult {
    const showArrows = this._period !== "today";
    return html`
      <div class="period-nav">
        <div class="period-tabs">
          ${(Object.keys(PERIOD_LABELS) as PeriodType[]).map(
            (p) => html`
              <button
                class="period-tab ${this._period === p ? "active" : ""}"
                @click=${() => this._setPeriod(p)}
              >
                ${PERIOD_LABELS[p]}
              </button>
            `
          )}
        </div>
        ${showArrows
          ? html`
              <button class="nav-arrow" @click=${() => this._navigate(-1)}>&larr;</button>
              <span class="period-label">${this._getPeriodLabel()}</span>
              <button class="nav-arrow" @click=${() => this._navigate(1)}>&rarr;</button>
            `
          : nothing}
      </div>
    `;
  }

  private _renderColumns(d: HistoryStatsResponse): TemplateResult {
    return html`
      <div class="fakta-columns">
        ${this._renderProductionColumn(d)}
        ${this._renderCostColumn(d)}
        ${this._renderSimAndFactsColumn(d)}
      </div>
    `;
  }

  private _renderProductionColumn(d: HistoryStatsResponse): TemplateResult {
    return html`
      <div class="fakta-card">
        <h3>Produktion och konsumtion</h3>
        <div class="fakta-row"><span class="label">S\u00e5ld</span><span class="value">${this._fmtKwh(d.production_sold)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv.</span><span class="value">${this._fmtKwh(d.production_own_use)}</span></div>
        <div class="fakta-row"><span class="label">Batteriladdning</span><span class="value">${this._fmtKwh(d.battery_charge)}</span></div>
        <div class="fakta-row"><span class="label">Batteri anv.</span><span class="value">${this._fmtKwh(d.battery_used)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt</span><span class="value">${this._fmtKwh(d.purchased)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Produktion</span><span class="value">${this._fmtKwh(d.sum_all_production)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Konsumtion</span><span class="value">${this._fmtKwh(d.sum_all_consumption)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Balans (prod. - f\u00f6rb.)</span><span class="value">${this._fmtKwh(d.sum_all_production - d.sum_all_consumption)}</span></div>
      </div>
    `;
  }

  private _renderCostColumn(d: HistoryStatsResponse): TemplateResult {
    return html`
      <div class="fakta-card">
        <h3>Kostnader och int\u00e4kter</h3>
        <div class="fakta-row"><span class="label">Prod s\u00e5lt</span><span class="value">${this._fmtSek(d.production_sold_profit)}</span></div>
        <div class="fakta-row"><span class="label">Prod n\u00e4tnytta</span><span class="value">${this._fmtSek(d.production_sold_grid_compensation_profit)}</span></div>
        <div class="fakta-row"><span class="label">Prod energiskatt</span><span class="value">${this._fmtSek(d.production_sold_tax_reduction_profit)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. spotpris</span><span class="value">${this._fmtSek(d.production_own_use_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(d.production_own_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. energiskatt</span><span class="value">${this._fmtSek(d.production_own_use_energy_tax_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. spotpris</span><span class="value">${this._fmtSek(d.battery_used_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(d.battery_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. energiskatt</span><span class="value">${this._fmtSek(d.battery_use_energy_tax_saved)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row"><span class="label">K\u00f6pt kostnad</span><span class="value">${this._fmtSek(-d.purchased_cost)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(-d.purchased_transfer_fee_cost)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt energiskatt</span><span class="value">${this._fmtSek(-d.purchased_tax_cost)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Inkl. kostnad</span><span class="value">${this._fmtSek(d.sum_all_production_sold_and_saved)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Exkl. kostnad</span><span class="value">${this._fmtSek(d.sum_all_production_sold_and_saved - d.sum_purchased_cost)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Produktion</span><span class="value">${this._fmtSek(d.sum_all_production_sold_and_saved)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Konsumtion</span><span class="value">${this._fmtSek(-d.sum_purchased_cost)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Batt. (prod. - f\u00f6rb.)</span><span class="value">${this._fmtSek(d.balance)}</span></div>
      </div>
    `;
  }

  private _renderSimAndFactsColumn(d: HistoryStatsResponse): TemplateResult {
    return html`
      <div class="fakta-card">
        <h3>Simulering</h3>
        <div class="sim-section">
          <div class="sim-toggle-group">
            <button
              class="sim-toggle ${this._simAddBattery ? "active" : ""}"
              @click=${() => { this._simAddBattery = true; }}
            >L\u00e4gg till batteri</button>
            <button
              class="sim-toggle ${!this._simAddBattery ? "active" : ""}"
              @click=${() => { this._simAddBattery = false; }}
            >Ta bort batteri</button>
          </div>
          ${this._simAddBattery
            ? html`
                <div class="sim-slider-row">
                  <input
                    type="range"
                    min="1"
                    max="30"
                    step="1"
                    .value=${String(this._simBatteryKwh)}
                    @input=${(e: Event) => {
                      this._simBatteryKwh = parseInt((e.target as HTMLInputElement).value);
                    }}
                  />
                  <span class="slider-value">${this._simBatteryKwh} kWh</span>
                </div>
              `
            : nothing}
          <button
            class="sim-btn"
            @click=${this._simulate}
            ?disabled=${this._simLoading}
          >
            ${this._simLoading ? "Ber\u00e4knar..." : "Ber\u00e4kna"}
          </button>
        </div>

        <h3>Fakta</h3>
        <div class="fakta-row"><span class="label">Produktionsindex (prod/dag)</span><span class="value">${this._fmtKwh(d.facts_production_index)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris s\u00e5ld</span><span class="value">${this._fmtSekPerKwh(d.facts_production_sold_avg_per_kwh_profit)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris k\u00f6pt</span><span class="value">${this._fmtSekPerKwh(d.facts_purchased_cost_avg_per_kwh)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris eget anv\u00e4ndning</span><span class="value">${this._fmtSekPerKwh(d.facts_production_own_use_avg_per_kwh_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. reducering avg. f\u00f6rb.</span><span class="value">${this._fmtKwh(d.peak_energy_reduction)}</span></div>
      </div>
    `;
  }
}
