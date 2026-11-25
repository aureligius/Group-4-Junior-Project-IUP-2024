using System.Security.Claims;
using CarbonKickers.Application.Leagues;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leagues")]
public sealed class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;

    public LeaguesController(ILeagueService leagueService)
    {
        _leagueService = leagueService;
    }

    [HttpGet("tiers")]
    [ProducesResponseType(typeof(IReadOnlyList<LeagueTierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeagueTierResponse>>> GetTiers()
    {
        return Ok(await _leagueService.GetTiersAsync());
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentLeagueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<CurrentLeagueResponse>> GetMyLeague()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var league = await _leagueService.GetCurrentLeagueAsync(userId);

            return league is null ? NoContent() : Ok(league);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
