using CarbonKickers.Application.Admin;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(AdminSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminSummaryResponse>> GetSummary()
    {
        return Ok(await _adminService.GetSummaryAsync());
    }

    [HttpPost("leagues/create-current-week")]
    [ProducesResponseType(typeof(CreateWeeklyLeaguesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateWeeklyLeaguesResponse>> CreateCurrentWeekLeagues()
    {
        return Ok(await _adminService.CreateCurrentWeekLeaguesAsync());
    }

    [HttpGet("league-tier-rules")]
    public async Task<ActionResult<IReadOnlyList<LeagueTierRuleAdminResponse>>> GetLeagueTierRules()
    {
        return Ok(await _adminService.GetLeagueTierRulesAsync());
    }

    [HttpPost("league-tier-rules")]
    public async Task<ActionResult<LeagueTierRuleAdminResponse>> CreateLeagueTierRule(
        SaveLeagueTierRuleRequest request
    )
    {
        try
        {
            var rule = await _adminService.CreateLeagueTierRuleAsync(request);

            return StatusCode(StatusCodes.Status201Created, rule);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("league-tier-rules/{ruleId:int}")]
    public async Task<ActionResult<LeagueTierRuleAdminResponse>> UpdateLeagueTierRule(
        int ruleId,
        SaveLeagueTierRuleRequest request
    )
    {
        try
        {
            return Ok(await _adminService.UpdateLeagueTierRuleAsync(ruleId, request));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("badges")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminBadgeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminBadgeResponse>>> GetBadges()
    {
        return Ok(await _adminService.GetBadgesAsync());
    }

    [HttpPost("badges")]
    [ProducesResponseType(typeof(AdminBadgeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminBadgeResponse>> CreateBadge(SaveBadgeRequest request)
    {
        try
        {
            var badge = await _adminService.CreateBadgeAsync(request);

            return StatusCode(StatusCodes.Status201Created, badge);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("badges/{badgeId:int}")]
    [ProducesResponseType(typeof(AdminBadgeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminBadgeResponse>> UpdateBadge(
        int badgeId,
        SaveBadgeRequest request
    )
    {
        try
        {
            return Ok(await _adminService.UpdateBadgeAsync(badgeId, request));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("transport-modes")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AdminTransportModeResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IReadOnlyList<AdminTransportModeResponse>>> GetTransportModes()
    {
        return Ok(await _adminService.GetTransportModesAsync());
    }

    [HttpPost("transport-modes")]
    [ProducesResponseType(typeof(AdminTransportModeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminTransportModeResponse>> CreateTransportMode(
        SaveTransportModeRequest request
    )
    {
        try
        {
            var mode = await _adminService.CreateTransportModeAsync(request);

            return StatusCode(StatusCodes.Status201Created, mode);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("transport-modes/{transportModeId:int}")]
    [ProducesResponseType(typeof(AdminTransportModeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminTransportModeResponse>> UpdateTransportMode(
        int transportModeId,
        SaveTransportModeRequest request
    )
    {
        try
        {
            return Ok(await _adminService.UpdateTransportModeAsync(transportModeId, request));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
