# My Solar Cells - Home Assistant Integration

[![hacs_badge](https://img.shields.io/badge/HACS-Custom-41BDF5.svg)](https://github.com/hacs/integration)

A Home Assistant custom integration that tracks solar energy production economics, fetches electricity prices from Tibber, calculates financial metrics (earnings from selling, savings from own use), and projects ROI over 30 years.

## Features

- **Tibber Integration**: Fetches spot prices at 15-minute resolution and consumption/production data
- **Financial Tracking**: Calculates daily, monthly, and yearly profit/savings from solar production
- **ROI Projection**: 30-year investment return projection with panel degradation and price development
- **Swedish Tax Rules**: Handles tax reduction, grid compensation, transfer fees, and energy tax
- **Custom Lovelace Card**: Visual ROI chart with payback year indicator

## Installation

### HACS (Recommended)

1. Open HACS in Home Assistant
2. Click the three dots menu -> Custom repositories
3. Add `https://github.com/joacimwall/ha-my-solar-cells` as an Integration
4. Search for "My Solar Cells" and install
5. Restart Home Assistant

### Manual

1. Copy the `custom_components/my_solar_cells` folder to your `config/custom_components/` directory
2. Restart Home Assistant

## Configuration

The integration uses a 5-step config flow:

### Step 1: Tibber API Key
Enter your Tibber API key from [developer.tibber.com](https://developer.tibber.com/settings/access-token)

### Step 2: Select Home
Choose which Tibber home to use. Optionally import only spot prices (if you use HA sensors for consumption/production data).

### Step 3: HA Sensors
Select the Home Assistant sensors that track your solar energy:
- **Grid Export Sensor**: kWh sold to the grid
- **Grid Import Sensor**: kWh purchased from the grid
- **Production Sensor**: Total kWh solar production
- **Battery Charge/Discharge Sensors** (optional)

### Step 4: Financial Parameters
Configure Swedish energy financial parameters:
| Parameter | Default | Description |
|-----------|---------|-------------|
| Tax Reduction | 0.60 SEK/kWh | Skattereduktion |
| Grid Compensation | 0.078 SEK/kWh | Natnytta |
| Transfer Fee | 0.30 SEK/kWh | Overforingsavgift |
| Energy Tax | 0.49 SEK/kWh | Energiskatt |
| Installed Capacity | 10.5 kW | Panel capacity |

### Step 5: Investment & ROI
Enter your solar investment details:
- Investment amount and loan details
- Panel degradation rate (default 0.25%/year)
- Electricity price development (default 1.05%/year)
- Installation date

## Sensors

### Daily (reset each day)
- `sensor.my_solar_cells_daily_production_sold` - kWh sold to grid
- `sensor.my_solar_cells_daily_production_own_use` - kWh used directly
- `sensor.my_solar_cells_daily_purchased` - kWh purchased
- `sensor.my_solar_cells_daily_sold_profit` - SEK earned from selling
- `sensor.my_solar_cells_daily_own_use_saved` - SEK saved from own use
- `sensor.my_solar_cells_daily_purchased_cost` - SEK cost of purchased
- `sensor.my_solar_cells_daily_balance` - SEK net balance

### Monthly
Same fields as daily plus:
- `sensor.my_solar_cells_monthly_tax_reduction` - SEK tax reduction
- `sensor.my_solar_cells_monthly_grid_compensation` - SEK grid compensation

### Yearly
- `sensor.my_solar_cells_yearly_total_savings` - SEK total savings this year
- `sensor.my_solar_cells_yearly_return_percentage` - % return on investment

### Lifetime / ROI
- `sensor.my_solar_cells_total_savings_to_date` - SEK lifetime savings
- `sensor.my_solar_cells_remaining_on_investment` - SEK remaining
- `sensor.my_solar_cells_roi_payback_year` - Projected payback year (with 30-year projection as attribute)
- `sensor.my_solar_cells_investment_amount` - Total investment

### Spot Prices
- `sensor.my_solar_cells_current_spot_price` - Current 15-min spot price (SEK/kWh)
- `sensor.my_solar_cells_current_price_level` - CHEAP/NORMAL/EXPENSIVE
- `sensor.my_solar_cells_avg_spot_price_today` - Today's average price

## Custom Lovelace Card

Add the ROI projection card to your dashboard:

```yaml
type: custom:my-solar-cells-roi-card
entity: sensor.my_solar_cells_roi_payback_year
show_chart: true
show_table: false
title: Solar ROI Projection
```

## How It Works

1. **Every 15 minutes**: Fetches current spot prices from Tibber (96 prices/day at quarter-hourly resolution)
2. **Every hour**: Fetches consumption/production data from Tibber, enriches with HA sensor data
3. **On each update**: Recalculates daily/monthly/yearly financial aggregations and ROI projection
4. **First run**: Performs historical import from your installation date

The financial calculations match the formulas from the MySolarCells mobile app:
- Sold profit = spot price per hour * kWh sold + grid compensation + tax reduction
- Own use savings = spot price * kWh used + transfer fee saved + energy tax saved
- Tax reduction capped when yearly production sold > purchased

## Development

```bash
# Run tests
cd ha-my-solar-cells
pip install pytest pytest-asyncio aiohttp
pytest tests/

# Build the Lovelace card
cd custom_cards/my-solar-cells-roi-card
npm install
npm run build
```

## License

MIT License - see [LICENSE](LICENSE)
