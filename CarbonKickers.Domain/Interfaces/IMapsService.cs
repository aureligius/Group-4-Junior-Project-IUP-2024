namespace CarbonKickers.Domain.Interfaces;

public interface IMapsService
{
    Task<double> CalculateDistanceAsync(
        string originPlaceId,
        string destinationPlaceId,
        string transportModeName);
}