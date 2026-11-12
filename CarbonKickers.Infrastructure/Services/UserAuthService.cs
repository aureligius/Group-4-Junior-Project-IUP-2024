using CarbonKickers.Application.Users;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class UserAuthService : IUserAuthService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserAuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new(false, "All fields are required.", null);
        }

        if (username.Length is < 3 or > 50)
        {
            return new(false, "Username must contain 3 to 50 characters.", null);
        }

        if (request.Password.Length < 8)
        {
            return new(false, "Password must contain at least 8 characters.", null);
        }

        var emailInUse = await _db.Users.AnyAsync(user => user.Email == email);
        if (emailInUse)
        {
            return new(false, "This email is already registered.", null);
        }

        var usernameInUse = await _db.Users.AnyAsync(user => user.Username == username);
        if (usernameInUse)
        {
            return new(false, "This username is already taken.", null);
        }

        var user = new User
        {
            Username = username,
            Email = email
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Protects against a duplicate account created at the same time.
            return new(false, "Username or email is already registered.", null);
        }

        return new(
            true,
            null,
            new AuthenticatedUser(user.Id, user.Username, user.Email, user.Role));
    }

    public async Task<AuthOperationResult> ValidateCredentialsAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.SingleOrDefaultAsync(item => item.Email == email);

        if (user is null)
        {
            return new(false, "Invalid email or password.", null);
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return new(false, "Invalid email or password.", null);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync();
        }

        return new(
            true,
            null,
            new AuthenticatedUser(user.Id, user.Username, user.Email, user.Role));
    }
}