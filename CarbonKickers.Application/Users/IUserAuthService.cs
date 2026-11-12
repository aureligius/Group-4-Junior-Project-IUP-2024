namespace CarbonKickers.Application.Users;

public interface IUserAuthService
{
    Task<AuthOperationResult> RegisterAsync(RegisterRequest request);

    Task<AuthOperationResult> ValidateCredentialsAsync(LoginRequest request);
}