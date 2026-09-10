namespace CarbonKickers.Application.Trips;

using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Interfaces;

public class CreateTripCommandHandler
{
    private readonly IMapsService _mapsService;
    private readonly IEmissionService _emissionService;

    public CreateTripCommandHandler(IMapsService mapsService, IEmissionService emissionService)
    {
        _mapsService = mapsService;
        _emissionService = emissionService;
    }

    public async Task<TripLog> ExecuteAsync(Guid userId, string origin, string destination, TransportMode mode, TransportMode baseline)
    {
        double distance = await _mapsService.CalculateDistanceAsync(origin, destination);

        var trip = new TripLog
        {
            UserId = userId,
            Origin = origin,
            Destination = destination,
            DistanceKm = distance,
            TransportMode = mode,
            BaselineTransportMode = baseline
        };

        trip.CalculateSavings(_emissionService);
        return trip;
    }
}