using CarbonKickers.Application.Leagues;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class LeagueService : ILeagueService
{
    private readonly AppDbContext _db;

    public LeagueService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LeagueTierResponse>> GetTiersAsync()
    {
        var now = DateTime.UtcNow;

        // Current weekly leagues are a snapshot. Do not let rule edits
        // change the tiers already being used this week.
        var currentWeekLeagues = await _db
            .Leagues.AsNoTracking()
            .Where(league => league.StartDate <= now && league.EndDate > now)
            .OrderBy(league => league.MinXp)
            .ToListAsync();

        if (currentWeekLeagues.Count > 0)
        {
            return currentWeekLeagues
                .Select(league => new LeagueTierResponse(
                    league.TierName,
                    league.MinXp,
                    league.MaxXp == int.MaxValue ? null : league.MaxXp
                ))
                .ToList();
        }

        // Before a week has been created, expose the editable future rules.
        return await _db
            .LeagueTierRules.AsNoTracking()
            .Where(rule => rule.IsActive)
            .OrderBy(rule => rule.MinXp)
            .Select(rule => new LeagueTierResponse(rule.TierName, rule.MinXp, rule.MaxXp))
            .ToListAsync();
    }

    public async Task JoinEligibleCurrentLeagueAsync(User user)
    {
        var now = DateTime.UtcNow;

        // A user stays in the first league they join during this week.
        var alreadyInActiveLeague = await _db
            .LeagueParticipants.Join(
                _db.Leagues,
                participant => participant.LeagueId,
                league => league.Id,
                (participant, league) =>
                    new
                    {
                        participant.UserId,
                        league.StartDate,
                        league.EndDate,
                    }
            )
            .AnyAsync(item =>
                item.UserId == user.Id && item.StartDate <= now && item.EndDate > now
            );

        if (alreadyInActiveLeague)
        {
            return;
        }

        var eligibleLeague = await _db
            .Leagues.Where(league =>
                league.StartDate <= now
                && league.EndDate > now
                && league.MinXp <= user.GreenXp
                && league.MaxXp >= user.GreenXp
            )
            .OrderByDescending(league => league.MinXp)
            .FirstOrDefaultAsync();

        if (eligibleLeague is null)
        {
            // The admin has not created the current week's leagues yet.
            return;
        }

        _db.LeagueParticipants.Add(
            new LeagueParticipant
            {
                UserId = user.Id,
                LeagueId = eligibleLeague.Id,
                Xp = user.GreenXp,
                Rank = 0,
                JoinedAt = now,
            }
        );
    }

    public async Task<CurrentLeagueResponse?> GetCurrentLeagueAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User was not found.");

        await JoinEligibleCurrentLeagueAsync(user);

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync();
        }

        var now = DateTime.UtcNow;

        var league = await _db
            .LeagueParticipants.Where(participant => participant.UserId == userId)
            .Join(
                _db.Leagues,
                participant => participant.LeagueId,
                item => item.Id,
                (participant, item) =>
                    new
                    {
                        item.Id,
                        item.TierName,
                        item.MinXp,
                        item.MaxXp,
                        item.StartDate,
                        item.EndDate,
                    }
            )
            .Where(item => item.StartDate <= now && item.EndDate > now)
            .OrderByDescending(item => item.MinXp)
            .FirstOrDefaultAsync();

        if (league is null)
        {
            return null;
        }

        var participants = await _db
            .LeagueParticipants.AsNoTracking()
            .Where(participant => participant.LeagueId == league.Id)
            .Join(
                _db.Users.AsNoTracking(),
                participant => participant.UserId,
                userItem => userItem.Id,
                (participant, userItem) => new { participant.UserId, userItem.Username }
            )
            .ToListAsync();

        var participantIds = participants.Select(item => item.UserId).ToList();

        var tripScores = await _db
            .TripLogs.AsNoTracking()
            .Where(trip =>
                participantIds.Contains(trip.UserId)
                && trip.LoggedAt >= league.StartDate
                && trip.LoggedAt < league.EndDate
            )
            .Select(trip => new { trip.UserId, trip.Co2SavedKg })
            .ToListAsync();

        var scoresByUser = tripScores
            .GroupBy(trip => trip.UserId)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Co2SavedKg = group.Sum(trip => trip.Co2SavedKg),
                    XpEarned = group.Sum(trip => (int)Math.Floor(trip.Co2SavedKg * 10)),
                }
            );

        var standings = participants
            .Select(participant =>
            {
                scoresByUser.TryGetValue(participant.UserId, out var score);

                return new
                {
                    participant.UserId,
                    participant.Username,
                    Co2SavedKg = score?.Co2SavedKg ?? 0,
                    XpEarned = score?.XpEarned ?? 0,
                };
            })
            .OrderByDescending(item => item.Co2SavedKg)
            .ThenByDescending(item => item.XpEarned)
            .ThenBy(item => item.Username)
            .Select(
                (item, index) =>
                    new LeagueStandingResponse(
                        index + 1,
                        item.Username,
                        item.Co2SavedKg,
                        item.XpEarned,
                        item.UserId == userId
                    )
            )
            .ToList();

        return new CurrentLeagueResponse(
            league.Id,
            league.TierName,
            league.MinXp,
            league.MaxXp,
            league.StartDate,
            league.EndDate,
            participants.Count,
            standings
        );
    }
}
