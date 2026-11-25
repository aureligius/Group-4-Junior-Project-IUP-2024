using CarbonKickers.Domain.Models;

namespace CarbonKickers.Domain.Interfaces;

public interface IMapsService
{
    Task<RouteDistanceResult> CalculateDistanceAsync(
        string originPlaceId,
        string destinationPlaceId,
        string requestedTransportModeName);
}