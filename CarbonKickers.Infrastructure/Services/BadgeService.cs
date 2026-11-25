using CarbonKickers.Application.Badges;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class BadgeService : IBadgeService
{
    private readonly AppDbContext _db;

    public BadgeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BadgeResponse>> AwardEligibleBadgesAsync(
        User user)
    {
        var eligibleBadges = await _db.Badges
            .Where(badge => badge.XpRequirement <= user.GreenXp)
            .ToListAsync();

        var alreadyUnlockedIds = await _db.UserBadges
            .Where(userBadge => userBadge.UserId == user.Id)
            .Select(userBadge => userBadge.BadgeId)
            .ToHashSetAsync();

        var newlyUnlocked = new List<BadgeResponse>();

        foreach (var badge in eligibleBadges)
        {
            if (alreadyUnlockedIds.Contains(badge.Id))
            {
                continue;
            }

            var userBadge = new UserBadge
            {
                UserId = user.Id,
                BadgeId = badge.Id,
                UnlockedAt = DateTime.UtcNow
            };

            _db.UserBadges.Add(userBadge);

            newlyUnlocked.Add(new BadgeResponse(
                badge.Id,
                badge.Title,
                badge.Description,
                badge.XpRequirement,
                badge.IconUrl,
                userBadge.UnlockedAt));
        }

        return newlyUnlocked;
    }

    public async Task<IReadOnlyList<BadgeResponse>> GetUnlockedBadgesAsync(
        Guid userId)
    {
        return await (
            from userBadge in _db.UserBadges.AsNoTracking()
            join badge in _db.Badges.AsNoTracking()
                on userBadge.BadgeId equals badge.Id
            where userBadge.UserId == userId
            orderby userBadge.UnlockedAt descending
            select new BadgeResponse(
                badge.Id,
                badge.Title,
                badge.Description,
                badge.XpRequirement,
                badge.IconUrl,
                userBadge.UnlockedAt)
        ).ToListAsync();
    }
}