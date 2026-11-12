using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarbonKickers.Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CarbonKickers.Web.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserAuthService userAuthService,
        IConfiguration configuration)
    {
        _userAuthService = userAuthService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _userAuthService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Error });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Account created successfully."
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _userAuthService.ValidateCredentialsAsync(request);

        if (!result.Succeeded || result.User is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = CreateToken(result.User);

        return Ok(new LoginResponse(
            token,
            result.User.Username,
            result.User.Email));
    }

    private string CreateToken(AuthenticatedUser user)
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? throw new InvalidOperationException("JWT_KEY is missing.");

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? "CarbonKickersApi";

        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? "CarbonKickersWpf";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}