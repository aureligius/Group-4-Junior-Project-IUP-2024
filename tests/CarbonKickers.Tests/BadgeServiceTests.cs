using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;

namespace CarbonKickers.Tests;

public sealed class BadgeServiceTests
{
    [Fact]
    public async Task AwardEligibleBadgesAsync_AwardsOnlyEligibleBadgesOnce()
    {
        await using var db = TestDb.Create();

        var user = new User
        {
            Username = "Testing",
            Email = "testing@example.com"
        };

        user.AddXp(50);

        db.Users.Add(user);

        db.Badges.AddRange(
            new Badge
            {
                Title = "First Kick",
                Description = "Earn your first XP.",
                XpRequirement = 1,
                IconUrl = "🌱"
            },
            new Badge
            {
                Title = "Green Starter",
                Description = "Reach 50 XP.",
                XpRequirement = 50,
                IconUrl = "🌿"
            },
            new Badge
            {
                Title = "Carbon Crusher",
                Description = "Reach 500 XP.",
                XpRequirement = 500,
                IconUrl = "🏆"
            });

        await db.SaveChangesAsync();

        var service = new BadgeService(db);

        var firstAward = await service.AwardEligibleBadgesAsync(user);

        Assert.Equal(2, firstAward.Count);
        Assert.Contains(firstAward, badge => badge.Title == "First Kick");
        Assert.Contains(firstAward, badge => badge.Title == "Green Starter");

        await db.SaveChangesAsync();

        var secondAward = await service.AwardEligibleBadgesAsync(user);

        Assert.Empty(secondAward);
    }
}