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

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

        return response.IsSuccessStatusCode;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardResponse>("api/dashboard")
            ?? throw new InvalidOperationException(
                "The API returned an invalid dashboard response."
            );
    }

    public async Task<IReadOnlyList<TripHistoryResponse>> GetTripsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TripHistoryResponse>>("api/trips") ?? [];
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

    public async Task<WeeklyLeaderboardResponse> GetWeeklyLeaderboardAsync()
    {
        return await _httpClient.GetFromJsonAsync<WeeklyLeaderboardResponse>(
            "api/leaderboard/weekly"
        )
            ?? throw new InvalidOperationException(
                "The API returned an invalid weekly leaderboard response."
            );
    }

    public async Task<CurrentLeagueResponse?> GetMyLeagueAsync()
    {
        var response = await _httpClient.GetAsync("api/leagues/me");

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CurrentLeagueResponse>()
            ?? throw new InvalidOperationException("The API returned an invalid league response.");
    }

    public async Task<IReadOnlyList<ChallengeResponse>> GetChallengesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ChallengeResponse>>("api/challenges/me")
            ?? [];
    }

    public async Task<ChallengeResponse> CreateChallengeAsync(CreateChallengeRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/challenges", request);

        return await ReadChallengeResponseAsync(response);
    }

    public async Task<ChallengeResponse> AcceptChallengeAsync(Guid challengeId)
    {
        var response = await _httpClient.PostAsync($"api/challenges/{challengeId}/accept", null);

        return await ReadChallengeResponseAsync(response);
    }

    public async Task<ChallengeResponse> DeclineChallengeAsync(Guid challengeId)
    {
        var response = await _httpClient.PostAsync($"api/challenges/{challengeId}/decline", null);

        return await ReadChallengeResponseAsync(response);
    }

    public async Task<IReadOnlyList<UserSearchResponse>> SearchUsersAsync(string query)
    {
        var encodedQuery = Uri.EscapeDataString(query);

        return await _httpClient.GetFromJsonAsync<List<UserSearchResponse>>(
            $"api/users/search?query={encodedQuery}"
        ) ?? [];
    }

    private async Task<ChallengeResponse> ReadChallengeResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            throw new InvalidOperationException(
                error?.Message
                    ?? $"The API rejected the challenge request (HTTP {(int)response.StatusCode})."
            );
        }

        return await response.Content.ReadFromJsonAsync<ChallengeResponse>()
            ?? throw new InvalidOperationException(
                "The API returned an invalid challenge response."
            );
    }
}
