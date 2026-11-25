namespace CarbonKickers.Shared.Contracts;

public sealed record PlaceSuggestionResponse(
    string PlaceId,
    string DisplayName,
    string FormattedAddress);