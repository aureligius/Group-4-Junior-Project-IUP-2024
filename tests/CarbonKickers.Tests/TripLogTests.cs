using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;

namespace CarbonKickers.Tests;

public sealed class TripLogTests
{
    [Fact]
    public void CalculateSavings_UsesActualAndBaselineDistances()
    {
        var actualMode = new TransportMode
        {
            ModeName = "Motorcycle",
            EmissionFactorPerKm = 0.103
        };

        var baselineMode = new TransportMode
        {
            ModeName = "Car",
            EmissionFactorPerKm = 0.192
        };

        var trip = new TripLog
        {
            DistanceKm = 10,
            BaselineDistanceKm = 15,
            TransportMode = actualMode,
            BaselineTransportMode = baselineMode
        };

        trip.CalculateSavings(new ClimatiqEmissionService());

        Assert.Equal(1.03, trip.Co2EmittedKg, 2);
        Assert.Equal(2.88, trip.BaselineCo2EmittedKg, 2);
        Assert.Equal(1.85, trip.Co2SavedKg, 2);
    }

    [Fact]
    public void CalculateSavings_NeverReturnsNegativeSavings()
    {
        var actualMode = new TransportMode
        {
            ModeName = "Car",
            EmissionFactorPerKm = 0.192
        };

        var baselineMode = new TransportMode
        {
            ModeName = "Bicycle",
            EmissionFactorPerKm = 0
        };

        var trip = new TripLog
        {
            DistanceKm = 15,
            BaselineDistanceKm = 10,
            TransportMode = actualMode,
            BaselineTransportMode = baselineMode
        };

        trip.CalculateSavings(new ClimatiqEmissionService());

        Assert.Equal(0, trip.Co2SavedKg);
    }

    [Fact]
    public void CalculateEmissions_RejectsNegativeDistance()
    {
        var service = new ClimatiqEmissionService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.CalculateEmissions(-1, 0.192));
    }
}