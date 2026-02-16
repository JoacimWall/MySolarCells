# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Home Assistant custom integration ("My Solar Cells") that tracks solar energy production economics using the Tibber API, calculates financial metrics, and projects ROI over 30 years. Includes a custom Lovelace card for ROI visualization. Domain: `my_solar_cells`.

The Python financial/ROI engines are ported from a .NET app — `financial_engine.py` from `HistoryDataService.cs`, `roi_engine.py` from `RoiService.cs`, `tibber_client.py` from `TibberService.cs`. Preserve calculation parity with the original .NET logic.

## Commands

### Python tests
```bash
pip install pytest pytest-asyncio aiohttp  # one-time setup
pytest tests/                               # run all tests
pytest tests/test_financial_engine.py       # run single test file
pytest tests/test_financial_engine.py::test_function_name -v  # run single test
```

Tests mock all Home Assistant modules in `conftest.py` (no full HA install needed).

### Lovelace card (TypeScript)
```bash
cd custom_cards/my-solar-cells-roi-card
npm install         # one-time setup
npm run build       # build with rollup → my-solar-cells-roi-card.js
npm run watch       # dev mode with file watching
```

The compiled JS is copied to `custom_components/my_solar_cells/my-solar-cells-roi-card.js` for HA to serve.

## Architecture

### Data Flow
Tibber API → `tibber_client.py` → `storage.py` (JSON persistence) → `coordinator.py` (orchestration) → `financial_engine.py` + `roi_engine.py` (calculations) → `sensor.py` (HA entities)

### Key Modules (in `custom_components/my_solar_cells/`)
- **coordinator.py**: `DataUpdateCoordinator` subclass. Fetches spot prices every 15 min, consumption/production every 60 min. Performs historical import on first run. Enriches Tibber data with HA sensor values.
- **financial_engine.py**: Pure calculation module. `CalcParams` dataclass holds config. `calculate_daily_stats()` processes hourly records into daily aggregates. `generate_monthly_report()` creates period summaries. Tax reduction is capped when yearly production sold > purchased.
- **roi_engine.py**: 30-year projection with panel degradation (default 0.25%/yr) and price development (default 5%/yr). Handles incomplete current year by extrapolating. Tax reduction set to end in 2026.
- **tibber_client.py**: Async GraphQL client using aiohttp. Queries: test connection, get homes, consumption/production (HOURLY), spot prices (QUARTER_HOURLY).
- **config_flow.py**: 5-step setup wizard — Tibber key → home selection → HA sensors → financial params → investment/ROI.
- **storage.py**: JSON persistence via HA's `Store`. Hourly records keyed by UTC timestamp. Version 1 migration normalizes timezone offset keys.
- **sensor.py**: 30+ sensors grouped by period (daily/monthly/yearly/lifetime/spot price). Uses `SensorEntity` with proper `device_class` and `state_class`.

### Lovelace Card (`custom_cards/my-solar-cells-roi-card/`)
Built with Lit (web components) + TypeScript. Rollup bundles to single JS file. Displays ROI projection chart from the `roi_payback_year` sensor's attributes.

## Swedish Financial Model

All monetary values are in SEK. Key parameters with defaults:
- Tax reduction (skattereduktion): 0.60 SEK/kWh
- Grid compensation (nätnytta): 0.078 SEK/kWh
- Transfer fee (överföringsavgift): 0.30 SEK/kWh
- Energy tax (energiskatt): 0.49 SEK/kWh

## Code Conventions

- `from __future__ import annotations` in all Python files
- Type hints throughout, dataclasses for config/data structures
- Module-level `_LOGGER = logging.getLogger(__name__)`
- Async/await with `_async_` prefix convention for async methods
- Constants in `const.py` as `UPPER_SNAKE_CASE`
- No linter config — follows Home Assistant style conventions
