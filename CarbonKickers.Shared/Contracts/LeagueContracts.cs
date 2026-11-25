namespace CarbonKickers.Shared.Contracts;

public sealed record LeagueStandingResponse(
    int Rank,
    string Username,
    double Co2SavedKg,
    int XpEarned,
    bool IsCurrentUser
);

public sealed record CurrentLeagueResponse(
    int LeagueId,
    string TierName,
    int MinXp,
    int MaxXp,
    DateTime StartDate,
    DateTime EndDate,
    int ParticipantCount,
    IReadOnlyList<LeagueStandingResponse> Standings
);

public sealed record LeagueTierResponse(string TierName, int MinXp, int? MaxXp);
