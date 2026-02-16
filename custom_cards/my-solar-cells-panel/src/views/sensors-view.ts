import { LitElement, html, css, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { t } from "../localize";

interface SensorInfo {
  role: string;
  description: string;
  entity_id: string | null;
  current_state: string | null;
  last_stored_reading: number | null;
}

@customElement("sensors-view")
export class SensorsView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _sensors: SensorInfo[] = [];
  @state() private _loading = false;
  @state() private _error = "";

  static styles = [
    cardStyles,
    tableStyles,
    css`
      .status-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 50%;
        margin-right: 8px;
      }
      .status-dot.configured {
        background: var(--success-color, #4caf50);
      }
      .status-dot.missing {
        background: var(--error-color, #f44336);
      }
      .status-dot.optional {
        background: var(--warning-color, #ff9800);
      }
      .fallback-label {
        color: var(--secondary-text-color);
        font-size: 0.85em;
        font-style: italic;
      }
      .required-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--error-color, #f44336);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .optional-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--secondary-text-color);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .entity-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--primary-text-color);
      }
      .not-configured {
        color: var(--secondary-text-color);
        font-style: italic;
      }
      .info-box {
        background: var(--primary-background-color);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.9em;
        color: var(--secondary-text-color);
        line-height: 1.5;
      }
    `,
  ];

  updated(changed: Map<string, unknown>) {
    if (
      changed.has("hass") &&
      this.hass &&
      this.entryId &&
      !this._sensors.length &&
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
        type: "my_solar_cells/get_sensor_config",
        entry_id: this.entryId,
      });
      this._sensors = result.sensors;
    } catch (e: any) {
      this._error = e.message || "Failed to fetch sensor config";
    }
    this._loading = false;
  }

  render(): TemplateResult {
    if (this._loading) {
      return html`<div class="loading">${t(this.hass, "sensors.loadingSensors")}</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">${t(this.hass, "common.error")}: ${this._error}</div>`;
    }

    const configured = this._sensors.filter((s) => s.entity_id);
    const missing = this._sensors.filter((s) => !s.entity_id);

    return html`
      <div class="info-box">${this._renderInfoBox()}</div>

      <div class="card">
        <h3>${t(this.hass, "sensors.title")}</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>${t(this.hass, "sensors.status")}</th>
                <th>${t(this.hass, "sensors.role")}</th>
                <th>${t(this.hass, "sensors.description")}</th>
                <th>${t(this.hass, "sensors.entityId")}</th>
                <th>${t(this.hass, "sensors.currentState")}</th>
                <th>${t(this.hass, "sensors.lastStoredReading")}</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map((s) => this._renderRow(s))}
            </tbody>
          </table>
        </div>
      </div>

      ${missing.some((s) => s.role === "production")
        ? html`
            <div class="card">
              <h3>${t(this.hass, "sensors.requiredMissing")}</h3>
              <p style="color: var(--error-color, #f44336); font-size: 0.9em;"
                >${this._renderRequiredMissingText()}</p>
            </div>
          `
        : nothing}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          ${t(this.hass, "common.refresh")}
        </button>
      </div>
    `;
  }

  private _isRequired(role: string): boolean {
    return role === "production";
  }

  private _getFallbackLabel(role: string): string {
    if (role === "grid_export") return t(this.hass, "sensors.usingTibberApi");
    if (role === "grid_import") return t(this.hass, "sensors.usingTibberApi");
    return t(this.hass, "sensors.notConfigured");
  }

  private _renderInfoBox(): TemplateResult {
    return html`${this._unsafeHtml(t(this.hass, "sensors.infoBox"))}`;
  }

  private _renderRequiredMissingText(): TemplateResult {
    return html`${this._unsafeHtml(t(this.hass, "sensors.requiredMissingText"))}`;
  }

  private _unsafeHtml(str: string): TemplateResult {
    const el = document.createElement("span");
    el.innerHTML = str;
    return html`${el}`;
  }

  private _renderRow(s: SensorInfo): TemplateResult {
    const isConfigured = !!s.entity_id;
    const required = this._isRequired(s.role);
    const dotClass = isConfigured
      ? "configured"
      : required
        ? "missing"
        : "optional";
    return html`
      <tr>
        <td>
          <span class="status-dot ${dotClass}"></span>
        </td>
        <td>
          <strong>${s.role}</strong>
          ${required
            ? html`<span class="required-badge">${t(this.hass, "sensors.required")}</span>`
            : html`<span class="optional-badge">${t(this.hass, "sensors.optional")}</span>`}
        </td>
        <td>${s.description}</td>
        <td>
          ${isConfigured
            ? html`<span class="entity-id">${s.entity_id}</span>`
            : html`<span class="fallback-label">${this._getFallbackLabel(s.role)}</span>`}
        </td>
        <td>
          ${s.current_state != null
            ? s.current_state
            : html`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${s.last_stored_reading != null
            ? s.last_stored_reading.toFixed(3)
            : html`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `;
  }
}
