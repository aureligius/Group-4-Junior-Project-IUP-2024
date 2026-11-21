namespace CarbonKickers.Shared.Contracts;

public sealed record TripHistoryResponse(
    Guid Id,
    string Origin,
    string Destination,
    string TransportModeName,
    string BaselineTransportModeName,
    double DistanceKm,
    double Co2EmittedKg,
    double Co2SavedKg,
    int XpEarned,
    DateTime LoggedAt);