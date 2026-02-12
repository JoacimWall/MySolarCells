/**
 * My Solar Cells ROI Card - Custom Lovelace Card
 * Displays 30-year solar investment ROI projection with chart
 */
console.info(
  "%c MY-SOLAR-CELLS-ROI-CARD %c 1.0.0 ",
  "color: white; background: #4caf50; font-weight: 700;",
  "color: #4caf50; background: white; font-weight: 700;"
);

class MySolarCellsRoiCard extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: "open" });
    this._config = null;
    this._hass = null;
  }

  set hass(hass) {
    this._hass = hass;
    this._render();
  }

  setConfig(config) {
    if (!config.entity) {
      throw new Error("You need to define an entity");
    }
    this._config = {
      show_chart: true,
      show_table: false,
      title: "Solar ROI Projection",
      ...config,
    };
    this._render();
  }

  getCardSize() {
    return this._config && this._config.show_chart ? 6 : 3;
  }

  static getStubConfig() {
    return {
      entity: "sensor.my_solar_cells_roi_payback_year",
      show_chart: true,
      show_table: false,
      title: "Solar ROI Projection",
    };
  }

  _formatCurrency(value) {
    return (
      new Intl.NumberFormat("sv-SE", {
        style: "decimal",
        maximumFractionDigits: 0,
      }).format(value) + " SEK"
    );
  }

  _render() {
    if (!this._config || !this._hass) return;

    const entityId = this._config.entity;
    const stateObj = this._hass.states[entityId];

    if (!stateObj) {
      this.shadowRoot.innerHTML = `
        <ha-card>
          <style>${MySolarCellsRoiCard.CSS}</style>
          <div class="no-data">Entity not found: ${entityId}</div>
        </ha-card>
      `;
      return;
    }

    const projection = stateObj.attributes.projection || [];
    const investment = stateObj.attributes.investment_amount || 0;
    const totalSavings = stateObj.attributes.total_savings_to_date || 0;
    const paybackYear =
      stateObj.state !== "unknown" && stateObj.state !== "unavailable"
        ? stateObj.state
        : null;
    const remaining = Math.max(0, investment - totalSavings);

    let chartHtml = "";
    if (this._config.show_chart && projection.length > 0) {
      chartHtml = this._renderChart(projection, investment);
    }

    let tableHtml = "";
    if (this._config.show_table && projection.length > 0) {
      tableHtml = this._renderTable(projection);
    }

    let noDataHtml = "";
    if (projection.length === 0) {
      noDataHtml = `<div class="no-data">No projection data available yet. Data will appear after the first sync.</div>`;
    }

    this.shadowRoot.innerHTML = `
      <ha-card>
        <style>${MySolarCellsRoiCard.CSS}</style>
        <div class="header">
          <h2>${this._config.title}</h2>
        </div>
        <div class="summary">
          <div class="summary-item">
            <div class="label">Total Savings</div>
            <div class="value positive">${this._formatCurrency(totalSavings)}</div>
          </div>
          <div class="summary-item">
            <div class="label">Remaining</div>
            <div class="value">${this._formatCurrency(remaining)}</div>
          </div>
          <div class="summary-item">
            <div class="label">Payback Year</div>
            <div class="value highlight">${paybackYear || "\u2014"}</div>
          </div>
        </div>
        ${chartHtml}
        ${tableHtml}
        ${noDataHtml}
      </ha-card>
    `;
  }

  _renderChart(projection, investment) {
    let cumulative = 0;
    const cumulativeValues = projection.map((entry) => {
      cumulative += entry.year_savings_sold + entry.year_savings_own_use;
      return cumulative;
    });
    const maxCumulative = Math.max(...cumulativeValues, investment);
    const chartHeight = 240;

    const investmentLineTop =
      investment > 0
        ? chartHeight - (investment / maxCumulative) * chartHeight
        : -1;

    let investmentLineHtml = "";
    if (investment > 0 && investmentLineTop >= 0) {
      investmentLineHtml = `
        <div class="investment-line" style="top: ${investmentLineTop}px">
          <span class="line-label">Investment: ${this._formatCurrency(investment)}</span>
        </div>
      `;
    }

    let passedPayback = false;
    const barsHtml = projection
      .map((entry, i) => {
        const savings = cumulativeValues[i];
        const height = (savings / maxCumulative) * chartHeight;
        let barClass = "bar before-payback";
        if (entry.is_roi_year) {
          barClass = "bar payback-year";
          passedPayback = true;
        } else if (passedPayback) {
          barClass = "bar after-payback";
        }
        return `
          <div class="bar-wrapper">
            <div class="${barClass}"
                 style="height: ${Math.max(2, height)}px"
                 title="Year ${entry.year}: ${this._formatCurrency(savings)} cumulative">
            </div>
          </div>
        `;
      })
      .join("");

    const labelsHtml = projection
      .map((entry, i) =>
        i % 5 === 0
          ? `<span>${entry.year}</span>`
          : `<span></span>`
      )
      .join("");

    return `
      <div class="chart-wrapper">
        ${investmentLineHtml}
        <div class="bars-container">${barsHtml}</div>
        <div class="labels">${labelsHtml}</div>
      </div>
    `;
  }

  _renderTable(projection) {
    const rows = projection
      .map((entry) => {
        const total = entry.year_savings_sold + entry.year_savings_own_use;
        return `
          <tr class="${entry.is_roi_year ? "roi-year" : ""}">
            <td>${entry.year_from_start} (${entry.year})</td>
            <td>${this._formatCurrency(entry.year_savings_sold)}</td>
            <td>${this._formatCurrency(entry.year_savings_own_use)}</td>
            <td>${this._formatCurrency(total)}</td>
            <td>${this._formatCurrency(entry.remaining_on_investment)}</td>
          </tr>
        `;
      })
      .join("");

    return `
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
        <tbody>${rows}</tbody>
      </table>
    `;
  }
}

