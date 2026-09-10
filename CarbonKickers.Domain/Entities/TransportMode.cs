namespace CarbonKickers.Domain.Entities;

public class TransportMode
{
    public int Id { get; set; }
    public string ModeName { get; set; } = string.Empty;
    public double EmissionFactorPerKm { get; set; }

    public double GetEmissionFactor() => EmissionFactorPerKm;
}