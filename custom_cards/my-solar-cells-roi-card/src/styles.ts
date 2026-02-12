import { css } from "lit";

export const cardStyles = css`
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

  .chart-container {
    position: relative;
    width: 100%;
    height: 300px;
    margin-bottom: 16px;
  }

  .chart-container canvas {
    width: 100% !important;
    height: 100% !important;
  }

  .chart-bar-area {
    display: flex;
    align-items: flex-end;
    gap: 2px;
    height: 250px;
    padding: 0 4px;
    border-bottom: 1px solid var(--divider-color);
  }

  .chart-bar {
    flex: 1;
    min-width: 8px;
    border-radius: 2px 2px 0 0;
    position: relative;
    cursor: pointer;
    transition: opacity 0.2s;
  }

  .chart-bar:hover {
    opacity: 0.8;
  }

  .chart-bar.before-roi {
    background: var(--secondary-text-color);
    opacity: 0.5;
  }

  .chart-bar.after-roi {
    background: var(--success-color, #4caf50);
  }

  .chart-bar.roi-year {
    background: var(--accent-color, #03a9f4);
  }

  .chart-labels {
    display: flex;
    gap: 2px;
    padding: 4px;
  }

  .chart-labels span {
    flex: 1;
    text-align: center;
    font-size: 0.6em;
    color: var(--secondary-text-color);
  }

  .investment-line {
    position: absolute;
    left: 0;
    right: 0;
    border-top: 2px dashed var(--error-color, #f44336);
    pointer-events: none;
  }

  .investment-line-label {
    position: absolute;
    right: 4px;
    top: -16px;
    font-size: 0.7em;
    color: var(--error-color, #f44336);
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
