namespace CarbonKickers.Client.Services;

// Presentation data only.
// These values must match AdminService.CreateCurrentWeekLeaguesAsync.
public static class TierCatalog
{
    public sealed record Tier(string Name, int MinXp, int? MaxXp);

    public static IReadOnlyList<Tier> All { get; } =
    [
        new("Bronze", 0, 99),
        new("Silver", 100, 299),
        new("Gold", 300, 699),
        new("Diamond", 700, null),
    ];

    public static Tier ForXp(int xp)
    {
        return All.Last(tier => xp >= tier.MinXp);
    }

    public static Tier? NextAfter(Tier tier)
    {
        return All.SkipWhile(item => item != tier).Skip(1).FirstOrDefault();
    }
}