namespace CarbonKickers.Shared.Contracts;

public sealed record LeaderboardEntryResponse(
    int Rank,
    string Username,
    double WeeklyCo2SavedKg,
    int WeeklyXp,
    bool IsCurrentUser
);

public sealed record WeeklyLeaderboardResponse(
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    IReadOnlyList<LeaderboardEntryResponse> TopEntries,
    LeaderboardEntryResponse? CurrentUser
);
