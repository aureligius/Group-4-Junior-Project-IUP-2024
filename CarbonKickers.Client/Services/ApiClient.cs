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

    public async Task<IReadOnlyList<LeagueTierResponse>> GetLeagueTiersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<LeagueTierResponse>>("api/leagues/tiers")
            ?? [];
    }

    public async Task<AdminSummaryResponse> GetAdminSummaryAsync()
    {
        return await _httpClient.GetFromJsonAsync<AdminSummaryResponse>("api/admin/summary")
            ?? throw new InvalidOperationException("The API returned an invalid admin summary.");
    }

    public async Task<CreateWeeklyLeaguesResponse> CreateCurrentWeekLeaguesAsync()
    {
        var response = await _httpClient.PostAsync("api/admin/leagues/create-current-week", null);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            throw new InvalidOperationException(
                error?.Message ?? $"The API rejected the request (HTTP {(int)response.StatusCode})."
            );
        }

        return await response.Content.ReadFromJsonAsync<CreateWeeklyLeaguesResponse>()
            ?? throw new InvalidOperationException(
                "The API returned an invalid league creation response."
            );
    }

    public async Task<IReadOnlyList<AdminBadgeResponse>> GetAdminBadgesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AdminBadgeResponse>>("api/admin/badges")
            ?? [];
    }

    public async Task<AdminBadgeResponse> CreateAdminBadgeAsync(SaveBadgeRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/badges", request);

        return await ReadAdminResponseAsync<AdminBadgeResponse>(response);
    }

    public async Task<AdminBadgeResponse> UpdateAdminBadgeAsync(
        int badgeId,
        SaveBadgeRequest request
    )
    {
        var response = await _httpClient.PutAsJsonAsync($"api/admin/badges/{badgeId}", request);

        return await ReadAdminResponseAsync<AdminBadgeResponse>(response);
    }

    private async Task<T> ReadAdminResponseAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            throw new InvalidOperationException(
                error?.Message ?? $"The API rejected the request (HTTP {(int)response.StatusCode})."
            );
        }

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("The API returned an invalid admin response.");
    }

    public async Task<IReadOnlyList<AdminTransportModeResponse>> GetAdminTransportModesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AdminTransportModeResponse>>(
            "api/admin/transport-modes"
        ) ?? [];
    }

    public async Task<AdminTransportModeResponse> CreateAdminTransportModeAsync(
        SaveTransportModeRequest request
    )
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/transport-modes", request);

        return await ReadAdminResponseAsync<AdminTransportModeResponse>(response);
    }

    public async Task<AdminTransportModeResponse> UpdateAdminTransportModeAsync(
        int modeId,
        SaveTransportModeRequest request
    )
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/admin/transport-modes/{modeId}",
            request
        );

        return await ReadAdminResponseAsync<AdminTransportModeResponse>(response);
    }

    public async Task<IReadOnlyList<LeagueTierRuleAdminResponse>> GetLeagueTierRulesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<LeagueTierRuleAdminResponse>>(
            "api/admin/league-tier-rules"
        ) ?? [];
    }

    public async Task<LeagueTierRuleAdminResponse> CreateLeagueTierRuleAsync(
        SaveLeagueTierRuleRequest request
    )
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/league-tier-rules", request);

        return await ReadAdminResponseAsync<LeagueTierRuleAdminResponse>(response);
    }

    public async Task<LeagueTierRuleAdminResponse> UpdateLeagueTierRuleAsync(
        int ruleId,
        SaveLeagueTierRuleRequest request
    )
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/admin/league-tier-rules/{ruleId}",
            request
        );

        return await ReadAdminResponseAsync<LeagueTierRuleAdminResponse>(response);
    }
}
