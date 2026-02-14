"""Historical statistics import for My Solar Cells.

Imports stored Tibber hourly data into HA's recorder/statistics system so that
historical data appears in HA history graphs. Also enriches records with
production_own_use by querying the HA production sensor's recorded statistics.
"""

from __future__ import annotations

import logging
from datetime import datetime, timedelta, timezone
from typing import TYPE_CHECKING, Any

from .const import CONF_PRODUCTION_SENSOR, DOMAIN
from .financial_engine import (
    CalcParams,
    HistoryStats,
    calculate_daily_stats,
    get_calc_params_for_year,
)

if TYPE_CHECKING:
    from .coordinator import MySolarCellsCoordinator

_LOGGER = logging.getLogger(__name__)

# Bump this to force a re-import when import logic changes.
STATISTICS_IMPORT_VERSION = 4

# (sensor_key, unit, HistoryStats attribute name)
SENSORS_TO_IMPORT: list[tuple[str, str, str]] = [
    ("daily_production_sold", "kWh", "production_sold"),
    ("daily_production_own_use", "kWh", "production_own_use"),
    ("daily_purchased", "kWh", "purchased"),
    ("daily_sold_profit", "SEK", "sum_production_sold_profit"),
    ("daily_own_use_saved", "SEK", "sum_production_own_use_saved"),
    ("daily_purchased_cost", "SEK", "sum_purchased_cost"),
    ("daily_balance", "SEK", "balance"),
]


def _parse_to_utc_datetime(ts: str) -> datetime:
    """Parse ISO timestamp string to UTC datetime."""
    dt = datetime.fromisoformat(ts)
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    return dt


def enrich_records_with_production_own_use(
    records: dict[str, dict],
    prod_deltas: dict[str, float],
) -> int:
    """Enrich stored records with production_own_use from production deltas.

    For each record where a production delta exists:
      production_own_use = max(0, prod_delta - production_sold)
      production_own_use_profit = production_own_use * unit_price_buy

    Returns the number of records enriched.
    """
    enriched = 0
    for ts, record in records.items():
        delta = prod_deltas.get(ts)
        if delta is not None:
            production_sold = record.get("production_sold", 0)
            own_use = max(0, delta - production_sold)
            record["production_own_use"] = own_use
            record["production_own_use_profit"] = own_use * record.get("unit_price_buy", 0)
            enriched += 1
    return enriched


def calculate_hourly_financial_stats(
    records: list[dict],
    calc_params: CalcParams,
    yearly_params: dict[str, dict[str, float]] | None,
) -> dict[str, HistoryStats]:
    """Calculate financial stats for each hourly record.

    Uses per-year parameter overrides via get_calc_params_for_year().
    Returns dict keyed by timestamp string -> HistoryStats.
    """
    result: dict[str, HistoryStats] = {}
    for record in records:
        ts = record.get("timestamp", "")
        if not ts:
            continue
        year = ts[:4]
        params = get_calc_params_for_year(year, calc_params, yearly_params)
        stats = calculate_daily_stats([record], params)
        result[ts] = stats
    return result


def build_statistics_list(
    hourly_stats: dict[str, HistoryStats],
    attr_name: str,
) -> list[tuple[datetime, float, float]]:
    """Build sorted list of (start_dt, daily_state, cumulative_sum).

    daily_state resets at day boundaries (midnight UTC).
    cumulative_sum monotonically increases across all hours.
    """
    result: list[tuple[datetime, float, float]] = []
    cumulative_sum = 0.0
    daily_state = 0.0
    current_date: str | None = None

    for ts in sorted(hourly_stats.keys()):
        stats = hourly_stats[ts]
        hourly_value = getattr(stats, attr_name)

        date = ts[:10]
        if date != current_date:
            daily_state = 0.0
            current_date = date

        daily_state += hourly_value
        cumulative_sum += hourly_value

        dt = _parse_to_utc_datetime(ts)
        result.append((dt, daily_state, cumulative_sum))

    return result


async def _fetch_production_deltas(
    hass: Any,
    production_sensor_id: str,
    start_dt: datetime,
    end_dt: datetime,
) -> dict[str, float]:
    """Query HA recorder for production sensor hourly deltas.

    Returns dict of ISO timestamp -> hourly production delta (kWh).
    """
    from homeassistant.components.recorder import get_instance
    from homeassistant.components.recorder.statistics import statistics_during_period

    result = await get_instance(hass).async_add_executor_job(
        statistics_during_period,
        hass,
        start_dt,
        end_dt,
        {production_sensor_id},
        "hour",
        None,
        {"sum"},
    )

    prod_stats = result.get(production_sensor_id, [])
    if not prod_stats:
        return {}

    # Build hourly deltas from cumulative sums
    deltas: dict[str, float] = {}
    prev_sum: float | None = None
    for stat in prod_stats:
        stat_start = stat["start"]
        stat_sum = stat.get("sum")
        if stat_sum is None:
            continue

        if prev_sum is not None:
            delta = max(0, stat_sum - prev_sum)
            if isinstance(stat_start, datetime):
                key = stat_start.strftime("%Y-%m-%dT%H:%M:%S")
            else:
                key = str(stat_start)
            deltas[key] = delta

        prev_sum = stat_sum

    return deltas


