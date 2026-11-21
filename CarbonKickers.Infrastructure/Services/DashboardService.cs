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

    public async Task<DashboardResponse> GetAsync(Guid userId)
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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstDate = today.AddDays(-6);
        var firstDateTime = firstDate.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc);

        var dailyTotals = await _db.TripLogs
            .AsNoTracking()
            .Where(trip =>
                trip.UserId == userId &&
                trip.LoggedAt >= firstDateTime)
            .GroupBy(trip => trip.LoggedAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                Co2SavedKg = group.Sum(trip => trip.Co2SavedKg),
                TripCount = group.Count()
            })
            .ToListAsync();

        var dailyByDate = dailyTotals.ToDictionary(
            item => DateOnly.FromDateTime(item.Date),
            item => item);

        var lastSevenDays = Enumerable.Range(0, 7)
            .Select(offset => firstDate.AddDays(offset))
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