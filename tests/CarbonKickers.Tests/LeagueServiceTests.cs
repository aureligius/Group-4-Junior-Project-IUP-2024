using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;

namespace CarbonKickers.Tests;

public sealed class LeagueServiceTests
{
    [Fact]
    public async Task GetCurrentLeagueAsync_AutomaticallyJoinsEligibleLeague()
    {
        await using var db = TestDb.Create();

        var user = new User
        {
            Username = "Testing",
            Email = "testing@example.com"
        };

        user.AddXp(150);

        var now = DateTime.UtcNow;

        var silverLeague = new League
        {
            TierName = "Silver",
            MinXp = 100,
            MaxXp = 299,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(6)
        };

        db.AddRange(user, silverLeague);
        await db.SaveChangesAsync();

        var service = new LeagueService(db);

        var result = await service.GetCurrentLeagueAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("Silver", result!.TierName);
        Assert.Equal(1, result.ParticipantCount);

        Assert.Single(db.LeagueParticipants);

        Assert.Equal(user.Id, db.LeagueParticipants.Single().UserId);
        Assert.Equal(silverLeague.Id, db.LeagueParticipants.Single().LeagueId);
    }

    [Fact]
    public async Task GetCurrentLeagueAsync_ReturnsNullWhenNoLeagueExists()
    {
        await using var db = TestDb.Create();

        var user = new User
        {
            Username = "Testing",
            Email = "testing@example.com"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new LeagueService(db);

        var result = await service.GetCurrentLeagueAsync(user.Id);

        Assert.Null(result);
    }
}