async def async_import_historical_statistics(
    hass: Any,
    coordinator: MySolarCellsCoordinator,
) -> bool:
    """Import historical statistics into HA recorder.

    Steps:
      A. Look up entity_ids from entity registry
      B. Enrich stored records with production_own_use
      C. Calculate financial stats per hour
      D. Build cumulative sums and call async_import_statistics

    Returns True if import completed, False if entities not yet registered.
    """
    from homeassistant.helpers import entity_registry as er

    # Step A: Look up entity_ids from entity registry
    registry = er.async_get(hass)
    entity_map: dict[str, str] = {}
    for sensor_key, _, _ in SENSORS_TO_IMPORT:
        unique_id = f"{coordinator._entry_id}_{sensor_key}"
        entity_id = registry.async_get_entity_id("sensor", DOMAIN, unique_id)
        if entity_id is None:
            _LOGGER.debug(
                "Entity not yet registered for %s, deferring statistics import",
                sensor_key,
            )
            return False
        entity_map[sensor_key] = entity_id

    records = coordinator._storage.hourly_records
    if not records:
        _LOGGER.info("No hourly records to import statistics for")
        return True

    _LOGGER.info(
        "Starting historical statistics import: %d records, entities: %s",
        len(records),
        entity_map,
    )

    # Step B: Enrich with production_own_use from HA production sensor
    production_sensor = coordinator._config.get(CONF_PRODUCTION_SENSOR)
    if production_sensor:
        _LOGGER.info(
            "Querying production sensor %s for own-use enrichment",
            production_sensor,
        )
        timestamps = sorted(records.keys())
        start_dt = _parse_to_utc_datetime(timestamps[0])
        end_dt = _parse_to_utc_datetime(timestamps[-1]) + timedelta(hours=1)

        try:
            prod_deltas = await _fetch_production_deltas(
                hass, production_sensor, start_dt, end_dt
            )
            _LOGGER.info(
                "Production sensor returned %d hourly deltas", len(prod_deltas)
            )
            if prod_deltas:
                count = enrich_records_with_production_own_use(records, prod_deltas)
                _LOGGER.info("Enriched %d records with production_own_use", count)
                if count > 0:
                    await coordinator._storage.async_save()
            else:
                _LOGGER.warning(
                    "No historical statistics found for production sensor %s, "
                    "production_own_use will be 0 for historical data",
                    production_sensor,
                )
        except Exception:
            _LOGGER.warning(
                "Failed to query production sensor statistics, "
                "production_own_use will be 0 for historical data",
                exc_info=True,
            )
    else:
        _LOGGER.info("No production sensor configured, skipping own-use enrichment")

    # Step C: Calculate financial stats per hour
    all_records = coordinator._storage.get_all_records_as_list()
    _LOGGER.info(
        "Calculating financial stats for %d records, time range: %s to %s",
        len(all_records),
        all_records[0].get("timestamp", "?") if all_records else "N/A",
        all_records[-1].get("timestamp", "?") if all_records else "N/A",
    )

    # Log a sample record so we can verify data content
    if all_records:
        sample = all_records[len(all_records) // 2]
        _LOGGER.info(
            "Sample record (mid-point): ts=%s production_sold=%.3f "
            "production_own_use=%.3f purchased=%.3f unit_price_buy=%.4f",
            sample.get("timestamp", "?"),
            sample.get("production_sold", 0),
            sample.get("production_own_use", 0),
            sample.get("purchased", 0),
            sample.get("unit_price_buy", 0),
        )

    hourly_stats = calculate_hourly_financial_stats(
        all_records, coordinator._calc_params, coordinator._yearly_params
    )

    if not hourly_stats:
        _LOGGER.warning("No hourly financial stats calculated, nothing to import")
        return True

    # Step D: Build cumulative sums and import
    from homeassistant.components.recorder.models import StatisticData, StatisticMetaData
    from homeassistant.components.recorder.statistics import (
        StatisticMeanType,
        async_import_statistics,
    )

    for sensor_key, unit, attr_name in SENSORS_TO_IMPORT:
        entity_id = entity_map[sensor_key]
        stats_list = build_statistics_list(hourly_stats, attr_name)

        if not stats_list:
            _LOGGER.warning(
                "Sensor %s (%s): no statistics entries to import", sensor_key, entity_id
            )
            continue

        # Log details about the data being imported
        first_dt, first_state, first_sum = stats_list[0]
        last_dt, last_state, last_sum = stats_list[-1]
        non_zero = sum(1 for _, s, _ in stats_list if s != 0.0)
        _LOGGER.info(
            "Sensor %s → %s: %d entries (%d non-zero), "
            "range %s to %s, final sum=%.4f, unit=%s",
            sensor_key,
            entity_id,
            len(stats_list),
            non_zero,
            first_dt.isoformat(),
            last_dt.isoformat(),
            last_sum,
            unit,
        )

        if non_zero == 0:
            _LOGGER.warning(
                "Sensor %s: ALL values are 0 — history will appear empty",
                sensor_key,
            )

        metadata = StatisticMetaData(
            has_mean=False,
            has_sum=True,
            mean_type=StatisticMeanType.NONE,
            name=sensor_key,
            source="recorder",
            statistic_id=entity_id,
            unit_of_measurement=unit,
        )

        statistics = [
            StatisticData(start=start, state=state, sum=cumsum)
            for start, state, cumsum in stats_list
        ]

        _LOGGER.info(
            "Calling async_import_statistics for %s (statistic_id=%s, source=recorder)",
            sensor_key,
            entity_id,
        )
        async_import_statistics(hass, metadata, statistics)

    msg = (
        f"Historical statistics import complete: "
        f"{len(hourly_stats)} hours across {len(SENSORS_TO_IMPORT)} sensors"
    )
    _LOGGER.info(msg)

    from homeassistant.components.persistent_notification import async_create

    async_create(
        hass,
        msg,
        title="My Solar Cells",
        notification_id="my_solar_cells_statistics_import",
    )
    return True
