import { css } from "lit";

export const panelStyles = css`
  :host {
    display: block;
    padding: 16px;
    --mdc-theme-primary: var(--primary-color);
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .header h1 {
    margin: 0;
    font-size: 1.5em;
    font-weight: 400;
    color: var(--primary-text-color);
  }

  .tabs {
    display: flex;
    border-bottom: 1px solid var(--divider-color);
    margin-bottom: 16px;
  }

  .tab {
    padding: 12px 20px;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    color: var(--secondary-text-color);
    font-size: 0.95em;
    font-weight: 500;
    transition: color 0.2s, border-color 0.2s;
    background: none;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .tab:hover {
    color: var(--primary-text-color);
  }

  .tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
  }
`;

export const cardStyles = css`
  .card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
    margin-bottom: 16px;
  }

  .card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1em;
    font-weight: 500;
    color: var(--primary-text-color);
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 12px;
  }

  .stat-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }

  .stat-label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }

  .stat-value {
    font-size: 1.3em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
`;

export const tableStyles = css`
  .table-controls {
    display: flex;
    gap: 12px;
    align-items: flex-end;
    flex-wrap: wrap;
    margin-bottom: 16px;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .input-group label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
  }

  .input-group input {
    padding: 8px 12px;
    border: 1px solid var(--divider-color);
    border-radius: 6px;
    background: var(--card-background-color, white);
    color: var(--primary-text-color);
    font-size: 0.9em;
  }

  button.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.9em;
    font-weight: 500;
  }

  button.btn:hover {
    opacity: 0.9;
  }

  button.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }

  th {
    text-align: left;
    padding: 8px 6px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
    white-space: nowrap;
  }

  td {
    padding: 6px;
    border-bottom: 1px solid var(--divider-color);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-top: 12px;
    font-size: 0.9em;
    color: var(--secondary-text-color);
  }

  .summary-row {
    background: var(--primary-background-color);
    font-weight: 600;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`;

export const faktaStyles = css`
  .period-nav {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
    flex-wrap: wrap;
  }

  .period-tabs {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
  }

  .period-tab {
    padding: 6px 14px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.85em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .period-tab:hover {
    color: var(--primary-text-color);
  }

  .period-tab.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .nav-arrow {
    padding: 4px 10px;
    border: 1px solid var(--divider-color);
    background: var(--ha-card-background, var(--card-background-color, white));
    color: var(--primary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 1em;
    line-height: 1;
  }

  .nav-arrow:hover {
    background: var(--primary-background-color);
  }

  .period-label {
    font-weight: 600;
    font-size: 0.9em;
    color: var(--primary-text-color);
    min-width: 120px;
    text-align: center;
  }

  .fakta-columns {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 16px;
    align-items: start;
  }

  @media (max-width: 900px) {
    .fakta-columns {
      grid-template-columns: 1fr;
    }
  }

  .fakta-card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
  }

  .fakta-card h3 {
    margin: 0 0 12px 0;
    font-size: 1em;
    font-weight: 600;
    color: var(--primary-text-color);
  }

  .fakta-row {
    display: flex;
    justify-content: space-between;
    padding: 4px 0;
    font-size: 0.85em;
  }

  .fakta-row .label {
    color: var(--secondary-text-color);
  }

  .fakta-row .value {
    font-weight: 500;
    color: var(--primary-text-color);
    font-variant-numeric: tabular-nums;
  }

  .fakta-separator {
    border: none;
    border-top: 1px solid var(--divider-color);
    margin: 6px 0;
  }

  .fakta-summary {
    font-weight: 600;
    font-size: 0.9em;
  }

  .fakta-summary .value {
    font-weight: 700;
  }

  .sim-section {
    margin-bottom: 16px;
  }

  .sim-toggle-group {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
    margin-bottom: 10px;
  }

  .sim-toggle {
    flex: 1;
    padding: 6px 10px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.8em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .sim-toggle.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .sim-slider-row {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 10px;
  }

  .sim-slider-row input[type="range"] {
    flex: 1;
  }

  .sim-slider-row .slider-value {
    min-width: 50px;
    text-align: right;
    font-size: 0.85em;
    font-weight: 500;
  }

  .sim-checkbox {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-bottom: 10px;
    font-size: 0.85em;
    color: var(--secondary-text-color);
  }

  .sim-checkbox input {
    margin: 0;
  }

  button.sim-btn {
    width: 100%;
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.85em;
    font-weight: 500;
    margin-bottom: 16px;
  }

  button.sim-btn:hover {
    opacity: 0.9;
  }

  button.sim-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`;
