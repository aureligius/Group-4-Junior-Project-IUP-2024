using CarbonKickers.Application.Trips;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Interfaces;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class TripService : ITripService
{
    private readonly AppDbContext _db;
    private readonly IMapsService _mapsService;
    private readonly IEmissionService _emissionService;

    public TripService(AppDbContext db, IMapsService mapsService, IEmissionService emissionService)
    {
        _db = db;
        _mapsService = mapsService;
        _emissionService = emissionService;
    }

    public async Task<CreateTripResponse> CreateAsync(Guid userId, CreateTripRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User was not found.");

        var transportMode = await _db.TransportModes.FindAsync(request.TransportModeId)
            ?? throw new KeyNotFoundException("Selected transport mode was not found.");

        var baselineMode = await _db.TransportModes.FindAsync(request.BaselineTransportModeId)
            ?? throw new KeyNotFoundException("Baseline transport mode was not found.");

        var distanceKm = await _mapsService.CalculateDistanceAsync(
            request.OriginPlaceId.Trim(),
            request.DestinationPlaceId.Trim(),
            transportMode.ModeName
        );

        var trip = new TripLog
        {
            UserId = userId,
            Origin = request.Origin.Trim(),
            OriginPlaceId = request.OriginPlaceId.Trim(),
            Destination = request.Destination.Trim(),
            DestinationPlaceId = request.DestinationPlaceId.Trim(),
            DistanceKm = distanceKm,
            TransportModeId = transportMode.Id,
            TransportMode = transportMode,
            BaselineTransportModeId = baselineMode.Id,
            BaselineTransportMode = baselineMode,
        };

        trip.CalculateSavings(_emissionService);

        var xpEarned = (int)Math.Floor(trip.Co2SavedKg * 10);

        user.AddCo2Saved(trip.Co2SavedKg);
        user.AddXp(xpEarned);

        _db.TripLogs.Add(trip);
        await _db.SaveChangesAsync();

        return new CreateTripResponse(
            trip.Id,
            trip.Origin,
            trip.Destination,
            trip.DistanceKm,
            transportMode.ModeName,
            baselineMode.ModeName,
            trip.Co2EmittedKg,
            trip.Co2SavedKg,
            xpEarned,
            trip.LoggedAt
        );
    }
}
