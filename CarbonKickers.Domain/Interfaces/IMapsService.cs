namespace CarbonKickers.Domain.Interfaces;

public interface IMapsService
{
    Task<double> CalculateDistanceAsync(string origin, string destination);
}