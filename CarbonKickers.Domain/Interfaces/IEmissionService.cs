namespace CarbonKickers.Domain.Interfaces;

using CarbonKickers.Domain.Entities;

public interface IEmissionService
{
    double GetEmissionFactor(TransportMode mode);
    double CalculateEmissions(double distanceKm, double emissionFactor);
}