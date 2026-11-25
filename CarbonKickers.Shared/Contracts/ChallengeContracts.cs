using System.ComponentModel.DataAnnotations;

namespace CarbonKickers.Shared.Contracts;

public sealed class CreateChallengeRequest
{
    [Required]
    [StringLength(50)]
    public string OpponentUsername { get; init; } = string.Empty;

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }
}

public sealed record ChallengeResponse(
    Guid Id,
    string ChallengerUsername,
    string OpponentUsername,
    DateTime StartDate,
    DateTime EndDate,
    double ChallengerSavedKg,
    double OpponentSavedKg,
    string Status,
    string? WinnerUsername,
    bool IsCurrentUserChallenger
);
