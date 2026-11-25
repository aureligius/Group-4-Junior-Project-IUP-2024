using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Interfaces;

namespace CarbonKickers.Infrastructure.Services;

public abstract class EmissionServiceBase : IEmissionService
{
    public double GetEmissionFactor(TransportMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        return mode.GetEmissionFactor();
    }

    public double CalculateEmissions(double distanceKm, double emissionFactor)
    {
        ValidateInputs(distanceKm, emissionFactor);

        return CalculateEmissionsCore(distanceKm, emissionFactor);
    }

    protected abstract double CalculateEmissionsCore(
        double distanceKm,
        double emissionFactor);

    private static void ValidateInputs(
        double distanceKm,
        double emissionFactor)
    {
        if (distanceKm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distanceKm),
                "Distance cannot be negative.");
        }

        if (emissionFactor < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(emissionFactor),
                "Emission factor cannot be negative.");
        }
    }
}