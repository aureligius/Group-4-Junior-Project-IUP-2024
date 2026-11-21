using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Trips;

public interface ITripHistoryService
{
    Task<IReadOnlyList<TripHistoryResponse>> GetForUserAsync(
        Guid userId);
}