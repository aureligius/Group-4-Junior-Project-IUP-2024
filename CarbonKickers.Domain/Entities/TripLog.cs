namespace CarbonKickers.Domain.Entities;

using CarbonKickers.Domain.Interfaces;

public class TripLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    
    public int TransportModeId { get; set; }
    public TransportMode TransportMode { get; set; } = null!;
    
    public int BaselineTransportModeId { get; set; }
    public TransportMode BaselineTransportMode { get; set; } = null!;
    
    public double Co2EmittedKg { get; private set; }
    public double Co2SavedKg { get; private set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public void CalculateSavings(IEmissionService emissionService)
    {
        double actualFactor = emissionService.GetEmissionFactor(TransportMode);
        Co2EmittedKg = emissionService.CalculateEmissions(DistanceKm, actualFactor);

        double baselineFactor = emissionService.GetEmissionFactor(BaselineTransportMode);
        double baselineEmissions = emissionService.CalculateEmissions(DistanceKm, baselineFactor);

        Co2SavedKg = Math.Max(0, baselineEmissions - Co2EmittedKg);
    }
}