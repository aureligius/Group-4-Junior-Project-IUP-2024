namespace CarbonKickers.Shared.Contracts;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AuthenticatedUser(
    Guid Id,
    string Username,
    string Email,
    string Role);

public sealed record AuthOperationResult(
    bool Succeeded,
    string? Error,
    AuthenticatedUser? User);

public sealed record LoginResponse(
    string Token,
    string Username,
    string Email);
