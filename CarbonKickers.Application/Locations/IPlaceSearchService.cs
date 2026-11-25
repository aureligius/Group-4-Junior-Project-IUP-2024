using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Locations;

public interface IPlaceSearchService
{
    Task<IReadOnlyList<PlaceSuggestionResponse>> SearchAsync(string query);
}
