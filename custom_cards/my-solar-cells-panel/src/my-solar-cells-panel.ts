import { LitElement, html, css, TemplateResult } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { panelStyles } from "./styles";
import { t } from "./localize";
import "./views/overview-view";
import "./views/hourly-energy-view";
import "./views/sensors-view";
import "./views/yearly-params-view";
import "./views/roi-view";
import "./views/fakta-view";

type TabName = "overview" | "hourly" | "sensors" | "params" | "roi" | "fakta";

@customElement("my-solar-cells-panel")
export class MySolarCellsPanel extends LitElement {
  @property({ attribute: false }) hass: any;
  @property({ attribute: false }) narrow!: boolean;
  @property({ attribute: false }) route: any;
  @property({ attribute: false }) panel: any;

  @state() private _activeTab: TabName = "overview";

  static styles = [
    panelStyles,
    css`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }

      .tab-content {
        display: none;
      }

      .tab-content[active] {
        display: block;
      }
    `,
  ];

  private get _entryId(): string {
    return this.panel?.config?.entry_id || "";
  }

  render(): TemplateResult {
    return html`
      <div class="content">
        <div class="header">
          <h1>${t(this.hass, "panel.title")}</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${this._activeTab === "overview" ? "active" : ""}"
            @click=${() => (this._activeTab = "overview")}
          >
            ${t(this.hass, "tab.overview")}
          </button>
          <button
            class="tab ${this._activeTab === "roi" ? "active" : ""}"
            @click=${() => (this._activeTab = "roi")}
          >
            ${t(this.hass, "tab.roi")}
          </button>
          <button
            class="tab ${this._activeTab === "fakta" ? "active" : ""}"
            @click=${() => (this._activeTab = "fakta")}
          >
            ${t(this.hass, "tab.fakta")}
          </button>
          <button
            class="tab ${this._activeTab === "sensors" ? "active" : ""}"
            @click=${() => (this._activeTab = "sensors")}
          >
            ${t(this.hass, "tab.sensors")}
          </button>
          <button
            class="tab ${this._activeTab === "params" ? "active" : ""}"
            @click=${() => (this._activeTab = "params")}
          >
            ${t(this.hass, "tab.yearlyParams")}
          </button>
          <button
            class="tab ${this._activeTab === "hourly" ? "active" : ""}"
            @click=${() => (this._activeTab = "hourly")}
          >
            ${t(this.hass, "tab.hourlyEnergy")}
          </button>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "overview"}>
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "roi"}>
          <roi-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></roi-view>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "fakta"}>
          <fakta-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></fakta-view>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "sensors"}>
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "params"}>
          <yearly-params-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></yearly-params-view>
        </div>
        <div class="tab-content" ?active=${this._activeTab === "hourly"}>
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        </div>
      </div>
    `;
  }
}
