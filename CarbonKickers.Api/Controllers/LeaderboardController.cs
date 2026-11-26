using System.Security.Claims;
using CarbonKickers.Application.Leaderboard;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leaderboard")]
public sealed class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet("weekly")]
    [ProducesResponseType(typeof(WeeklyLeaderboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeeklyLeaderboardResponse>> GetWeekly(
        [FromQuery] int maximumEntries = 10
    )
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var leaderboard = await _leaderboardService.GetWeeklyAsync(userId, maximumEntries);

        return Ok(leaderboard);
    }
}
