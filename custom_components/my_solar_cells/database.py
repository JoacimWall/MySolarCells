"""SQLite database manager for My Solar Cells."""

from __future__ import annotations

import json
import logging
import os
import sqlite3
from typing import Any

from homeassistant.core import HomeAssistant

_LOGGER = logging.getLogger(__name__)

_SCHEMA_SQL = """
CREATE TABLE IF NOT EXISTS hourly_energy (
    timestamp TEXT PRIMARY KEY,
    purchased REAL DEFAULT 0,
    purchased_cost REAL DEFAULT 0,
    production_sold REAL DEFAULT 0,
    production_sold_profit REAL DEFAULT 0,
    unit_price_buy REAL DEFAULT 0,
    unit_price_vat_buy REAL DEFAULT 0,
    unit_price_sold REAL DEFAULT 0,
    unit_price_vat_sold REAL DEFAULT 0,
    production_own_use REAL DEFAULT 0,
    production_own_use_profit REAL DEFAULT 0,
    battery_charge REAL DEFAULT 0,
    battery_used REAL DEFAULT 0,
    battery_used_profit REAL DEFAULT 0,
    synced INTEGER DEFAULT 0,
    sensor_enriched INTEGER DEFAULT 0,
    price_level TEXT DEFAULT ''
);

CREATE TABLE IF NOT EXISTS spot_prices (
    timestamp TEXT PRIMARY KEY,
    total REAL DEFAULT 0,
    energy REAL DEFAULT 0,
    tax REAL DEFAULT 0,
    level TEXT DEFAULT '',
    currency TEXT DEFAULT 'SEK'
);

CREATE TABLE IF NOT EXISTS yearly_params (
    year INTEGER PRIMARY KEY,
    tax_reduction REAL,
    grid_compensation REAL,
    transfer_fee REAL,
    energy_tax REAL,
    installed_kw REAL
);

CREATE TABLE IF NOT EXISTS metadata (
    key TEXT PRIMARY KEY,
    value TEXT
);
"""

# Column names for hourly_energy table (order must match INSERT statement)
_HOURLY_COLS = [
    "timestamp", "purchased", "purchased_cost", "production_sold",
    "production_sold_profit", "unit_price_buy", "unit_price_vat_buy",
    "unit_price_sold", "unit_price_vat_sold", "production_own_use",
    "production_own_use_profit", "battery_charge", "battery_used",
    "battery_used_profit", "synced", "sensor_enriched", "price_level",
]

_SPOT_COLS = ["timestamp", "total", "energy", "tax", "level", "currency"]


