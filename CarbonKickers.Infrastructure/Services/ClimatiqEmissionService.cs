namespace CarbonKickers.Infrastructure.Services;

using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Interfaces;

public class ClimatiqEmissionService : IEmissionService
{
    public double GetEmissionFactor(TransportMode mode) => mode.EmissionFactorPerKm;

    public double CalculateEmissions(double distanceKm, double emissionFactor)
    {
        return distanceKm * emissionFactor;
    }
}