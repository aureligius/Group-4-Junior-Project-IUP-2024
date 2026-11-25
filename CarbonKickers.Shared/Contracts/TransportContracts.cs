namespace CarbonKickers.Shared.Contracts;

public sealed record TransportModeResponse(
    int Id,
    string Name,
    double EmissionFactorPerKm);