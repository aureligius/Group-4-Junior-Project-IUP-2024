using CarbonKickers.Application.Leaderboard;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _db;

    public LeaderboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WeeklyLeaderboardResponse> GetWeeklyAsync(
        Guid currentUserId,
        int maximumEntries = 10
    )
    {
        maximumEntries = Math.Clamp(maximumEntries, 1, 50);

        var now = DateTime.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
        var weekStartUtc = now.Date.AddDays(-daysSinceMonday);
        var weekEndUtc = weekStartUtc.AddDays(7);

        var tripData = await _db
            .TripLogs.AsNoTracking()
            .Where(trip => trip.LoggedAt >= weekStartUtc && trip.LoggedAt < weekEndUtc)
            .Join(
                _db.Users.AsNoTracking(),
                trip => trip.UserId,
                user => user.Id,
                (trip, user) =>
                    new
                    {
                        user.Id,
                        user.Username,
                        trip.Co2SavedKg,
                    }
            )
            .ToListAsync();

        var rankedEntries = tripData
            .GroupBy(item => new { item.Id, item.Username })
            .Select(group => new
            {
                UserId = group.Key.Id,
                group.Key.Username,
                WeeklyCo2SavedKg = group.Sum(item => item.Co2SavedKg),

                // Must match TripService's XP rule:
                // XP = floor(saved kg × 10) for each trip.
                WeeklyXp = group.Sum(item => (int)Math.Floor(item.Co2SavedKg * 10)),
            })
            .OrderByDescending(item => item.WeeklyCo2SavedKg)
            .ThenByDescending(item => item.WeeklyXp)
            .ThenBy(item => item.Username)
            .Select(
                (item, index) =>
                    new
                    {
                        Rank = index + 1,
                        item.UserId,
                        item.Username,
                        item.WeeklyCo2SavedKg,
                        item.WeeklyXp,
                    }
            )
            .ToList();

        var topEntries = rankedEntries
            .Take(maximumEntries)
            .Select(item => new LeaderboardEntryResponse(
                item.Rank,
                item.Username,
                item.WeeklyCo2SavedKg,
                item.WeeklyXp,
                item.UserId == currentUserId
            ))
            .ToList();

        var currentUserEntry = rankedEntries.FirstOrDefault(item => item.UserId == currentUserId);

        var currentUser = currentUserEntry is null
            ? null
            : new LeaderboardEntryResponse(
                currentUserEntry.Rank,
                currentUserEntry.Username,
                currentUserEntry.WeeklyCo2SavedKg,
                currentUserEntry.WeeklyXp,
                true
            );

        return new WeeklyLeaderboardResponse(weekStartUtc, weekEndUtc, topEntries, currentUser);
    }
}
