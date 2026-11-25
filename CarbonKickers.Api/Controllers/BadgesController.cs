using System.Security.Claims;
using CarbonKickers.Application.Badges;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/badges")]
public sealed class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgesController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    [HttpGet("me")]
    [ProducesResponseType(
        typeof(IReadOnlyList<BadgeResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BadgeResponse>>> GetMine()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var badges = await _badgeService
            .GetUnlockedBadgesAsync(userId);

        return Ok(badges);
    }
}