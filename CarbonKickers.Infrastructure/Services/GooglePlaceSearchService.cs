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

    public GooglePlaceSearchService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<PlaceSuggestionResponse>> SearchAsync(string query)
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("GoogleMaps:ApiKey is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/places:searchText");

        request.Content = JsonContent.Create(
            new
            {
                textQuery = query.Trim(),
                languageCode = "id",
                regionCode = "ID",
                pageSize = 5,
            }
        );

        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add(
            "X-Goog-FieldMask",
            "places.id,places.displayName,places.formattedAddress"
        );

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Google Places API returned HTTP {(int)response.StatusCode}."
            );
        }

        var result = await response.Content.ReadFromJsonAsync<GooglePlacesSearchResponse>();

        return result
            ?.Places?.Where(place =>
                !string.IsNullOrWhiteSpace(place.Id)
                && !string.IsNullOrWhiteSpace(place.FormattedAddress)
            )
            .Select(place => new PlaceSuggestionResponse(
                place.Id!,
                place.DisplayName?.Text ?? place.FormattedAddress!,
                place.FormattedAddress!
            ))
            .ToList()
            ?? [];
    }

    private sealed record GooglePlacesSearchResponse(
        [property: JsonPropertyName("places")] List<GooglePlace>? Places
    );

    private sealed record GooglePlace(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("displayName")] GoogleDisplayName? DisplayName,
        [property: JsonPropertyName("formattedAddress")] string? FormattedAddress
    );

    private sealed record GoogleDisplayName([property: JsonPropertyName("text")] string? Text);
}
