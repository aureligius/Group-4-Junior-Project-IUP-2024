using CarbonKickers.Domain.Entities;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Badges;

public interface IBadgeService
{
    Task<IReadOnlyList<BadgeResponse>> AwardEligibleBadgesAsync(
        User user);

    Task<IReadOnlyList<BadgeResponse>> GetUnlockedBadgesAsync(
        Guid userId);
}