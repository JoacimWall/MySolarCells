"""Fixtures for My Solar Cells tests."""

from __future__ import annotations

import sys
from types import ModuleType
from unittest.mock import MagicMock

# Mock homeassistant modules so tests can run without the full HA installation
_HA_MODULES = [
    "homeassistant",
    "homeassistant.core",
    "homeassistant.config_entries",
    "homeassistant.helpers",
    "homeassistant.helpers.storage",
    "homeassistant.helpers.update_coordinator",
    "homeassistant.helpers.aiohttp_client",
    "homeassistant.helpers.device_registry",
    "homeassistant.helpers.entity_platform",
    "homeassistant.helpers.selector",
    "homeassistant.components",
    "homeassistant.components.sensor",
    "homeassistant.components.recorder",
    "homeassistant.components.recorder.models",
    "homeassistant.components.recorder.statistics",
    "homeassistant.helpers.entity_registry",
    "homeassistant.const",
    "voluptuous",
    "aiohttp",
]

for mod_name in _HA_MODULES:
    if mod_name not in sys.modules:
        mock_mod = MagicMock()
        # Ensure voluptuous has expected attributes
        if mod_name == "voluptuous":
            mock_mod.Schema = MagicMock()
            mock_mod.Required = MagicMock()
            mock_mod.Optional = MagicMock()
            mock_mod.Coerce = MagicMock()
            mock_mod.In = MagicMock()
        sys.modules[mod_name] = mock_mod

import pytest


@pytest.fixture
def sample_hourly_records() -> list[dict]:
    """Return sample hourly energy records for testing."""
    return [
        {
            "timestamp": "2025-06-15T06:00:00+02:00",
            "purchased": 1.5,
            "purchased_cost": 0.75,
            "production_sold": 2.0,
            "production_sold_profit": 1.20,
            "production_own_use": 3.0,
            "production_own_use_profit": 1.80,
            "battery_charge": 0.0,
            "battery_used": 0.0,
            "battery_used_profit": 0.0,
            "unit_price_buy": 0.50,
            "unit_price_vat_buy": 0.625,
            "unit_price_sold": 0.60,
            "unit_price_vat_sold": 0.75,
            "price_level": "NORMAL",
            "synced": True,
        },
        {
            "timestamp": "2025-06-15T07:00:00+02:00",
            "purchased": 0.8,
            "purchased_cost": 0.48,
            "production_sold": 3.5,
            "production_sold_profit": 2.10,
            "production_own_use": 4.0,
            "production_own_use_profit": 2.40,
            "battery_charge": 1.0,
            "battery_used": 0.5,
            "battery_used_profit": 0.25,
            "unit_price_buy": 0.60,
            "unit_price_vat_buy": 0.75,
            "unit_price_sold": 0.60,
            "unit_price_vat_sold": 0.75,
            "price_level": "NORMAL",
            "synced": True,
        },
        {
            "timestamp": "2025-06-15T08:00:00+02:00",
            "purchased": 0.5,
            "purchased_cost": 0.20,
            "production_sold": 5.0,
            "production_sold_profit": 3.00,
            "production_own_use": 5.5,
            "production_own_use_profit": 3.30,
            "battery_charge": 2.0,
            "battery_used": 0.0,
            "battery_used_profit": 0.0,
            "unit_price_buy": 0.40,
            "unit_price_vat_buy": 0.50,
            "unit_price_sold": 0.60,
            "unit_price_vat_sold": 0.75,
            "price_level": "CHEAP",
            "synced": True,
        },
    ]


@pytest.fixture
def sample_calc_params():
    """Return sample calculation parameters."""
    from custom_components.my_solar_cells.financial_engine import CalcParams

    return CalcParams(
        tax_reduction=0.60,
        grid_compensation=0.078,
        transfer_fee=0.30,
        energy_tax=0.49,
        installed_kw=10.5,
        use_spot_price=True,
        fixed_price=0.0,
    )


@pytest.fixture
def sample_yearly_records() -> list[dict]:
    """Return a larger sample spanning multiple months for yearly tests."""
    records = []
    # Generate 12 months of synthetic data
    months_data = {
        1: {"prod_sold": 50, "own_use": 30, "purchased": 200},
        2: {"prod_sold": 100, "own_use": 80, "purchased": 180},
        3: {"prod_sold": 300, "own_use": 200, "purchased": 150},
        4: {"prod_sold": 500, "own_use": 350, "purchased": 100},
        5: {"prod_sold": 600, "own_use": 400, "purchased": 80},
        6: {"prod_sold": 650, "own_use": 450, "purchased": 70},
        7: {"prod_sold": 620, "own_use": 430, "purchased": 75},
        8: {"prod_sold": 550, "own_use": 380, "purchased": 90},
        9: {"prod_sold": 400, "own_use": 280, "purchased": 120},
        10: {"prod_sold": 200, "own_use": 150, "purchased": 160},
        11: {"prod_sold": 80, "own_use": 50, "purchased": 190},
        12: {"prod_sold": 40, "own_use": 20, "purchased": 210},
    }

    for month, data in months_data.items():
        # Create one representative daily record per month at hour 12
        for day in range(1, 29):  # 28 days per month for simplicity
            ts = f"2024-{month:02d}-{day:02d}T12:00:00+01:00"
            daily_sold = data["prod_sold"] / 28
            daily_own = data["own_use"] / 28
            daily_purchased = data["purchased"] / 28
            records.append(
                {
                    "timestamp": ts,
                    "purchased": round(daily_purchased, 2),
                    "purchased_cost": round(daily_purchased * 0.50, 2),
                    "production_sold": round(daily_sold, 2),
                    "production_sold_profit": round(daily_sold * 0.60, 2),
                    "production_own_use": round(daily_own, 2),
                    "production_own_use_profit": round(daily_own * 0.50, 2),
                    "battery_charge": 0.0,
                    "battery_used": 0.0,
                    "battery_used_profit": 0.0,
                    "unit_price_buy": 0.50,
                    "unit_price_vat_buy": 0.625,
                    "unit_price_sold": 0.60,
                    "unit_price_vat_sold": 0.75,
                    "price_level": "NORMAL",
                    "synced": True,
                }
            )
    return records
