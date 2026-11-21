using System.ComponentModel.DataAnnotations;

namespace CarbonKickers.Shared.Contracts;

public sealed class CreateTripRequest
{
    [Required]
    [StringLength(300)]
    public string Origin { get; init; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OriginPlaceId { get; init; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Destination { get; init; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DestinationPlaceId { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TransportModeId { get; init; }

    [Range(1, int.MaxValue)]
    public int BaselineTransportModeId { get; init; }
}

public sealed record CreateTripResponse(
    Guid Id,
    string Origin,
    string Destination,
    double DistanceKm,
    string TransportModeName,
    string BaselineTransportModeName,
    double Co2EmittedKg,
    double Co2SavedKg,
    int XpEarned,
    DateTime LoggedAt
);
