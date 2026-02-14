"""Constants for the My Solar Cells integration."""

DOMAIN = "my_solar_cells"
PLATFORMS = ["sensor"]

# Tibber API
TIBBER_API_URL = "https://api.tibber.com/v1-beta/gql"

# Update intervals (seconds)
UPDATE_INTERVAL_MINUTES = 15
HOURLY_UPDATE_INTERVAL_MINUTES = 60

# --- Defaults from EnergyCalculationParameter.cs ---
DEFAULT_TAX_REDUCTION = 0.60  # SEK/kWh sold, max 18000 SEK/year
DEFAULT_GRID_COMPENSATION = 0.078  # SEK/kWh (nätnytta)
DEFAULT_TRANSFER_FEE = 0.30  # SEK/kWh (överföringsavgift)
DEFAULT_ENERGY_TAX = 0.49  # SEK/kWh (energiskatt)
DEFAULT_INSTALLED_KW = 10.5  # kW installed panel capacity

# --- Defaults from SavingEstimateParameters.cs ---
DEFAULT_PRICE_DEVELOPMENT = 1.05  # % annual electricity price increase
DEFAULT_PANEL_DEGRADATION = 0.25  # % annual panel degradation

# --- Investment defaults ---
DEFAULT_INVESTMENT_AMOUNT = 0
DEFAULT_LOAN_AMOUNT = 0
DEFAULT_INTEREST_RATE = 0.0
DEFAULT_AMORTIZATION_MONTHLY = 0

# --- Config entry keys ---
CONF_API_KEY = "api_key"
CONF_HOME_ID = "home_id"
CONF_HOME_NAME = "home_name"
CONF_IMPORT_ONLY_SPOT_PRICE = "import_only_spot_price"
CONF_GRID_EXPORT_SENSOR = "grid_export_sensor"
CONF_GRID_IMPORT_SENSOR = "grid_import_sensor"
CONF_PRODUCTION_SENSOR = "production_sensor"
CONF_BATTERY_CHARGE_SENSOR = "battery_charge_sensor"
CONF_BATTERY_DISCHARGE_SENSOR = "battery_discharge_sensor"
CONF_TAX_REDUCTION = "tax_reduction"
CONF_GRID_COMPENSATION = "grid_compensation"
CONF_TRANSFER_FEE = "transfer_fee"
CONF_ENERGY_TAX = "energy_tax"
CONF_INSTALLED_KW = "installed_kw"
CONF_INVESTMENT_AMOUNT = "investment_amount"
CONF_LOAN_AMOUNT = "loan_amount"
CONF_INTEREST_RATE = "interest_rate"
CONF_AMORTIZATION_MONTHLY = "amortization_monthly"
CONF_PANEL_DEGRADATION = "panel_degradation"
CONF_PRICE_DEVELOPMENT = "price_development"
CONF_INSTALLATION_DATE = "installation_date"
CONF_YEARLY_PARAMS = "yearly_params"
CONF_TIBBER_START_YEAR = "tibber_start_year"

# Year when tax reduction is removed in Sweden
TAX_REDUCTION_END_YEAR = 2026

# ROI projection years
ROI_PROJECTION_YEARS = 30

# Average monthly production per installed kW (Swedish climate)
# From AverageProductionMonth.GetSnitMonth() in RoiService.cs
AVERAGE_PRODUCTION_PER_KW = {
    1: 13.9,
    2: 32.3,
    3: 85.05,
    4: 120.75,
    5: 137.95,
    6: 136.55,
    7: 134.9,
    8: 115.65,
    9: 89.9,
    10: 54.05,
    11: 19.6,
    12: 9.6,
}

# Storage keys
STORAGE_VERSION = 1
STORAGE_KEY_PREFIX = "my_solar_cells"
