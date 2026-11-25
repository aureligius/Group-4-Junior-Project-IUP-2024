using CarbonKickers.Application.Badges;
using CarbonKickers.Application.Leagues;
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
    private readonly IBadgeService _badgeService;
    private readonly ILeagueService _leagueService;

    public TripService(
        AppDbContext db,
        IMapsService mapsService,
        IEmissionService emissionService,
        IBadgeService badgeService,
        ILeagueService leagueService
    )
    {
        _db = db;
        _mapsService = mapsService;
        _emissionService = emissionService;
        _badgeService = badgeService;
        _leagueService = leagueService;
    }

    public async Task<CreateTripResponse> CreateAsync(Guid userId, CreateTripRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User was not found.");

        var transportMode = await _db.TransportModes.SingleOrDefaultAsync(mode =>
            mode.Id == request.TransportModeId && mode.IsActive
        )
            ?? throw new KeyNotFoundException(
                "Selected transport mode was not found or is inactive."
            );

        var baselineMode = await _db.TransportModes.SingleOrDefaultAsync(mode =>
            mode.Id == request.BaselineTransportModeId && mode.IsActive
        )
            ?? throw new KeyNotFoundException(
                "Baseline transport mode was not found or is inactive."
            );

        var actualRoute = await _mapsService.CalculateDistanceAsync(
            request.OriginPlaceId.Trim(),
            request.DestinationPlaceId.Trim(),
            transportMode.ModeName
        );

        var baselineRoute = await _mapsService.CalculateDistanceAsync(
            request.OriginPlaceId.Trim(),
            request.DestinationPlaceId.Trim(),
            baselineMode.ModeName
        );

        var trip = new TripLog
        {
            UserId = userId,
            Origin = request.Origin.Trim(),
            OriginPlaceId = request.OriginPlaceId.Trim(),
            Destination = request.Destination.Trim(),
            DestinationPlaceId = request.DestinationPlaceId.Trim(),
            DistanceKm = actualRoute.DistanceKm,
            BaselineDistanceKm = baselineRoute.DistanceKm,
            ActualRouteMode = actualRoute.RouteModeUsed,
            ActualDistanceIsEstimated = actualRoute.IsEstimated,
            BaselineRouteMode = baselineRoute.RouteModeUsed,
            BaselineDistanceIsEstimated = baselineRoute.IsEstimated,
            TransportModeId = transportMode.Id,
            TransportMode = transportMode,
            BaselineTransportModeId = baselineMode.Id,
            BaselineTransportMode = baselineMode,
        };

        trip.CalculateSavings(_emissionService);

        var xpEarned = (int)Math.Floor(trip.Co2SavedKg * 10);

        user.AddCo2Saved(trip.Co2SavedKg);
        user.AddXp(xpEarned);

        var newlyUnlockedBadges = await _badgeService.AwardEligibleBadgesAsync(user);

        await _leagueService.JoinEligibleCurrentLeagueAsync(user);

        _db.TripLogs.Add(trip);
        await _db.SaveChangesAsync();

        return new CreateTripResponse(
            trip.Id,
            trip.Origin,
            trip.Destination,
            trip.DistanceKm,
            trip.BaselineDistanceKm,
            trip.ActualRouteMode,
            trip.ActualDistanceIsEstimated,
            trip.BaselineRouteMode,
            trip.BaselineDistanceIsEstimated,
            transportMode.ModeName,
            baselineMode.ModeName,
            trip.Co2EmittedKg,
            trip.BaselineCo2EmittedKg,
            trip.Co2SavedKg,
            xpEarned,
            newlyUnlockedBadges,
            trip.LoggedAt
        );
    }
}
