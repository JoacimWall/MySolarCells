"""Tests for the Tibber GraphQL client."""

import base64
import json
from datetime import datetime, timezone
from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from custom_components.my_solar_cells.tibber_client import (
    TibberApiError,
    TibberClient,
    _encode_to_base64,
    _parse_timestamp,
)


class TestHelpers:
    """Tests for helper functions."""

    def test_encode_to_base64(self):
        """Test Base64 encoding matches StringHelper.EncodeTo64."""
        text = "2025-06-15T00:00:00"
        result = _encode_to_base64(text)
        decoded = base64.b64decode(result).decode("utf-8")
        assert decoded == text

    def test_parse_timestamp_with_offset(self):
        """Test ISO timestamp parsing with timezone offset."""
        ts = "2025-06-15T14:00:00.000+02:00"
        dt = _parse_timestamp(ts)
        assert dt.year == 2025
        assert dt.month == 6
        assert dt.day == 15
        assert dt.hour == 14

    def test_parse_timestamp_utc(self):
        """Test ISO timestamp parsing with Z suffix."""
        ts = "2025-06-15T12:00:00+00:00"
        dt = _parse_timestamp(ts)
        assert dt.hour == 12


class TestTibberClient:
    """Tests for TibberClient methods."""

    @pytest.fixture
    def mock_session(self):
        """Create a mock aiohttp session."""
        session = MagicMock()
        return session

    def _make_client(self, session):
        return TibberClient(session, "test-api-key-123")

    @pytest.mark.asyncio
    async def test_test_connection_success(self, mock_session):
        """Test successful connection test."""
        response_data = {
            "data": {"viewer": {"accountType": ["tibber"]}}
        }
        mock_resp = AsyncMock()
        mock_resp.status = 200
        mock_resp.json = AsyncMock(return_value=response_data)
        mock_session.post = MagicMock(return_value=AsyncMock(
            __aenter__=AsyncMock(return_value=mock_resp),
            __aexit__=AsyncMock(return_value=False),
        ))

        client = self._make_client(mock_session)
        result = await client.test_connection()
        assert result == "tibber"

    @pytest.mark.asyncio
    async def test_test_connection_failure(self, mock_session):
        """Test connection test with API error."""
        mock_resp = AsyncMock()
        mock_resp.status = 401
        mock_resp.text = AsyncMock(return_value="Unauthorized")
        mock_session.post = MagicMock(return_value=AsyncMock(
            __aenter__=AsyncMock(return_value=mock_resp),
            __aexit__=AsyncMock(return_value=False),
        ))

        client = self._make_client(mock_session)
        with pytest.raises(TibberApiError):
            await client.test_connection()

    @pytest.mark.asyncio
    async def test_get_homes(self, mock_session):
        """Test fetching homes list."""
        response_data = {
            "data": {
                "viewer": {
                    "homes": [
                        {
                            "id": "home-123",
                            "address": {
                                "address1": "Test Street 1",
                                "postalCode": "12345",
                                "city": "Stockholm",
                                "country": "SE",
                            },
                        }
                    ]
                }
            }
        }
        mock_resp = AsyncMock()
        mock_resp.status = 200
        mock_resp.json = AsyncMock(return_value=response_data)
        mock_session.post = MagicMock(return_value=AsyncMock(
            __aenter__=AsyncMock(return_value=mock_resp),
            __aexit__=AsyncMock(return_value=False),
        ))

        client = self._make_client(mock_session)
        homes = await client.get_homes()
        assert len(homes) == 1
        assert homes[0]["id"] == "home-123"
        assert homes[0]["address1"] == "Test Street 1"
        assert homes[0]["city"] == "Stockholm"

    @pytest.mark.asyncio
    async def test_get_spot_prices(self, mock_session):
        """Test fetching spot prices at quarter-hourly resolution."""
        response_data = {
            "data": {
                "viewer": {
                    "home": {
                        "currentSubscription": {
                            "priceInfo": {
                                "today": [
                                    {
                                        "total": 0.85,
                                        "energy": 0.65,
                                        "tax": 0.20,
                                        "level": "NORMAL",
                                        "startsAt": "2025-06-15T00:00:00.000+02:00",
                                        "currency": "SEK",
                                    },
                                    {
                                        "total": 0.82,
                                        "energy": 0.62,
                                        "tax": 0.20,
                                        "level": "CHEAP",
                                        "startsAt": "2025-06-15T00:15:00.000+02:00",
                                        "currency": "SEK",
                                    },
                                ],
                                "tomorrow": [],
                            }
                        }
                    }
                }
            }
        }
        mock_resp = AsyncMock()
        mock_resp.status = 200
        mock_resp.json = AsyncMock(return_value=response_data)
        mock_session.post = MagicMock(return_value=AsyncMock(
            __aenter__=AsyncMock(return_value=mock_resp),
            __aexit__=AsyncMock(return_value=False),
        ))

        client = self._make_client(mock_session)
        prices = await client.get_spot_prices("home-123")
        assert len(prices["today"]) == 2
        assert prices["today"][0]["total"] == 0.85
        assert prices["today"][1]["level"] == "CHEAP"
        assert prices["tomorrow"] == []

    def test_headers(self, mock_session):
        """Test that headers include API key."""
        client = self._make_client(mock_session)
        headers = client._headers
        assert headers["Authorization"] == "Bearer test-api-key-123"
        assert headers["Content-Type"] == "application/json"

    @pytest.mark.asyncio
    async def test_graphql_error_response(self, mock_session):
        """Test handling of GraphQL errors in response body."""
        response_data = {
            "errors": [{"message": "Invalid query"}]
        }
        mock_resp = AsyncMock()
        mock_resp.status = 200
        mock_resp.json = AsyncMock(return_value=response_data)
        mock_session.post = MagicMock(return_value=AsyncMock(
            __aenter__=AsyncMock(return_value=mock_resp),
            __aexit__=AsyncMock(return_value=False),
        ))

        client = self._make_client(mock_session)
        with pytest.raises(TibberApiError, match="Invalid query"):
            await client.test_connection()
