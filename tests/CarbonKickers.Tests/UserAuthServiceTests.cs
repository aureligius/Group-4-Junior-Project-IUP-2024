using CarbonKickers.Infrastructure.Services;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Tests;

public sealed class UserAuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserAndHashesPassword()
    {
        await using var db = TestDb.Create();

        var service = new UserAuthService(db);

        var result = await service.RegisterAsync(
            new RegisterRequest("Testing", "testing@example.com", "strong-password")
        );

        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);

        var savedUser = db.Users.Single();

        Assert.Equal("Testing", savedUser.Username);
        Assert.Equal("testing@example.com", savedUser.Email);

        Assert.NotEqual("strong-password", savedUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail()
    {
        await using var db = TestDb.Create();

        var service = new UserAuthService(db);

        var firstRegistration = await service.RegisterAsync(
            new RegisterRequest("TestingOne", "same@example.com", "strong-password")
        );

        var duplicateRegistration = await service.RegisterAsync(
            new RegisterRequest("TestingTwo", "same@example.com", "another-password")
        );

        Assert.True(firstRegistration.Succeeded);

        Assert.False(duplicateRegistration.Succeeded);

        Assert.Equal("This email is already registered.", duplicateRegistration.Error);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_AcceptsCorrectPassword()
    {
        await using var db = TestDb.Create();

        var service = new UserAuthService(db);

        await service.RegisterAsync(
            new RegisterRequest("Testing", "testing@example.com", "strong-password")
        );

        var result = await service.ValidateCredentialsAsync(
            new LoginRequest("testing@example.com", "strong-password")
        );

        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);
        Assert.Equal("Testing", result.User.Username);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_RejectsWrongPassword()
    {
        await using var db = TestDb.Create();

        var service = new UserAuthService(db);

        await service.RegisterAsync(
            new RegisterRequest("Testing", "testing@example.com", "strong-password")
        );

        var result = await service.ValidateCredentialsAsync(
            new LoginRequest("testing@example.com", "wrong-password")
        );

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email or password.", result.Error);
    }
}
