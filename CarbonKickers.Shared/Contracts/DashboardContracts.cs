namespace CarbonKickers.Shared.Contracts;

public sealed record DashboardResponse(
    string Username,
    int GreenXp,
    double TotalCo2SavedKg,
    int TotalTrips,
    IReadOnlyList<DailyImpactResponse> LastSevenDays);

public sealed record DailyImpactResponse(
    DateOnly Date,
    double Co2SavedKg,
    int TripCount);