using CarbonKickers.Application.Dashboard;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponse> GetAsync(Guid userId, int utcOffsetMinutes = 0)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Username,
                item.GreenXp,
                item.TotalCo2SavedKg
            })
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("User was not found.");

        // Trips are stored in UTC; shift them into the user's local time so each
        // trip lands on the calendar day the user actually made it.
        var offset = TimeSpan.FromMinutes(utcOffsetMinutes);

        var today = DateOnly.FromDateTime(DateTime.UtcNow + offset);
        var firstDate = today.AddDays(-6);

        // Local midnight at the start of the window, expressed in UTC.
        var windowStartUtc = DateTime.SpecifyKind(
            firstDate.ToDateTime(TimeOnly.MinValue) - offset,
            DateTimeKind.Utc);

        // At most one week of one user's trips, so grouping in memory is cheap.
        var recentTrips = await _db.TripLogs
            .AsNoTracking()
            .Where(trip =>
                trip.UserId == userId &&
                trip.LoggedAt >= windowStartUtc)
            .Select(trip => new { trip.LoggedAt, trip.Co2SavedKg })
            .ToListAsync();

        var dailyByDate = recentTrips
            .GroupBy(trip => DateOnly.FromDateTime(trip.LoggedAt + offset))
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Co2SavedKg = group.Sum(trip => trip.Co2SavedKg),
                    TripCount = group.Count()
                });

        var lastSevenDays = Enumerable.Range(0, 7)
            .Select(dayIndex => firstDate.AddDays(dayIndex))
            .Select(date =>
            {
                if (dailyByDate.TryGetValue(date, out var total))
                {
                    return new DailyImpactResponse(
                        date,
                        total.Co2SavedKg,
                        total.TripCount);
                }

                return new DailyImpactResponse(date, 0, 0);
            })
            .ToList();

        var totalTrips = await _db.TripLogs
            .AsNoTracking()
            .CountAsync(trip => trip.UserId == userId);

        return new DashboardResponse(
            user.Username,
            user.GreenXp,
            user.TotalCo2SavedKg,
            totalTrips,
            lastSevenDays);
    }
}