MySolarCellsRoiCard.CSS = `
  :host {
    display: block;
  }
  ha-card {
    padding: 16px;
  }
  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }
  .header h2 {
    margin: 0;
    font-size: 1.2em;
    font-weight: 500;
  }
  .summary {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 12px;
    margin-bottom: 20px;
  }
  .summary-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }
  .summary-item .label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }
  .summary-item .value {
    font-size: 1.4em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
  .summary-item .value.positive {
    color: var(--success-color, #4caf50);
  }
  .summary-item .value.highlight {
    color: var(--accent-color, #03a9f4);
  }
  .chart-wrapper {
    position: relative;
    width: 100%;
    height: 280px;
    overflow: hidden;
  }
  .bars-container {
    display: flex;
    align-items: flex-end;
    gap: 1px;
    height: 240px;
    padding: 0 2px;
  }
  .bar-wrapper {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 100%;
    justify-content: flex-end;
  }
  .bar {
    width: 100%;
    min-width: 6px;
    max-width: 20px;
    border-radius: 2px 2px 0 0;
    transition: opacity 0.2s;
  }
  .bar:hover {
    opacity: 0.8;
  }
  .bar.before-payback {
    background: var(--disabled-color, #9e9e9e);
  }
  .bar.after-payback {
    background: var(--success-color, #4caf50);
  }
  .bar.payback-year {
    background: var(--accent-color, #03a9f4);
  }
  .labels {
    display: flex;
    gap: 1px;
    padding: 4px 2px 0;
  }
  .labels span {
    flex: 1;
    text-align: center;
    font-size: 0.55em;
    color: var(--secondary-text-color);
    overflow: hidden;
  }
  .investment-line {
    position: absolute;
    left: 0;
    right: 0;
    border-top: 2px dashed var(--error-color, #f44336);
    pointer-events: none;
    z-index: 1;
  }
  .line-label {
    position: absolute;
    right: 4px;
    transform: translateY(-100%);
    font-size: 0.65em;
    color: var(--error-color, #f44336);
    background: var(--card-background-color);
    padding: 0 4px;
  }
  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }
  th {
    text-align: left;
    padding: 6px 4px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
  }
  td {
    padding: 4px;
    border-bottom: 1px solid var(--divider-color);
  }
  tr.roi-year td {
    font-weight: 600;
    color: var(--accent-color, #03a9f4);
  }
`;

customElements.define("my-solar-cells-roi-card", MySolarCellsRoiCard);

window.customCards = window.customCards || [];
window.customCards.push({
  type: "my-solar-cells-roi-card",
  name: "My Solar Cells ROI Card",
  description: "Displays 30-year solar investment ROI projection with chart",
  preview: true,
});
