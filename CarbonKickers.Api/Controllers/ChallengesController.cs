using System.Security.Claims;
using CarbonKickers.Application.Challenges;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/challenges")]
public sealed class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challengeService;

    public ChallengesController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChallengeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ChallengeResponse>> Create(CreateChallengeRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var challenge = await _challengeService.CreateAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, challenge);
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

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<ChallengeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChallengeResponse>>> GetMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _challengeService.GetMineAsync(userId));
    }

    [HttpPost("{challengeId:guid}/accept")]
    public async Task<ActionResult<ChallengeResponse>> Accept(Guid challengeId)
    {
        return await Respond(challengeId, _challengeService.AcceptAsync);
    }

    [HttpPost("{challengeId:guid}/decline")]
    public async Task<ActionResult<ChallengeResponse>> Decline(Guid challengeId)
    {
        return await Respond(challengeId, _challengeService.DeclineAsync);
    }

    private async Task<ActionResult<ChallengeResponse>> Respond(
        Guid challengeId,
        Func<Guid, Guid, Task<ChallengeResponse>> responseAction
    )
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await responseAction(userId, challengeId));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
