using System.ComponentModel.DataAnnotations;

namespace CarbonKickers.Shared.Contracts;

public sealed record AdminSummaryResponse(
    int TotalUsers,
    int TotalTrips,
    double TotalCo2SavedKg,
    int TotalBadgesUnlocked,
    int ActiveChallenges,
    int ActiveLeagues
);

public sealed record LeagueAdminResponse(
    int Id,
    string TierName,
    int MinXp,
    int MaxXp,
    DateTime StartDate,
    DateTime EndDate
);

public sealed record CreateWeeklyLeaguesResponse(
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    IReadOnlyList<LeagueAdminResponse> Leagues
);

public sealed class SaveBadgeRequest
{
    [Required]
    [StringLength(100)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Description { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int XpRequirement { get; init; }

    [Required]
    [StringLength(500)]
    public string IconUrl { get; init; } = string.Empty;
}

public sealed record AdminBadgeResponse(
    int Id,
    string Title,
    string Description,
    int XpRequirement,
    string IconUrl
);

public sealed class SaveTransportModeRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double EmissionFactorPerKm { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed record AdminTransportModeResponse(
    int Id,
    string Name,
    double EmissionFactorPerKm,
    bool IsActive
);

public sealed record LeagueTierRuleAdminResponse(
    int Id,
    string TierName,
    int MinXp,
    int? MaxXp,
    bool IsActive
);

public sealed class SaveLeagueTierRuleRequest
{
    [Required]
    [StringLength(50)]
    public string TierName { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int MinXp { get; init; }

    [Range(0, int.MaxValue)]
    public int? MaxXp { get; init; }

    public bool IsActive { get; init; } = true;
}
