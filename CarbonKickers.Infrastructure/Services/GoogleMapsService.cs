using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CarbonKickers.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarbonKickers.Infrastructure.Services;

public sealed class GoogleMapsService : IMapsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<double> CalculateDistanceAsync(
        string originPlaceId,
        string destinationPlaceId,
        string transportModeName
    )
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("GoogleMaps:ApiKey is not configured.");
        }

        var requestBody = new
        {
            origin = new { placeId = originPlaceId },
            destination = new { placeId = destinationPlaceId },
            travelMode = ConvertToGoogleTravelMode(transportModeName),
            units = "METRIC",
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "directions/v2:computeRoutes");

        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", "routes.distanceMeters");

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Google Routes API returned HTTP {(int)response.StatusCode}."
            );
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleRoutesResponse>();

        var distanceMeters = result?.Routes?.FirstOrDefault()?.DistanceMeters;

        if (distanceMeters is null || distanceMeters <= 0)
        {
            throw new InvalidOperationException(
                "Google Routes API did not return a valid route distance."
            );
        }

        return distanceMeters.Value / 1000.0;
    }

    private static string ConvertToGoogleTravelMode(string transportModeName)
    {
        return transportModeName.Trim().ToLowerInvariant() switch
        {
            "walk" => "WALK",
            "bicycle" => "BICYCLE",
            "motorcycle" => "TWO_WHEELER",
            "bus" or "train" => "TRANSIT",
            _ => "DRIVE",
        };
    }

    private sealed record GoogleRoutesResponse(
        [property: JsonPropertyName("routes")] List<GoogleRoute>? Routes
    );

    private sealed record GoogleRoute(
        [property: JsonPropertyName("distanceMeters")] long? DistanceMeters
    );

    private sealed record GoogleGeocodingResults(
        [property: JsonPropertyName("origin")] GoogleGeocodedWaypoint? Origin,
        [property: JsonPropertyName("destination")] GoogleGeocodedWaypoint? Destination
    );

    private sealed record GoogleGeocodedWaypoint(
        [property: JsonPropertyName("placeId")] string? PlaceId,
        [property: JsonPropertyName("partialMatch")] bool? PartialMatch
    );
}
