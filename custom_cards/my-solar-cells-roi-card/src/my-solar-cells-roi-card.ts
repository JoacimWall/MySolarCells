import { LitElement, html, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { MySolarCellsRoiCardConfig, RoiProjectionEntry } from "./types";
import { cardStyles } from "./styles";
import "./roi-chart";

@customElement("my-solar-cells-roi-card")
export class MySolarCellsRoiCard extends LitElement {
  @property({ attribute: false }) hass: any;
  @state() private _config?: MySolarCellsRoiCardConfig;

  static styles = cardStyles;

  setConfig(config: MySolarCellsRoiCardConfig): void {
    if (!config.entity) {
      throw new Error("You need to define an entity");
    }
    this._config = {
      show_chart: true,
      show_table: false,
      title: "Solar ROI Projection",
      ...config,
    };
  }

  getCardSize(): number {
    return this._config?.show_chart ? 6 : 3;
  }

  render(): TemplateResult {
    if (!this._config || !this.hass) {
      return html`<ha-card><div class="no-data">Loading...</div></ha-card>`;
    }

    const entityId = this._config.entity;
    const stateObj = this.hass.states[entityId];

    if (!stateObj) {
      return html`
        <ha-card>
          <div class="no-data">Entity not found: ${entityId}</div>
        </ha-card>
      `;
    }

    const projection: RoiProjectionEntry[] =
      stateObj.attributes.projection || [];
    const investment: number = stateObj.attributes.investment_amount || 0;
    const totalSavings: number = stateObj.attributes.total_savings_to_date || 0;
    const paybackYear = stateObj.state !== "unknown" ? stateObj.state : null;

    const remaining = Math.max(0, investment - totalSavings);

    return html`
      <ha-card>
        <div class="header">
          <h2>${this._config.title}</h2>
        </div>

        <div class="summary">
          <div class="summary-item">
            <div class="label">Total Savings</div>
            <div class="value positive">
              ${this._formatCurrency(totalSavings)}
            </div>
          </div>
          <div class="summary-item">
            <div class="label">Remaining</div>
            <div class="value">${this._formatCurrency(remaining)}</div>
          </div>
          <div class="summary-item">
            <div class="label">Payback Year</div>
            <div class="value highlight">${paybackYear || "—"}</div>
          </div>
        </div>

        ${this._config.show_chart && projection.length > 0
          ? html`
              <roi-chart
                .projection=${projection}
                .investment=${investment}
              ></roi-chart>
            `
          : nothing}
        ${this._config.show_table && projection.length > 0
          ? this._renderTable(projection)
          : nothing}
        ${projection.length === 0
          ? html`<div class="no-data">
              No projection data available yet. Data will appear after the first
              sync.
            </div>`
          : nothing}
      </ha-card>
    `;
  }

  private _renderTable(projection: RoiProjectionEntry[]): TemplateResult {
    return html`
      <table>
        <thead>
          <tr>
            <th>Year</th>
            <th>Sold</th>
            <th>Own Use</th>
            <th>Total</th>
            <th>Remaining</th>
          </tr>
        </thead>
        <tbody>
          ${projection.map(
            (entry) => html`
              <tr class="${entry.is_roi_year ? "roi-year" : ""}">
                <td>${entry.year_from_start} (${entry.year})</td>
                <td>${this._formatCurrency(entry.year_savings_sold)}</td>
                <td>${this._formatCurrency(entry.year_savings_own_use)}</td>
                <td>
                  ${this._formatCurrency(
                    entry.year_savings_sold + entry.year_savings_own_use
                  )}
                </td>
                <td>${this._formatCurrency(entry.remaining_on_investment)}</td>
              </tr>
            `
          )}
        </tbody>
      </table>
    `;
  }

  private _formatCurrency(value: number): string {
    return (
      new Intl.NumberFormat("sv-SE", {
        style: "decimal",
        maximumFractionDigits: 0,
      }).format(value) + " SEK"
    );
  }

  static getStubConfig() {
    return {
      entity: "sensor.my_solar_cells_roi_payback_year",
      show_chart: true,
      show_table: false,
      title: "Solar ROI Projection",
    };
  }
}

// Register the card with Home Assistant
(window as any).customCards = (window as any).customCards || [];
(window as any).customCards.push({
  type: "my-solar-cells-roi-card",
  name: "My Solar Cells ROI Card",
  description: "Displays 30-year solar investment ROI projection with chart",
  preview: true,
});
