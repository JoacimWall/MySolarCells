"""Battery simulation engine - ported from HistoryDataService.cs lines 208-267."""

from __future__ import annotations

import logging
from copy import deepcopy

_LOGGER = logging.getLogger(__name__)


def simulate_battery_add(
    hourly_records: list[dict],
    max_battery_kwh: float,
) -> list[dict]:
    """Simulate adding a battery to the system.

    Iterates chronologically, charging from excess production_sold
    and discharging to offset purchased.  All-or-nothing logic matches
    .NET HistoryDataService.cs lines 214-250.
    """
    records = deepcopy(hourly_records)
    current_charge = 0.0

    for rec in records:
        sold = rec.get("production_sold", 0.0)
        purchased = rec.get("purchased", 0.0)
        unit_price_sold = rec.get("unit_price_sold", 0.0)
        unit_price_buy = rec.get("unit_price_buy", 0.0)

        # Charge: if there is excess production and battery has room
        if sold > 0 and (current_charge + sold) <= max_battery_kwh:
            current_charge += sold
            rec["battery_charge"] = rec.get("battery_charge", 0.0) + sold
            rec["production_sold"] = rec.get("production_sold", 0.0) - sold
            rec["production_sold_profit"] = rec.get("production_sold_profit", 0.0) - round(
                sold * unit_price_sold, 4
            )
        # Discharge: if there is demand and battery has enough
        elif purchased > 0 and current_charge > purchased:
            current_charge -= purchased
            rec["battery_used"] = rec.get("battery_used", 0.0) + purchased
            rec["battery_used_profit"] = rec.get("battery_used_profit", 0.0) + round(
                purchased * unit_price_buy, 4
            )
            rec["purchased"] = 0.0
            rec["purchased_cost"] = 0.0

    return records


def simulate_battery_remove(hourly_records: list[dict]) -> list[dict]:
    """Simulate removing an existing battery.

    Converts battery_charge back to production_sold and battery_used
    back to purchased, recalculating financial values using spot prices.
    """
    records = deepcopy(hourly_records)

    for rec in records:
        charge = rec.get("battery_charge", 0.0)
        used = rec.get("battery_used", 0.0)
        unit_price_sold = rec.get("unit_price_sold", 0.0)
        unit_price_buy = rec.get("unit_price_buy", 0.0)

        if charge > 0:
            rec["production_sold"] = rec.get("production_sold", 0.0) + charge
            rec["production_sold_profit"] = rec.get("production_sold_profit", 0.0) + round(
                charge * unit_price_sold, 4
            )
            rec["battery_charge"] = 0.0

        if used > 0:
            rec["purchased"] = rec.get("purchased", 0.0) + used
            rec["purchased_cost"] = rec.get("purchased_cost", 0.0) + round(
                used * unit_price_buy, 4
            )
            rec["battery_used"] = 0.0
            rec["battery_used_profit"] = 0.0

    return records
