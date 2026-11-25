using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Trips;

public interface ITripService
{
    Task<CreateTripResponse> CreateAsync(
        Guid userId,
        CreateTripRequest request);
}