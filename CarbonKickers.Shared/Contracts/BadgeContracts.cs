namespace CarbonKickers.Shared.Contracts;

public sealed record BadgeResponse(
    int Id,
    string Title,
    string Description,
    int XpRequirement,
    string IconUrl,
    DateTime UnlockedAt);