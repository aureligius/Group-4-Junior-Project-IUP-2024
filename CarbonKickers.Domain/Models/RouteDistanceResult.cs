namespace CarbonKickers.Domain.Models;

public sealed record RouteDistanceResult(
    double DistanceKm,
    string RouteModeUsed,
    bool IsEstimated);