import { LitElement, html, css, TemplateResult, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import { RoiProjectionEntry } from "./types";

@customElement("roi-chart")
export class RoiChart extends LitElement {
  @property({ type: Array }) projection: RoiProjectionEntry[] = [];
  @property({ type: Number }) investment = 0;

  static styles = css`
    :host {
      display: block;
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
  `;

  render(): TemplateResult {
    if (!this.projection || this.projection.length === 0) {
      return html`<div>No projection data</div>`;
    }

    // Calculate max cumulative savings for scaling
    let cumulative = 0;
    const cumulativeValues = this.projection.map((entry) => {
      cumulative += entry.year_savings_sold + entry.year_savings_own_use;
      return cumulative;
    });
    const maxCumulative = Math.max(...cumulativeValues, this.investment);
    const chartHeight = 240;

    // Investment line position
    const investmentLineTop =
      this.investment > 0
        ? chartHeight - (this.investment / maxCumulative) * chartHeight
        : -1;

    let runningPayback = false;

    return html`
      <div class="chart-wrapper">
        ${this.investment > 0 && investmentLineTop >= 0
          ? html`
              <div
                class="investment-line"
                style="top: ${investmentLineTop}px"
              >
                <span class="line-label"
                  >Investment: ${this._formatCurrency(this.investment)}</span
                >
              </div>
            `
          : nothing}
        <div class="bars-container">
          ${this.projection.map((entry, i) => {
            const savings = cumulativeValues[i];
            const height = (savings / maxCumulative) * chartHeight;
            let barClass = "bar before-payback";
            if (entry.is_roi_year) {
              barClass = "bar payback-year";
              runningPayback = true;
            } else if (runningPayback) {
              barClass = "bar after-payback";
            }
            return html`
              <div class="bar-wrapper">
                <div
                  class="${barClass}"
                  style="height: ${Math.max(2, height)}px"
                  title="Year ${entry.year}: ${this._formatCurrency(
                    savings
                  )} cumulative"
                ></div>
              </div>
            `;
          })}
        </div>
        <div class="labels">
          ${this.projection.map((entry, i) =>
            i % 5 === 0
              ? html`<span>${entry.year}</span>`
              : html`<span></span>`
          )}
        </div>
      </div>
    `;
  }

  private _formatCurrency(value: number): string {
    return new Intl.NumberFormat("sv-SE", {
      style: "decimal",
      maximumFractionDigits: 0,
    }).format(value) + " SEK";
  }
}
