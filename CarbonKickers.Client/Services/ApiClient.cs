using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7211/") };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result is not null && !string.IsNullOrWhiteSpace(result.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                result.Token
            );
        }

        return result;
    }

    /// <summary>
    /// Registers a new account. Returns null on success, or the reason the API rejected it.
    /// </summary>
    public async Task<string?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadErrorMessageAsync(response)
            ?? $"Registration failed (HTTP {(int)response.StatusCode}). Please try again.";
    }

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return string.IsNullOrWhiteSpace(error?.Message) ? null : error.Message;
        }
        catch (System.Text.Json.JsonException)
        {
            // Body was empty or not JSON (e.g. an unhandled server error page).
            return null;
        }
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        // Lets the API group trips by this machine's calendar days (e.g. 420 for UTC+7).
        var utcOffsetMinutes = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

        return await _httpClient.GetFromJsonAsync<DashboardResponse>(
                $"api/dashboard?utcOffsetMinutes={utcOffsetMinutes}"
            )
            ?? throw new InvalidOperationException(
                "The API returned an invalid dashboard response."
            );
    }

    public async Task<IReadOnlyList<TripHistoryResponse>> GetTripsAsync()
    {
        var trips =
            await _httpClient.GetFromJsonAsync<List<TripHistoryResponse>>("api/trips") ?? [];

        // The API sends UTC timestamps; show them in the user's local time.
        return trips.Select(trip => trip with { LoggedAt = trip.LoggedAt.ToLocalTime() }).ToList();
    }

    public async Task<IReadOnlyList<TransportModeResponse>> GetTransportModesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TransportModeResponse>>(
            "api/transport-modes"
        ) ?? [];
    }

    public async Task<IReadOnlyList<PlaceSuggestionResponse>> SearchPlacesAsync(string query)
    {
        var encodedQuery = Uri.EscapeDataString(query);

        return await _httpClient.GetFromJsonAsync<List<PlaceSuggestionResponse>>(
            $"api/places/search?query={encodedQuery}"
        ) ?? [];
    }

    public async Task<CreateTripResponse> CreateTripAsync(CreateTripRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trips", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            throw new InvalidOperationException(
                error?.Message
                    ?? $"The API rejected the trip request (HTTP {(int)response.StatusCode})."
            );
        }

        return await response.Content.ReadFromJsonAsync<CreateTripResponse>()
            ?? throw new InvalidOperationException("The API returned an invalid trip response.");
    }

    private sealed record ApiErrorResponse(string? Message);

    public async Task<IReadOnlyList<BadgeResponse>> GetBadgesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<BadgeResponse>>("api/badges/me") ?? [];
    }
}
