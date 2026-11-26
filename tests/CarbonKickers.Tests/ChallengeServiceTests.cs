using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Services;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Tests;

public sealed class ChallengeServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsSelfChallenge()
    {
        await using var db = TestDb.Create();

        var user = new User
        {
            Username = "Testing",
            Email = "testing@example.com"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new ChallengeService(db);

        var request = new CreateChallengeRequest
        {
            OpponentUsername = "Testing",
            StartDate = DateTimeOffset.UtcNow.AddHours(1),
            EndDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(user.Id, request));

        Assert.Equal(
            "You cannot challenge yourself.",
            exception.Message);
    }

    [Fact]
    public async Task AcceptAsync_OnlyAllowsTheOpponent()
    {
        await using var db = TestDb.Create();

        var challenger = new User
        {
            Username = "Challenger",
            Email = "challenger@example.com"
        };

        var opponent = new User
        {
            Username = "Opponent",
            Email = "opponent@example.com"
        };

        db.Users.AddRange(challenger, opponent);
        await db.SaveChangesAsync();

        var service = new ChallengeService(db);

        var createdChallenge = await service.CreateAsync(
            challenger.Id,
            new CreateChallengeRequest
            {
                OpponentUsername = "Opponent",
                StartDate = DateTimeOffset.UtcNow.AddHours(1),
                EndDate = DateTimeOffset.UtcNow.AddDays(7)
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AcceptAsync(
                challenger.Id,
                createdChallenge.Id));

        var acceptedChallenge = await service.AcceptAsync(
            opponent.Id,
            createdChallenge.Id);

        Assert.Equal("Accepted", acceptedChallenge.Status);
    }

    [Fact]
    public async Task CreateAsync_RejectsSecondActiveChallengeWithSameUser()
    {
        await using var db = TestDb.Create();

        var challenger = new User
        {
            Username = "Challenger",
            Email = "challenger@example.com"
        };

        var opponent = new User
        {
            Username = "Opponent",
            Email = "opponent@example.com"
        };

        db.Users.AddRange(challenger, opponent);
        await db.SaveChangesAsync();

        var service = new ChallengeService(db);

        var request = new CreateChallengeRequest
        {
            OpponentUsername = "Opponent",
            StartDate = DateTimeOffset.UtcNow.AddHours(1),
            EndDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        await service.CreateAsync(challenger.Id, request);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(challenger.Id, request));

        Assert.Equal(
            "You already have a pending or active challenge with this user.",
            exception.Message);
    }
}