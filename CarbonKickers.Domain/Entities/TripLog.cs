namespace CarbonKickers.Domain.Entities;

using CarbonKickers.Domain.Interfaces;

public class TripLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string? OriginPlaceId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string? DestinationPlaceId { get; set; }
    public double DistanceKm { get; set; }

    public double BaselineDistanceKm { get; set; }

    public string ActualRouteMode { get; set; } = "DRIVE";
    public bool ActualDistanceIsEstimated { get; set; }

    public string BaselineRouteMode { get; set; } = "DRIVE";
    public bool BaselineDistanceIsEstimated { get; set; }

    public int TransportModeId { get; set; }
    public TransportMode TransportMode { get; set; } = null!;

    public int BaselineTransportModeId { get; set; }
    public TransportMode BaselineTransportMode { get; set; } = null!;

    public double Co2EmittedKg { get; private set; }
    public double BaselineCo2EmittedKg { get; private set; }
    public double Co2SavedKg { get; private set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public void CalculateSavings(IEmissionService emissionService)
    {
        var actualFactor = emissionService.GetEmissionFactor(TransportMode);

        Co2EmittedKg = emissionService.CalculateEmissions(DistanceKm, actualFactor);

        var baselineFactor = emissionService.GetEmissionFactor(BaselineTransportMode);

        BaselineCo2EmittedKg = emissionService.CalculateEmissions(
            BaselineDistanceKm,
            baselineFactor
        );

        Co2SavedKg = Math.Max(0, BaselineCo2EmittedKg - Co2EmittedKg);
    }
}
