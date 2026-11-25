namespace CarbonKickers.Client.Services;

// Sample data for the screens whose back end doesn't exist yet: leaderboard, weekly league and arena.
// Every rival here is synthetic, and joining or challenging only lasts until the app closes.
// When CarbonKickers.Api gets these endpoints, replace the PreviewData calls with ApiClient calls.
public static class PreviewData
{
    public sealed record Tier(string Name, int MinXp, int? MaxXp);

    public sealed record Rival(string Username, int GreenXp, int WeeklyXp, double Co2SavedKg);

    public enum ChallengeState
    {
        Incoming,
        Waiting,
        Active,
        Won,
        Lost,
        Declined,
    }

    public sealed class Challenge
    {
        public required string Opponent { get; init; }
        public required int Days { get; init; }
        public required DateTime EndsAt { get; set; }
        public double MineKg { get; set; }
        public double TheirsKg { get; set; }
        public ChallengeState State { get; set; }
    }

    public static IReadOnlyList<Tier> Tiers { get; } =
    [
        new("Bronze", 0, 499),
        new("Silver", 500, 1499),
        new("Gold", 1500, 2999),
        new("Platinum", 3000, 4999),
        new("Diamond", 5000, null),
    ];

    public static IReadOnlyList<Rival> Rivals { get; } =
    [
        new("WalkItOut", 5320, 410, 248.6),
        new("GreenGoblin", 3480, 385, 162.3),
        new("PedalPusher", 2210, 340, 104.9),
        new("BusBuddy", 1640, 290, 77.2),
        new("SolarSari", 1180, 260, 55.0),
        new("KopiKeliling", 760, 215, 36.4),
        new("LeafLover", 520, 180, 24.8),
        new("TrainTom", 410, 150, 19.5),
        new("RideRina", 260, 120, 12.1),
        new("StrollSam", 140, 95, 6.7),
        new("NewLeafNia", 60, 60, 2.9),
    ];

    public static List<Challenge> Challenges { get; } =
    [
        new() { Opponent = "PedalPusher", Days = 7, EndsAt = DateTime.Today.AddDays(7), State = ChallengeState.Incoming },
        new() { Opponent = "GreenGoblin", Days = 7, EndsAt = DateTime.Today.AddDays(4), MineKg = 6.4, TheirsKg = 5.1, State = ChallengeState.Active },
        new() { Opponent = "TrainTom", Days = 5, EndsAt = DateTime.Today.AddDays(2), MineKg = 2.0, TheirsKg = 3.3, State = ChallengeState.Active },
        new() { Opponent = "BusBuddy", Days = 7, EndsAt = DateTime.Today.AddDays(-5), MineKg = 8.2, TheirsKg = 6.9, State = ChallengeState.Won },
        new() { Opponent = "WalkItOut", Days = 7, EndsAt = DateTime.Today.AddDays(-12), MineKg = 4.1, TheirsKg = 7.5, State = ChallengeState.Lost },
    ];

    public static bool HasJoinedCompetition { get; private set; }

    public static bool HasJoinedLeague { get; private set; }

    // Competing puts the player on the leaderboard and lets them battle in the Arena
    public static void JoinCompetition() => HasJoinedCompetition = true;

    // Joining a league also joins the competition
    public static void JoinLeague()
    {
        HasJoinedCompetition = true;
        HasJoinedLeague = true;
    }

    public static Tier TierFor(int xp) => Tiers.Last(tier => xp >= tier.MinXp);

    public static Tier? NextTier(Tier tier) =>
        Tiers.SkipWhile(t => t != tier).Skip(1).FirstOrDefault();

    // Leagues run Monday to Sunday
    public static DateTime WeekStart =>
        DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));

    public static int DaysLeftThisWeek => Math.Max(1, (WeekStart.AddDays(7) - DateTime.Today).Days);

    // This week's league for a tier: eight rivals whose XP sits inside the tier.
    public static IReadOnlyList<Rival> LeagueRivals(Tier tier)
    {
        var top = tier.MaxXp ?? tier.MinXp + 3000;
        var names = Rivals.Select(r => r.Username).ToList();
        int[] weekly = [420, 365, 310, 255, 205, 160, 110, 70];

        return Enumerable.Range(0, 8)
            .Select(i => new Rival(
                names[i],
                tier.MinXp + (top - tier.MinXp) * (8 - i) / 9,
                weekly[i],
                Math.Round(weekly[i] / 10.0, 1)))
            .ToList();
    }

    public static IEnumerable<string> ChallengeableRivals() =>
        Rivals.Select(r => r.Username)
            .Where(name => !Challenges.Any(c =>
                c.Opponent == name && c.State is ChallengeState.Incoming or ChallengeState.Waiting or ChallengeState.Active));

    public static void Accept(Challenge challenge)
    {
        challenge.State = ChallengeState.Active;
        challenge.EndsAt = DateTime.Today.AddDays(challenge.Days);
        HasJoinedCompetition = true;
    }

    public static void Decline(Challenge challenge) => challenge.State = ChallengeState.Declined;

    public static void SendChallenge(string opponent, int days)
    {
        HasJoinedCompetition = true;
        Challenges.Insert(0, new Challenge
        {
            Opponent = opponent,
            Days = days,
            EndsAt = DateTime.Today.AddDays(days),
            State = ChallengeState.Waiting,
        });
    }
}
