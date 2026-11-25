using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CarbonKickers.Application.Locations;
using CarbonKickers.Shared.Contracts;
using Microsoft.Extensions.Configuration;

namespace CarbonKickers.Infrastructure.Services;

public sealed class GooglePlaceSearchService : IPlaceSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GooglePlaceSearchService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<PlaceSuggestionResponse>> SearchAsync(
        string query)
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "GoogleMaps:ApiKey is not configured.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "v1/places:autocomplete");

        request.Content = JsonContent.Create(new
        {
            input = query.Trim(),
            languageCode = "id",
            includedRegionCodes = new[] { "id" },

            // Improves Yogyakarta-area results but does not exclude other places.
            locationBias = new
            {
                circle = new
                {
                    center = new
                    {
                        latitude = -7.7956,
                        longitude = 110.3695
                    },
                    radius = 20000.0
                }
            }
        });

        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add(
            "X-Goog-FieldMask",
            "suggestions.placePrediction.placeId," +
            "suggestions.placePrediction.structuredFormat.mainText.text," +
            "suggestions.placePrediction.structuredFormat.secondaryText.text");

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Google Places Autocomplete returned HTTP {(int)response.StatusCode}.");
        }

        var result = await response.Content
            .ReadFromJsonAsync<GoogleAutocompleteResponse>();

        return result?.Suggestions?
            .Where(item =>
                !string.IsNullOrWhiteSpace(
                    item.PlacePrediction?.PlaceId))
            .Select(item =>
            {
                var prediction = item.PlacePrediction!;

                var displayName =
                    prediction.StructuredFormat?.MainText?.Text
                    ?? "Unnamed location";

                var secondaryText =
                    prediction.StructuredFormat?.SecondaryText?.Text;

                return new PlaceSuggestionResponse(
                    prediction.PlaceId!,
                    displayName,
                    string.IsNullOrWhiteSpace(secondaryText)
                        ? displayName
                        : secondaryText);
            })
            .ToList()
            ?? [];
    }

    private sealed record GoogleAutocompleteResponse(
        [property: JsonPropertyName("suggestions")]
        List<GoogleSuggestion>? Suggestions);

    private sealed record GoogleSuggestion(
        [property: JsonPropertyName("placePrediction")]
        GooglePlacePrediction? PlacePrediction);

    private sealed record GooglePlacePrediction(
        [property: JsonPropertyName("placeId")]
        string? PlaceId,

        [property: JsonPropertyName("structuredFormat")]
        GoogleStructuredFormat? StructuredFormat);

    private sealed record GoogleStructuredFormat(
        [property: JsonPropertyName("mainText")]
        GoogleText? MainText,

        [property: JsonPropertyName("secondaryText")]
        GoogleText? SecondaryText);

    private sealed record GoogleText(
        [property: JsonPropertyName("text")]
        string? Text);
}