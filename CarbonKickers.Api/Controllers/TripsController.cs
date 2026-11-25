using System.Security.Claims;
using CarbonKickers.Application.Trips;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trips")]
public sealed class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    private readonly ITripHistoryService _tripHistoryService;

    public TripsController(ITripService tripService, ITripHistoryService tripHistoryService)
    {
        _tripService = tripService;
        _tripHistoryService = tripHistoryService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateTripResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateTripResponse>> Create(CreateTripRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(
                new { message = "The access token does not contain a valid user ID." }
            );
        }

        try
        {
            var trip = await _tripService.CreateAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, trip);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Unable to calculate the route distance. Please try again." }
            );
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = exception.Message }
            );
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TripHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TripHistoryResponse>>> GetAll()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var trips = await _tripHistoryService.GetForUserAsync(userId);

        return Ok(trips);
    }
}
