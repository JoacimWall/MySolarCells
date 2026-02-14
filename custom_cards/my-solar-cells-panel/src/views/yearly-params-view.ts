import { LitElement, html, css, TemplateResult, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { cardStyles, tableStyles } from "../styles";
import { YearlyParams, YearlyParamsResponse } from "../types";

const DEFAULT_PARAMS: YearlyParams = {
  tax_reduction: 0.6,
  grid_compensation: 0.078,
  transfer_fee: 0.3,
  energy_tax: 0.49,
  installed_kw: 10,
};

@customElement("yearly-params-view")
export class YearlyParamsView extends LitElement {
  @property({ attribute: false }) hass: any;
  @property() entryId = "";

  @state() private _params: Record<string, YearlyParams> = {};
  @state() private _loading = false;
  @state() private _fetched = false;
  @state() private _error = "";
  @state() private _editingYear: string | null = null;
  @state() private _editValues: YearlyParams = { ...DEFAULT_PARAMS };
  @state() private _newYear = "";
  @state() private _saving = false;
  @state() private _minYear = 0;
  @state() private _maxYear = 0;

  static styles = [
    cardStyles,
    tableStyles,
    css`
      .edit-form {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 12px;
        margin-top: 12px;
      }

      .edit-form .input-group input {
        width: 100%;
        box-sizing: border-box;
      }

      .form-actions {
        display: flex;
        gap: 8px;
        margin-top: 16px;
        align-items: center;
      }

      .btn-danger {
        background: var(--error-color, #db4437) !important;
      }

      .btn-secondary {
        background: var(--secondary-text-color) !important;
      }

      .add-year-row {
        display: flex;
        gap: 8px;
        align-items: flex-end;
        margin-bottom: 16px;
      }

      .add-year-row .input-group select {
        width: 100px;
        padding: 6px 8px;
        border: 1px solid var(--divider-color, #e0e0e0);
        border-radius: 4px;
        background: var(--card-background-color, #fff);
        color: var(--primary-text-color);
        font-size: 14px;
      }

      tr.clickable {
        cursor: pointer;
      }

      tr.clickable:hover td {
        background: var(--primary-background-color);
      }

      tr.selected td {
        background: color-mix(in srgb, var(--primary-color) 12%, transparent);
      }
    `,
  ];

  updated(changed: Map<string, unknown>) {
    if (
      (changed.has("hass") || changed.has("entryId")) &&
      this.hass &&
      this.entryId &&
      !this._loading &&
      !this._fetched
    ) {
      this._fetchData();
    }
  }

  private async _fetchData() {
    if (!this.hass || !this.entryId) return;
    this._loading = true;
    this._error = "";
    try {
      const resp: YearlyParamsResponse = await this.hass.callWS({
        type: "my_solar_cells/get_yearly_params",
        entry_id: this.entryId,
      });
      this._params = resp.yearly_params;
      if (resp.first_timestamp) {
        this._minYear = new Date(resp.first_timestamp).getFullYear();
      }
      if (resp.last_timestamp) {
        this._maxYear = new Date(resp.last_timestamp).getFullYear();
      }
    } catch (e: any) {
      this._error = e.message || "Failed to fetch yearly params";
    }
    this._loading = false;
    this._fetched = true;
  }

  render(): TemplateResult {
    if (this._loading) {
      return html`<div class="loading">Loading yearly parameters...</div>`;
    }
    if (this._error) {
      return html`<div class="no-data">Error: ${this._error}</div>`;
    }

    const years = Object.keys(this._params).sort();

    return html`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>

        <div class="add-year-row">
          <div class="input-group">
            <label>Add Year</label>
            <select
              .value=${this._newYear}
              @change=${(e: Event) =>
                (this._newYear = (e.target as HTMLSelectElement).value)}
            >
              <option value="">Select year...</option>
              ${this._getAvailableYears().map(
                (y) => html`<option value=${y}>${y}</option>`
              )}
            </select>
          </div>
          <button class="btn" @click=${this._addYear}>Add</button>
        </div>

        ${years.length === 0
          ? html`<div class="no-data">
              No yearly parameters configured yet. Add a year above.
            </div>`
          : html`
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Year</th>
                      <th>Tax Reduction</th>
                      <th>Grid Comp.</th>
                      <th>Transfer Fee</th>
                      <th>Energy Tax</th>
                      <th>Installed kW</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${years.map((year) => {
                      const p = this._params[year];
                      const isSelected = this._editingYear === year;
                      return html`
                        <tr
                          class="clickable ${isSelected ? "selected" : ""}"
                          @click=${() => this._startEdit(year)}
                        >
                          <td>${year}</td>
                          <td>${this._fmt(p.tax_reduction)}</td>
                          <td>${this._fmt(p.grid_compensation)}</td>
                          <td>${this._fmt(p.transfer_fee)}</td>
                          <td>${this._fmt(p.energy_tax)}</td>
                          <td>${p.installed_kw != null ? p.installed_kw : "-"}</td>
                        </tr>
                      `;
                    })}
                  </tbody>
                </table>
              </div>
            `}
      </div>

      ${this._editingYear != null ? this._renderEditForm() : nothing}
    `;
  }

  private _renderEditForm(): TemplateResult {
    const v = this._editValues;
    return html`
      <div class="card">
        <h3>Edit ${this._editingYear}</h3>
        <div class="edit-form">
          <div class="input-group">
            <label>Tax Reduction (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(v.tax_reduction ?? "")}
              @input=${(e: Event) =>
                (this._editValues = {
                  ...this._editValues,
                  tax_reduction: parseFloat(
                    (e.target as HTMLInputElement).value
                  ),
                })}
            />
          </div>
          <div class="input-group">
            <label>Grid Compensation (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(v.grid_compensation ?? "")}
              @input=${(e: Event) =>
                (this._editValues = {
                  ...this._editValues,
                  grid_compensation: parseFloat(
                    (e.target as HTMLInputElement).value
                  ),
                })}
            />
          </div>
          <div class="input-group">
            <label>Transfer Fee (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(v.transfer_fee ?? "")}
              @input=${(e: Event) =>
                (this._editValues = {
                  ...this._editValues,
                  transfer_fee: parseFloat(
                    (e.target as HTMLInputElement).value
                  ),
                })}
            />
          </div>
          <div class="input-group">
            <label>Energy Tax (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(v.energy_tax ?? "")}
              @input=${(e: Event) =>
                (this._editValues = {
                  ...this._editValues,
                  energy_tax: parseFloat(
                    (e.target as HTMLInputElement).value
                  ),
                })}
            />
          </div>
          <div class="input-group">
            <label>Installed kW</label>
            <input
              type="number"
              step="0.01"
              .value=${String(v.installed_kw ?? "")}
              @input=${(e: Event) =>
                (this._editValues = {
                  ...this._editValues,
                  installed_kw: parseFloat(
                    (e.target as HTMLInputElement).value
                  ),
                })}
            />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn" ?disabled=${this._saving} @click=${this._save}>
            ${this._saving ? "Saving..." : "Save"}
          </button>
          <button class="btn btn-secondary" @click=${this._cancelEdit}>
            Cancel
          </button>
          <button class="btn btn-danger" @click=${this._delete}>Delete</button>
        </div>
      </div>
    `;
  }

  private _fmt(val: number | undefined): string {
    return val != null ? val.toFixed(3) : "-";
  }

  private _startEdit(year: string) {
    this._editingYear = year;
    const p = this._params[year] || {};
    this._editValues = {
      tax_reduction: p.tax_reduction ?? DEFAULT_PARAMS.tax_reduction,
      grid_compensation:
        p.grid_compensation ?? DEFAULT_PARAMS.grid_compensation,
      transfer_fee: p.transfer_fee ?? DEFAULT_PARAMS.transfer_fee,
      energy_tax: p.energy_tax ?? DEFAULT_PARAMS.energy_tax,
      installed_kw: p.installed_kw ?? DEFAULT_PARAMS.installed_kw,
    };
  }

  private _cancelEdit() {
    this._editingYear = null;
  }

  private _getAvailableYears(): number[] {
    const years: number[] = [];
    for (let y = this._minYear; y <= this._maxYear; y++) {
      if (!this._params[String(y)]) {
        years.push(y);
      }
    }
    return years;
  }

  private _addYear() {
    const year = parseInt(this._newYear, 10);
    if (isNaN(year)) return;
    const yearStr = String(year);
    if (this._params[yearStr]) {
      this._startEdit(yearStr);
      this._newYear = "";
      return;
    }

    // Pre-fill from the most recent prior year (carry-forward) or defaults
    const priorYears = Object.keys(this._params)
      .filter((y) => y < yearStr)
      .sort();
    const source =
      priorYears.length > 0
        ? this._params[priorYears[priorYears.length - 1]]
        : null;
    this._editValues = source ? { ...source } : { ...DEFAULT_PARAMS };
    this._editingYear = yearStr;
    this._newYear = "";
  }

  private async _save() {
    if (this._editingYear == null || !this.hass || !this.entryId) return;
    this._saving = true;
    try {
      await this.hass.callWS({
        type: "my_solar_cells/set_yearly_params",
        entry_id: this.entryId,
        year: parseInt(this._editingYear, 10),
        tax_reduction: this._editValues.tax_reduction ?? 0,
        grid_compensation: this._editValues.grid_compensation ?? 0,
        transfer_fee: this._editValues.transfer_fee ?? 0,
        energy_tax: this._editValues.energy_tax ?? 0,
        installed_kw: this._editValues.installed_kw ?? 0,
      });
      this._editingYear = null;
      await this._fetchData();
    } catch (e: any) {
      this._error = e.message || "Failed to save";
    }
    this._saving = false;
  }

  private async _delete() {
    if (this._editingYear == null || !this.hass || !this.entryId) return;
    if (!confirm(`Delete parameters for ${this._editingYear}?`)) return;
    this._saving = true;
    try {
      await this.hass.callWS({
        type: "my_solar_cells/delete_yearly_params",
        entry_id: this.entryId,
        year: parseInt(this._editingYear, 10),
      });
      this._editingYear = null;
      await this._fetchData();
    } catch (e: any) {
      this._error = e.message || "Failed to delete";
    }
    this._saving = false;
  }
}
