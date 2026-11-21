using CarbonKickers.Domain.Entities;

namespace CarbonKickers.Infrastructure.Services;

public sealed class ClimatiqEmissionService : EmissionServiceBase
{
    protected override double CalculateEmissionsCore(
        double distanceKm,
        double emissionFactor)
    {
        return distanceKm * emissionFactor;
    }
}