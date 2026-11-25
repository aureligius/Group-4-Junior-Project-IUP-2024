using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Challenges;

public interface IChallengeService
{
    Task<ChallengeResponse> CreateAsync(Guid challengerId, CreateChallengeRequest request);

    Task<IReadOnlyList<ChallengeResponse>> GetMineAsync(Guid userId);

    Task<ChallengeResponse> AcceptAsync(Guid opponentId, Guid challengeId);

    Task<ChallengeResponse> DeclineAsync(Guid opponentId, Guid challengeId);
}
