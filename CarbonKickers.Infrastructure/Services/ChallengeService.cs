using CarbonKickers.Application.Challenges;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Enums;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class ChallengeService : IChallengeService
{
    private readonly AppDbContext _db;

    public ChallengeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ChallengeResponse> CreateAsync(
        Guid challengerId,
        CreateChallengeRequest request
    )
    {
        var challenger = await _db.Users.FindAsync(challengerId)
            ?? throw new KeyNotFoundException("Challenger was not found.");

        var opponentUsername = request.OpponentUsername.Trim();

        var opponent = await _db.Users.SingleOrDefaultAsync(user =>
            user.Username.ToLower() == opponentUsername.ToLower()
        ) ?? throw new KeyNotFoundException("No user exists with that username.");

        if (opponent.Id == challengerId)
        {
            throw new InvalidOperationException("You cannot challenge yourself.");
        }

        var startDate = request.StartDate.UtcDateTime;
        var endDate = request.EndDate.UtcDateTime;
        var now = DateTime.UtcNow;

        if (startDate < now)
        {
            throw new InvalidOperationException("The challenge start date must be in the future.");
        }

        if (endDate <= startDate)
        {
            throw new InvalidOperationException(
                "The challenge end date must be after its start date."
            );
        }

        if (endDate > startDate.AddDays(30))
        {
            throw new InvalidOperationException("A challenge can last at most 30 days.");
        }

        var existingChallenge = await _db.Challenges.AnyAsync(challenge =>
            (
                challenge.Status == ChallengeStatus.Pending
                || challenge.Status == ChallengeStatus.Accepted
            )
            && (
                (challenge.ChallengerId == challengerId && challenge.OpponentId == opponent.Id)
                || (challenge.ChallengerId == opponent.Id && challenge.OpponentId == challengerId)
            )
        );

        if (existingChallenge)
        {
            throw new InvalidOperationException(
                "You already have a pending or active challenge with this user."
            );
        }

        var challenge = new Challenge
        {
            ChallengerId = challengerId,
            OpponentId = opponent.Id,
            StartDate = startDate,
            EndDate = endDate,
            Status = ChallengeStatus.Pending,
        };

        _db.Challenges.Add(challenge);
        await _db.SaveChangesAsync();

        return ToResponse(challenge, challenger.Username, opponent.Username, null, challengerId);
    }

    public async Task<IReadOnlyList<ChallengeResponse>> GetMineAsync(Guid userId)
    {
        var challenges = await _db
            .Challenges.Where(challenge =>
                challenge.ChallengerId == userId || challenge.OpponentId == userId
            )
            .OrderByDescending(challenge => challenge.StartDate)
            .ToListAsync();

        foreach (var challenge in challenges)
        {
            await RefreshChallengeAsync(challenge);
        }

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync();
        }

        var userIds = challenges
            .SelectMany(challenge =>
                new[] { challenge.ChallengerId, challenge.OpponentId, challenge.WinnerId }
            )
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var usernames = await _db
            .Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Username);

        return challenges
            .Select(challenge =>
                ToResponse(
                    challenge,
                    usernames[challenge.ChallengerId],
                    usernames[challenge.OpponentId],
                    challenge.WinnerId is null ? null : usernames[challenge.WinnerId.Value],
                    userId
                )
            )
            .ToList();
    }

    public async Task<ChallengeResponse> AcceptAsync(Guid opponentId, Guid challengeId)
    {
        var challenge = await _db.Challenges.FindAsync(challengeId)
            ?? throw new KeyNotFoundException("Challenge was not found.");

        if (challenge.OpponentId != opponentId)
        {
            throw new UnauthorizedAccessException(
                "Only the challenged user can accept this challenge."
            );
        }

        if (challenge.Status != ChallengeStatus.Pending)
        {
            throw new InvalidOperationException("Only pending challenges can be accepted.");
        }

        if (challenge.EndDate <= DateTime.UtcNow)
        {
            challenge.Status = ChallengeStatus.Expired;
            await _db.SaveChangesAsync();

            throw new InvalidOperationException("This challenge has already expired.");
        }

        challenge.AcceptChallenge();
        await _db.SaveChangesAsync();

        return await GetOneAsync(challenge, opponentId);
    }

    public async Task<ChallengeResponse> DeclineAsync(Guid opponentId, Guid challengeId)
    {
        var challenge = await _db.Challenges.FindAsync(challengeId)
            ?? throw new KeyNotFoundException("Challenge was not found.");

        if (challenge.OpponentId != opponentId)
        {
            throw new UnauthorizedAccessException(
                "Only the challenged user can decline this challenge."
            );
        }

        if (challenge.Status != ChallengeStatus.Pending)
        {
            throw new InvalidOperationException("Only pending challenges can be declined.");
        }

        challenge.Status = ChallengeStatus.Declined;
        await _db.SaveChangesAsync();

        return await GetOneAsync(challenge, opponentId);
    }

    private async Task RefreshChallengeAsync(Challenge challenge)
    {
        var now = DateTime.UtcNow;

        if (challenge.Status == ChallengeStatus.Pending && challenge.EndDate <= now)
        {
            challenge.Status = ChallengeStatus.Expired;
            return;
        }

        if (challenge.Status != ChallengeStatus.Accepted)
        {
            return;
        }

        var relevantTrips = await _db
            .TripLogs.AsNoTracking()
            .Where(trip =>
                (trip.UserId == challenge.ChallengerId || trip.UserId == challenge.OpponentId)
                && trip.LoggedAt >= challenge.StartDate
                && trip.LoggedAt < challenge.EndDate
            )
            .Select(trip => new { trip.UserId, trip.Co2SavedKg })
            .ToListAsync();

        challenge.ChallengerSavedKg = relevantTrips
            .Where(trip => trip.UserId == challenge.ChallengerId)
            .Sum(trip => trip.Co2SavedKg);

        challenge.OpponentSavedKg = relevantTrips
            .Where(trip => trip.UserId == challenge.OpponentId)
            .Sum(trip => trip.Co2SavedKg);

        if (challenge.EndDate <= now)
        {
            // A tie goes to the challenger under the existing domain rule.
            challenge.ResolveWinner();
        }
    }

    private async Task<ChallengeResponse> GetOneAsync(Challenge challenge, Guid currentUserId)
    {
        await RefreshChallengeAsync(challenge);

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync();
        }

        var userIds = new[] { challenge.ChallengerId, challenge.OpponentId, challenge.WinnerId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var usernames = await _db
            .Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Username);

        return ToResponse(
            challenge,
            usernames[challenge.ChallengerId],
            usernames[challenge.OpponentId],
            challenge.WinnerId is null ? null : usernames[challenge.WinnerId.Value],
            currentUserId
        );
    }

    private static ChallengeResponse ToResponse(
        Challenge challenge,
        string challengerUsername,
        string opponentUsername,
        string? winnerUsername,
        Guid currentUserId
    )
    {
        return new ChallengeResponse(
            challenge.Id,
            challengerUsername,
            opponentUsername,
            challenge.StartDate,
            challenge.EndDate,
            challenge.ChallengerSavedKg,
            challenge.OpponentSavedKg,
            challenge.Status.ToString(),
            winnerUsername,
            challenge.ChallengerId == currentUserId
        );
    }
}
