namespace CarbonKickers.Application.Leagues;

public sealed record LeagueTierDefinition(
    string TierName,
    int MinXp,
    int? MaxXp
);

public static class LeagueTierCatalog
{
    public static IReadOnlyList<LeagueTierDefinition> All { get; } =
    [
        new("Bronze", 0, 99),
        new("Silver", 100, 299),
        new("Gold", 300, 699),
        new("Diamond", 700, null),
    ];
}