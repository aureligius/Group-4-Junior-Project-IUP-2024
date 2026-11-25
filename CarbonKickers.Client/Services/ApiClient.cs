using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Services;

/// <summary>
/// Thrown when a signed-in request comes back 401: the login token has expired.
/// The <see cref="ApiClient.SessionExpired"/> event has already sent the user back to login,
/// so screens don't need to show anything for it.
/// </summary>
public sealed class SessionExpiredException : Exception
{
    public SessionExpiredException()
        : base("Your session has expired. Please log in again.") { }
}

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient(new SessionExpiryHandler(this))
        {
            BaseAddress = new Uri("https://localhost:7211/"),
            // HttpClient waits 100 s by default. Saving a trip makes two Google route calls
            // (15 s limit each on the API), so 30 s covers a slow save without a long freeze.
            Timeout = TimeSpan.FromSeconds(30),
        };
    }

    /// <summary>
    /// Raised once, on the UI thread, when the API rejects the login token (it lasts 8 hours).
    /// </summary>
    public event EventHandler? SessionExpired;

    private int _sessionExpiredRaised;

    private void OnSessionExpired()
    {
        if (Interlocked.Exchange(ref _sessionExpiredRaised, 1) == 1)
        {
            return;
        }

        System.Windows.Application.Current?.Dispatcher.InvokeAsync(
            () => SessionExpired?.Invoke(this, EventArgs.Empty)
        );
    }

    // Sits in front of every request: a 401 on a request that carried a token means it expired.
    private sealed class SessionExpiryHandler(ApiClient owner) : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var response = await base.SendAsync(request, cancellationToken);

            var hadToken = owner._httpClient.DefaultRequestHeaders.Authorization is not null;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && hadToken)
            {
                owner.OnSessionExpired();
                throw new SessionExpiredException();
            }

            return response;
        }
    }

    /// <summary>
    /// Logs in. Returns null when the email or password is wrong; throws
    /// <see cref="InvalidOperationException"/> with the reason for any other failure.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            // e.g. the database is down (500): say so instead of blaming the password
            throw new InvalidOperationException(
                await ReadErrorMessageAsync(response)
                    ?? $"Login failed on the server (HTTP {(int)response.StatusCode}). Please try again."
            );
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
