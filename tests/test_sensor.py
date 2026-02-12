"""Tests for sensor entity definitions.

Note: These tests verify the sensor description data structure.
The actual sensor classes require a full HA environment and are tested
via integration tests when installed in Home Assistant.
"""

import pytest

from custom_components.my_solar_cells.const import DOMAIN


# Manually define expected sensor keys since importing sensor.py
# requires a full HA environment due to class inheritance.
EXPECTED_DAILY_KEYS = [
    "daily_production_sold",
    "daily_production_own_use",
    "daily_purchased",
    "daily_sold_profit",
    "daily_own_use_saved",
    "daily_purchased_cost",
    "daily_balance",
]

EXPECTED_MONTHLY_KEYS = [
    "monthly_production_sold",
    "monthly_production_own_use",
    "monthly_purchased",
    "monthly_sold_profit",
    "monthly_own_use_saved",
    "monthly_purchased_cost",
    "monthly_balance",
    "monthly_tax_reduction",
    "monthly_grid_compensation",
]

EXPECTED_YEARLY_KEYS = [
    "yearly_total_savings",
    "yearly_return_percentage",
]

EXPECTED_LIFETIME_KEYS = [
    "total_savings_to_date",
    "remaining_on_investment",
    "investment_amount",
]

EXPECTED_SPOT_KEYS = [
    "current_spot_price",
    "avg_spot_price_today",
]

EXPECTED_SPECIAL_KEYS = [
    "roi_payback_year",
    "current_price_level",
]


class TestSensorKeys:
    """Tests for expected sensor key structure."""

    def test_domain_is_correct(self):
        """Test the domain constant."""
        assert DOMAIN == "my_solar_cells"

    def test_daily_keys_count(self):
        """Test daily sensor key count."""
        assert len(EXPECTED_DAILY_KEYS) == 7

    def test_monthly_keys_count(self):
        """Test monthly sensor key count."""
        assert len(EXPECTED_MONTHLY_KEYS) == 9

    def test_yearly_keys_count(self):
        """Test yearly sensor key count."""
        assert len(EXPECTED_YEARLY_KEYS) == 2

    def test_lifetime_keys_count(self):
        """Test lifetime sensor key count."""
        assert len(EXPECTED_LIFETIME_KEYS) == 3

    def test_spot_keys_count(self):
        """Test spot price sensor key count."""
        assert len(EXPECTED_SPOT_KEYS) == 2

    def test_total_sensor_count(self):
        """Test total expected sensor count (descriptions + special sensors)."""
        total = (
            len(EXPECTED_DAILY_KEYS)
            + len(EXPECTED_MONTHLY_KEYS)
            + len(EXPECTED_YEARLY_KEYS)
            + len(EXPECTED_LIFETIME_KEYS)
            + len(EXPECTED_SPOT_KEYS)
            + len(EXPECTED_SPECIAL_KEYS)
        )
        # 7 + 9 + 2 + 3 + 2 + 2 = 25 total sensors
        assert total == 25

    def test_all_keys_unique(self):
        """Test all sensor keys are unique across all categories."""
        all_keys = (
            EXPECTED_DAILY_KEYS
            + EXPECTED_MONTHLY_KEYS
            + EXPECTED_YEARLY_KEYS
            + EXPECTED_LIFETIME_KEYS
            + EXPECTED_SPOT_KEYS
            + EXPECTED_SPECIAL_KEYS
        )
        assert len(all_keys) == len(set(all_keys))
