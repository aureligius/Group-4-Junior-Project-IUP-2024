using CarbonKickers.Application.Trips;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class TripHistoryService : ITripHistoryService
{
    private readonly AppDbContext _db;

    public TripHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TripHistoryResponse>> GetForUserAsync(
        Guid userId)
    {
        return await _db.TripLogs
            .AsNoTracking()
            .Where(trip => trip.UserId == userId)
            .OrderByDescending(trip => trip.LoggedAt)
            .Select(trip => new TripHistoryResponse(
                trip.Id,
                trip.Origin,
                trip.Destination,
                trip.TransportMode.ModeName,
                trip.BaselineTransportMode.ModeName,
                trip.DistanceKm,
                trip.Co2EmittedKg,
                trip.Co2SavedKg,
                (int)Math.Floor(trip.Co2SavedKg * 10),
                trip.LoggedAt))
            .ToListAsync();
    }
}