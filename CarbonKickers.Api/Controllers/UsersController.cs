using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarbonKickers.Application.Users;
using CarbonKickers.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonKickers.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserSearchService _userSearchService;

    public UsersController(IUserSearchService userSearchService)
    {
        _userSearchService = userSearchService;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSearchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserSearchResponse>>> Search(
        [FromQuery, Required, StringLength(50, MinimumLength = 2)] string query
    )
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var users = await _userSearchService.SearchAsync(userId, query);

        return Ok(users);
    }
}
