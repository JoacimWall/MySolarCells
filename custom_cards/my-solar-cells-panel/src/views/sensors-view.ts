import { LitElement, html, css, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";

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
      return html`<div class="loading">Loading sensor configuration...</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">Error: ${this._error}</div>`;
    }

    const configured = this._sensors.filter((s) => s.entity_id);
    const missing = this._sensors.filter((s) => !s.entity_id);

    return html`
      <div class="info-box">
        These HA sensors are used to enrich Tibber data with production and
        battery information. <strong>production_own_use</strong> is calculated
        as: total production (from HA sensor) minus grid export (from Tibber
        API). Only the <strong>production</strong> sensor needs to be configured
        for this calculation. Sensors are configured in the integration setup
        flow.
      </div>

      <div class="card">
        <h3>Sensor Configuration</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Status</th>
                <th>Role</th>
                <th>Description</th>
                <th>Entity ID</th>
                <th>Current State</th>
                <th>Last Stored Reading</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map((s) => this._renderRow(s))}
            </tbody>
          </table>
        </div>
      </div>

      ${missing.length > 0
        ? html`
            <div class="card">
              <h3>Missing Sensors</h3>
              <p style="color: var(--secondary-text-color); font-size: 0.9em;">
                The following sensors are not configured. To calculate
                <strong>production_own_use</strong>, you need the
                <strong>production</strong> sensor configured. Grid export
                data is fetched from the Tibber API automatically.
              </p>
              <ul style="color: var(--secondary-text-color); font-size: 0.9em;">
                ${missing.map(
                  (s) =>
                    html`<li>${s.description} (<code>${s.role}</code>)</li>`
                )}
              </ul>
            </div>
          `
        : nothing}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          Refresh
        </button>
      </div>
    `;
  }

  private _renderRow(s: SensorInfo): TemplateResult {
    const isConfigured = !!s.entity_id;
    return html`
      <tr>
        <td>
          <span
            class="status-dot ${isConfigured ? "configured" : "missing"}"
          ></span>
        </td>
        <td><strong>${s.role}</strong></td>
        <td>${s.description}</td>
        <td>
          ${isConfigured
            ? html`<span class="entity-id">${s.entity_id}</span>`
            : html`<span class="not-configured">Not configured</span>`}
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
