using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;

namespace CarbonKickers.Tests;

public sealed class TripHistoryServiceTests
{
    [Fact]
    public async Task GetForUserAsync_ReturnsOnlyTheCurrentUsersTrips()
    {
        await using var db = TestDb.Create();

        var bicycle = new TransportMode { ModeName = "Bicycle", EmissionFactorPerKm = 0 };

        var car = new TransportMode { ModeName = "Car", EmissionFactorPerKm = 0.192 };

        var alice = new User { Username = "Alice", Email = "alice@example.com" };

        var bob = new User { Username = "Bob", Email = "bob@example.com" };

        db.AddRange(bicycle, car, alice, bob);

        db.TripLogs.AddRange(
            new TripLog
            {
                UserId = alice.Id,
                Origin = "UGM",
                Destination = "Malioboro",
                DistanceKm = 3,
                BaselineDistanceKm = 3,
                TransportMode = bicycle,
                BaselineTransportMode = car,
            },
            new TripLog
            {
                UserId = bob.Id,
                Origin = "Tugu",
                Destination = "Airport",
                DistanceKm = 40,
                BaselineDistanceKm = 40,
                TransportMode = car,
                BaselineTransportMode = car,
            }
        );

        await db.SaveChangesAsync();

        var service = new TripHistoryService(db);

        var aliceTrips = await service.GetForUserAsync(alice.Id);

        Assert.Single(aliceTrips);
        Assert.Equal("UGM", aliceTrips[0].Origin);
        Assert.DoesNotContain(aliceTrips, trip => trip.Origin == "Tugu");
    }
}