class MySolarCellsDatabase:
    """SQLite-backed storage replacing the old JSON Store."""

    def __init__(self, hass: HomeAssistant, entry_id: str) -> None:
        self._hass = hass
        self._entry_id = entry_id
        self._db_path: str = hass.config.path(f".storage/my_solar_cells_{entry_id}.db")
        self._conn: sqlite3.Connection | None = None

    # ------------------------------------------------------------------
    # Lifecycle
    # ------------------------------------------------------------------

    async def async_setup(self) -> None:
        """Create connection and tables."""
        await self._hass.async_add_executor_job(self._setup_sync)

    def _setup_sync(self) -> None:
        os.makedirs(os.path.dirname(self._db_path), exist_ok=True)
        self._conn = sqlite3.connect(self._db_path, check_same_thread=False)
        self._conn.row_factory = sqlite3.Row
        self._conn.executescript(_SCHEMA_SQL)
        self._conn.commit()

    async def async_close(self) -> None:
        """Close the database connection."""
        await self._hass.async_add_executor_job(self._close_sync)

    def _close_sync(self) -> None:
        if self._conn:
            self._conn.close()
            self._conn = None

    # ------------------------------------------------------------------
    # Hourly energy records
    # ------------------------------------------------------------------

    def upsert_hourly_record(self, timestamp: str, record: dict) -> None:
        """Insert or replace an hourly energy record."""
        assert self._conn is not None
        values = (
            timestamp,
            record.get("purchased", 0),
            record.get("purchased_cost", 0),
            record.get("production_sold", 0),
            record.get("production_sold_profit", 0),
            record.get("unit_price_buy", 0),
            record.get("unit_price_vat_buy", 0),
            record.get("unit_price_sold", 0),
            record.get("unit_price_vat_sold", 0),
            record.get("production_own_use", 0),
            record.get("production_own_use_profit", 0),
            record.get("battery_charge", 0),
            record.get("battery_used", 0),
            record.get("battery_used_profit", 0),
            1 if record.get("synced") else 0,
            1 if record.get("sensor_enriched") else 0,
            record.get("price_level", ""),
        )
        placeholders = ",".join("?" * len(_HOURLY_COLS))
        self._conn.execute(
            f"INSERT OR REPLACE INTO hourly_energy ({','.join(_HOURLY_COLS)}) "
            f"VALUES ({placeholders})",
            values,
        )

    def get_hourly_record(self, timestamp: str) -> dict | None:
        """Get a single hourly record by timestamp."""
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT * FROM hourly_energy WHERE timestamp = ?", (timestamp,)
        )
        row = cur.fetchone()
        return dict(row) if row else None

    def get_records_for_period(self, start_iso: str, end_iso: str) -> list[dict]:
        """Get hourly records in [start_iso, end_iso)."""
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT * FROM hourly_energy WHERE timestamp >= ? AND timestamp < ? "
            "ORDER BY timestamp",
            (start_iso, end_iso),
        )
        return [dict(row) for row in cur.fetchall()]

    def get_all_records_as_list(self) -> list[dict]:
        """Get all hourly records sorted by timestamp."""
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT * FROM hourly_energy ORDER BY timestamp"
        )
        return [dict(row) for row in cur.fetchall()]

    # ------------------------------------------------------------------
    # Spot prices
    # ------------------------------------------------------------------

    def upsert_quarter_hourly_price(self, timestamp: str, price: dict) -> None:
        """Insert or replace a quarter-hourly spot price."""
        assert self._conn is not None
        values = (
            timestamp,
            price.get("total", 0),
            price.get("energy", 0),
            price.get("tax", 0),
            price.get("level", ""),
            price.get("currency", "SEK"),
        )
        placeholders = ",".join("?" * len(_SPOT_COLS))
        self._conn.execute(
            f"INSERT OR REPLACE INTO spot_prices ({','.join(_SPOT_COLS)}) "
            f"VALUES ({placeholders})",
            values,
        )

    def get_prices_for_date(self, date_iso: str) -> list[dict]:
        """Get all quarter-hourly prices for a date (YYYY-MM-DD)."""
        assert self._conn is not None
        prefix = f"{date_iso}%"
        cur = self._conn.execute(
            "SELECT * FROM spot_prices WHERE timestamp LIKE ? ORDER BY timestamp",
            (prefix,),
        )
        rows = cur.fetchall()
        result = []
        for row in rows:
            d = dict(row)
            d["starts_at"] = d.pop("timestamp")
            result.append(d)
        return result

    def get_average_price_for_hour(self, hour_iso: str) -> float | None:
        """Get average of quarter-hourly prices for an hour.

        hour_iso should be like "2025-06-15T14" (first 13 chars).
        """
        assert self._conn is not None
        prefix = f"{hour_iso[:13]}%"
        cur = self._conn.execute(
            "SELECT AVG(total) as avg_total FROM spot_prices WHERE timestamp LIKE ?",
            (prefix,),
        )
        row = cur.fetchone()
        if row and row["avg_total"] is not None:
            return row["avg_total"]
        return None

    # ------------------------------------------------------------------
    # Metadata (key-value store)
    # ------------------------------------------------------------------

    def _get_meta(self, key: str) -> str | None:
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT value FROM metadata WHERE key = ?", (key,)
        )
        row = cur.fetchone()
        return row["value"] if row else None

    def _set_meta(self, key: str, value: str | None) -> None:
        assert self._conn is not None
        if value is None:
            self._conn.execute("DELETE FROM metadata WHERE key = ?", (key,))
        else:
            self._conn.execute(
                "INSERT OR REPLACE INTO metadata (key, value) VALUES (?, ?)",
                (key, value),
            )

    @property
    def last_tibber_sync(self) -> str | None:
        return self._get_meta("last_tibber_sync")

    @last_tibber_sync.setter
    def last_tibber_sync(self, value: str | None) -> None:
        self._set_meta("last_tibber_sync", value)

    @property
    def last_sensor_readings(self) -> dict[str, float]:
        raw = self._get_meta("last_sensor_readings")
        if raw:
            try:
                return json.loads(raw)
            except (json.JSONDecodeError, TypeError):
                pass
        return {}

    @last_sensor_readings.setter
    def last_sensor_readings(self, value: dict[str, float]) -> None:
        self._set_meta("last_sensor_readings", json.dumps(value))

    @property
    def roi_projection(self) -> list[dict]:
        raw = self._get_meta("roi_projection")
        if raw:
            try:
                return json.loads(raw)
            except (json.JSONDecodeError, TypeError):
                pass
        return []

    @roi_projection.setter
    def roi_projection(self, value: list[dict]) -> None:
        self._set_meta("roi_projection", json.dumps(value))

    @property
    def monthly_cache(self) -> dict:
        raw = self._get_meta("monthly_cache")
        if raw:
            try:
                return json.loads(raw)
            except (json.JSONDecodeError, TypeError):
                pass
        return {}

    def update_monthly_cache(self, month_key: str, stats: dict) -> None:
        """Update a single month in the monthly cache."""
        cache = self.monthly_cache
        cache[month_key] = stats
        self._set_meta("monthly_cache", json.dumps(cache))

    # ------------------------------------------------------------------
    # Yearly financial parameters
    # ------------------------------------------------------------------

    _YEARLY_PARAM_COLS = [
        "tax_reduction", "grid_compensation", "transfer_fee", "energy_tax",
        "installed_kw",
    ]

    def set_yearly_params(self, year: int, params: dict) -> None:
        """Insert or replace financial parameters for a specific year."""
        assert self._conn is not None
        self._conn.execute(
            "INSERT OR REPLACE INTO yearly_params "
            "(year, tax_reduction, grid_compensation, transfer_fee, energy_tax, "
            "installed_kw) "
            "VALUES (?, ?, ?, ?, ?, ?)",
            (
                year,
                params.get("tax_reduction"),
                params.get("grid_compensation"),
                params.get("transfer_fee"),
                params.get("energy_tax"),
                params.get("installed_kw"),
            ),
        )

    def get_yearly_params(self, year: int) -> dict | None:
        """Get financial parameters for a specific year, or None if not set."""
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT * FROM yearly_params WHERE year = ?", (year,)
        )
        row = cur.fetchone()
        if not row:
            return None
        result = {}
        for col in self._YEARLY_PARAM_COLS:
            val = row[col]
            if val is not None:
                result[col] = val
        return result if result else None

    def get_all_yearly_params(self) -> dict[str, dict]:
        """Get all yearly params as dict keyed by year string.

        Compatible with the format expected by get_calc_params_for_year().
        """
        assert self._conn is not None
        cur = self._conn.execute("SELECT * FROM yearly_params ORDER BY year")
        result: dict[str, dict] = {}
        for row in cur.fetchall():
            params = {}
            for col in self._YEARLY_PARAM_COLS:
                val = row[col]
                if val is not None:
                    params[col] = val
            if params:
                result[str(row["year"])] = params
        return result

    def delete_yearly_params(self, year: int) -> None:
        """Delete financial parameters for a specific year."""
        assert self._conn is not None
        self._conn.execute("DELETE FROM yearly_params WHERE year = ?", (year,))

    # ------------------------------------------------------------------
    # Panel query helpers
    # ------------------------------------------------------------------

    def get_hourly_record_count(self) -> int:
        """Get total number of hourly energy records."""
        assert self._conn is not None
        cur = self._conn.execute("SELECT COUNT(*) as cnt FROM hourly_energy")
        return cur.fetchone()["cnt"]

    def get_spot_price_count(self) -> int:
        """Get total number of spot price records."""
        assert self._conn is not None
        cur = self._conn.execute("SELECT COUNT(*) as cnt FROM spot_prices")
        return cur.fetchone()["cnt"]

    def get_first_hourly_timestamp(self) -> str | None:
        """Get the earliest hourly energy timestamp."""
        assert self._conn is not None
        cur = self._conn.execute("SELECT MIN(timestamp) as ts FROM hourly_energy")
        row = cur.fetchone()
        return row["ts"] if row else None

    def get_last_hourly_timestamp(self) -> str | None:
        """Get the latest hourly energy timestamp."""
        assert self._conn is not None
        cur = self._conn.execute("SELECT MAX(timestamp) as ts FROM hourly_energy")
        row = cur.fetchone()
        return row["ts"] if row else None

    def get_records_for_period_paginated(
        self, start_iso: str, end_iso: str, offset: int = 0, limit: int = 50
    ) -> tuple[list[dict], int]:
        """Get paginated hourly records in [start_iso, end_iso) with total count."""
        assert self._conn is not None
        cur = self._conn.execute(
            "SELECT COUNT(*) as cnt FROM hourly_energy "
            "WHERE timestamp >= ? AND timestamp < ?",
            (start_iso, end_iso),
        )
        total = cur.fetchone()["cnt"]
        cur = self._conn.execute(
            "SELECT * FROM hourly_energy WHERE timestamp >= ? AND timestamp < ? "
            "ORDER BY timestamp LIMIT ? OFFSET ?",
            (start_iso, end_iso, limit, offset),
        )
        records = [dict(row) for row in cur.fetchall()]
        return records, total

    def get_prices_for_date_raw(self, date_iso: str) -> list[dict]:
        """Get all quarter-hourly prices for a date without column rename."""
        assert self._conn is not None
        prefix = f"{date_iso}%"
        cur = self._conn.execute(
            "SELECT * FROM spot_prices WHERE timestamp LIKE ? ORDER BY timestamp",
            (prefix,),
        )
        return [dict(row) for row in cur.fetchall()]

    # ------------------------------------------------------------------
    # Save / Remove
    # ------------------------------------------------------------------

    async def async_save(self) -> None:
        """Commit pending changes."""
        await self._hass.async_add_executor_job(self._save_sync)

    def _save_sync(self) -> None:
        if self._conn:
            self._conn.commit()

    async def async_remove(self) -> None:
        """Close connection and delete the database file."""
        await self._hass.async_add_executor_job(self._remove_sync)

    def _remove_sync(self) -> None:
        if self._conn:
            self._conn.close()
            self._conn = None
        if os.path.exists(self._db_path):
            os.remove(self._db_path)
            _LOGGER.info("Removed database file: %s", self._db_path)
