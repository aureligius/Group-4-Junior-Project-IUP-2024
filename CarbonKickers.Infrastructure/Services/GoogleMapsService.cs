using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CarbonKickers.Domain.Interfaces;
using CarbonKickers.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace CarbonKickers.Infrastructure.Services;

public sealed class GoogleMapsService : IMapsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleMapsService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<RouteDistanceResult> CalculateDistanceAsync(
        string originPlaceId,
        string destinationPlaceId,
        string requestedTransportModeName)
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "GoogleMaps:ApiKey is not configured.");
        }

        var modesToTry = GetTravelModes(requestedTransportModeName);

        for (var index = 0; index < modesToTry.Count; index++)
        {
            var routeMode = modesToTry[index];

            var distanceKm = await TryGetDistanceAsync(
                originPlaceId,
                destinationPlaceId,
                routeMode,
                apiKey);

            if (distanceKm is not null)
            {
                return new RouteDistanceResult(
                    distanceKm.Value,
                    routeMode,
                    IsEstimated: index > 0);
            }
        }

        throw new InvalidOperationException(
            $"No route was found for {requestedTransportModeName}.");
    }

    private async Task<double?> TryGetDistanceAsync(
        string originPlaceId,
        string destinationPlaceId,
        string travelMode,
        string apiKey)
    {
        var requestBody = new
        {
            origin = new { placeId = originPlaceId },
            destination = new { placeId = destinationPlaceId },
            travelMode,
            units = "METRIC"
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "directions/v2:computeRoutes");

        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add(
            "X-Goog-FieldMask",
            "routes.distanceMeters");

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Google Routes API returned HTTP {(int)response.StatusCode}.");
        }

        var result = await response.Content
            .ReadFromJsonAsync<GoogleRoutesResponse>();

        var distanceMeters = result?.Routes?
            .FirstOrDefault()?
            .DistanceMeters;

        return distanceMeters is > 0
            ? distanceMeters.Value / 1000.0
            : null;
    }

    private static IReadOnlyList<string> GetTravelModes(
        string transportModeName)
    {
        return transportModeName.Trim().ToLowerInvariant() switch
        {
            "walk" => ["WALK", "DRIVE"],
            "bicycle" => ["BICYCLE", "WALK", "DRIVE"],
            "motorcycle" => ["TWO_WHEELER", "DRIVE"],
            "bus" or "train" => ["TRANSIT", "DRIVE"],
            _ => ["DRIVE"]
        };
    }

    private sealed record GoogleRoutesResponse(
        [property: JsonPropertyName("routes")]
        List<GoogleRoute>? Routes);

    private sealed record GoogleRoute(
        [property: JsonPropertyName("distanceMeters")]
        long? DistanceMeters);
}