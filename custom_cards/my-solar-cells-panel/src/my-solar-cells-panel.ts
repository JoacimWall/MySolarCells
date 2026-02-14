import { LitElement, html, css, TemplateResult } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { panelStyles } from "./styles";
import "./views/overview-view";
import "./views/hourly-energy-view";
import "./views/sensors-view";

type TabName = "overview" | "hourly" | "sensors";

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
    `,
  ];

  private get _entryId(): string {
    return this.panel?.config?.entry_id || "";
  }

  render(): TemplateResult {
    return html`
      <div class="content">
        <div class="header">
          <h1>Solar Data Browser</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${this._activeTab === "overview" ? "active" : ""}"
            @click=${() => (this._activeTab = "overview")}
          >
            Overview
          </button>
          <button
            class="tab ${this._activeTab === "hourly" ? "active" : ""}"
            @click=${() => (this._activeTab = "hourly")}
          >
            Hourly Energy
          </button>
          <button
            class="tab ${this._activeTab === "sensors" ? "active" : ""}"
            @click=${() => (this._activeTab = "sensors")}
          >
            Sensors
          </button>
        </div>
        ${this._renderActiveTab()}
      </div>
    `;
  }

  private _renderActiveTab(): TemplateResult {
    switch (this._activeTab) {
      case "overview":
        return html`
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        `;
      case "hourly":
        return html`
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        `;
      case "sensors":
        return html`
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        `;
    }
  }
}
