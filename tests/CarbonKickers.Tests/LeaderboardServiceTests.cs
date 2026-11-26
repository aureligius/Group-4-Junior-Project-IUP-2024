using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;

namespace CarbonKickers.Tests;

public sealed class LeaderboardServiceTests
{
    [Fact]
    public async Task GetWeeklyAsync_RanksUsersByWeeklyCo2Saved()
    {
        await using var db = TestDb.Create();

        var bicycle = new TransportMode { ModeName = "Bicycle", EmissionFactorPerKm = 0 };

        var car = new TransportMode { ModeName = "Car", EmissionFactorPerKm = 0.192 };

        var alice = new User { Username = "Alice", Email = "alice@example.com" };

        var bob = new User { Username = "Bob", Email = "bob@example.com" };

        var emissionService = new ClimatiqEmissionService();

        var aliceTrip = new TripLog
        {
            UserId = alice.Id,
            DistanceKm = 10,
            BaselineDistanceKm = 10,
            TransportMode = bicycle,
            BaselineTransportMode = car,
            LoggedAt = DateTime.UtcNow,
        };

        aliceTrip.CalculateSavings(emissionService);
        // Alice saves 10 × 0.192 = 1.92 kg CO₂.

        var bobTrip = new TripLog
        {
            UserId = bob.Id,
            DistanceKm = 20,
            BaselineDistanceKm = 20,
            TransportMode = bicycle,
            BaselineTransportMode = car,
            LoggedAt = DateTime.UtcNow,
        };

        bobTrip.CalculateSavings(emissionService);
        // Bob saves 20 × 0.192 = 3.84 kg CO₂.

        db.AddRange(bicycle, car, alice, bob, aliceTrip, bobTrip);

        await db.SaveChangesAsync();

        var service = new LeaderboardService(db);

        var result = await service.GetWeeklyAsync(alice.Id);

        Assert.Equal(2, result.TopEntries.Count);

        Assert.Equal("Bob", result.TopEntries[0].Username);
        Assert.Equal(1, result.TopEntries[0].Rank);
        Assert.Equal(3.84, result.TopEntries[0].WeeklyCo2SavedKg, 2);

        Assert.NotNull(result.CurrentUser);
        Assert.Equal("Alice", result.CurrentUser!.Username);
        Assert.Equal(2, result.CurrentUser.Rank);
        Assert.Equal(1.92, result.CurrentUser.WeeklyCo2SavedKg, 2);
    }
}
