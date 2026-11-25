using CarbonKickers.Application.Admin;
using CarbonKickers.Domain.Entities;
using CarbonKickers.Domain.Enums;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AdminSummaryResponse> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _db.Users.CountAsync();
        var totalTrips = await _db.TripLogs.CountAsync();
        var totalBadgesUnlocked = await _db.UserBadges.CountAsync();

        var totalCo2SavedKg = await _db.TripLogs.Select(trip => (double?)trip.Co2SavedKg).SumAsync()
            ?? 0;

        var activeChallenges = await _db.Challenges.CountAsync(challenge =>
            challenge.Status == ChallengeStatus.Pending
            || challenge.Status == ChallengeStatus.Accepted
        );

        var activeLeagues = await _db.Leagues.CountAsync(league =>
            league.StartDate <= now && league.EndDate > now
        );

        return new AdminSummaryResponse(
            totalUsers,
            totalTrips,
            totalCo2SavedKg,
            totalBadgesUnlocked,
            activeChallenges,
            activeLeagues
        );
    }

    public async Task<CreateWeeklyLeaguesResponse> CreateCurrentWeekLeaguesAsync()
    {
        var now = DateTime.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;

        var weekStartUtc = now.Date.AddDays(-daysSinceMonday);
        var weekEndUtc = weekStartUtc.AddDays(7);

        var definitions = await _db
            .LeagueTierRules.Where(rule => rule.IsActive)
            .OrderBy(rule => rule.MinXp)
            .ToListAsync();

        ValidateLeagueTierRules(definitions);

        foreach (var definition in definitions)
        {
            var exists = await _db.Leagues.AnyAsync(league =>
                league.TierName == definition.TierName
                && league.StartDate == weekStartUtc
                && league.EndDate == weekEndUtc
            );

            if (exists)
            {
                continue;
            }

            _db.Leagues.Add(
                new League
                {
                    TierName = definition.TierName,
                    MinXp = definition.MinXp,
                    MaxXp = definition.MaxXp ?? int.MaxValue,
                    StartDate = weekStartUtc,
                    EndDate = weekEndUtc,
                }
            );
        }

        await _db.SaveChangesAsync();

        var leagues = await _db
            .Leagues.AsNoTracking()
            .Where(league => league.StartDate == weekStartUtc && league.EndDate == weekEndUtc)
            .OrderBy(league => league.MinXp)
            .Select(league => new LeagueAdminResponse(
                league.Id,
                league.TierName,
                league.MinXp,
                league.MaxXp,
                league.StartDate,
                league.EndDate
            ))
            .ToListAsync();

        return new CreateWeeklyLeaguesResponse(weekStartUtc, weekEndUtc, leagues);
    }

    public async Task<IReadOnlyList<LeagueTierRuleAdminResponse>> GetLeagueTierRulesAsync()
    {
        return await _db
            .LeagueTierRules.AsNoTracking()
            .OrderBy(rule => rule.MinXp)
            .Select(rule => new LeagueTierRuleAdminResponse(
                rule.Id,
                rule.TierName,
                rule.MinXp,
                rule.MaxXp,
                rule.IsActive
            ))
            .ToListAsync();
    }

    public async Task<LeagueTierRuleAdminResponse> CreateLeagueTierRuleAsync(
        SaveLeagueTierRuleRequest request
    )
    {
        ValidateRuleValues(request);

        var name = request.TierName.Trim();

        var exists = await _db.LeagueTierRules.AnyAsync(rule =>
            rule.TierName.ToLower() == name.ToLower()
        );

        if (exists)
        {
            throw new InvalidOperationException("A league tier with that name already exists.");
        }

        var rule = new LeagueTierRule
        {
            TierName = name,
            MinXp = request.MinXp,
            MaxXp = request.MaxXp,
            IsActive = request.IsActive,
        };

        _db.LeagueTierRules.Add(rule);
        await _db.SaveChangesAsync();

        return ToLeagueTierRuleResponse(rule);
    }

    public async Task<LeagueTierRuleAdminResponse> UpdateLeagueTierRuleAsync(
        int ruleId,
        SaveLeagueTierRuleRequest request
    )
    {
        ValidateRuleValues(request);

        var rule = await _db.LeagueTierRules.FindAsync(ruleId)
            ?? throw new KeyNotFoundException("League tier rule was not found.");

        var name = request.TierName.Trim();

        var usedByAnotherRule = await _db.LeagueTierRules.AnyAsync(item =>
            item.Id != ruleId && item.TierName.ToLower() == name.ToLower()
        );

        if (usedByAnotherRule)
        {
            throw new InvalidOperationException("A league tier with that name already exists.");
        }

        rule.TierName = name;
        rule.MinXp = request.MinXp;
        rule.MaxXp = request.MaxXp;
        rule.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return ToLeagueTierRuleResponse(rule);
    }

    public async Task<IReadOnlyList<AdminBadgeResponse>> GetBadgesAsync()
    {
        return await _db
            .Badges.AsNoTracking()
            .OrderBy(badge => badge.XpRequirement)
            .ThenBy(badge => badge.Title)
            .Select(badge => new AdminBadgeResponse(
                badge.Id,
                badge.Title,
                badge.Description,
                badge.XpRequirement,
                badge.IconUrl
            ))
            .ToListAsync();
    }

    public async Task<AdminBadgeResponse> CreateBadgeAsync(SaveBadgeRequest request)
    {
        var title = request.Title.Trim();

        var exists = await _db.Badges.AnyAsync(badge => badge.Title.ToLower() == title.ToLower());

        if (exists)
        {
            throw new InvalidOperationException("A badge with that title already exists.");
        }

        var badge = new Badge
        {
            Title = title,
            Description = request.Description.Trim(),
            XpRequirement = request.XpRequirement,
            IconUrl = request.IconUrl.Trim(),
        };

        _db.Badges.Add(badge);
        await _db.SaveChangesAsync();

        return ToAdminBadgeResponse(badge);
    }

    public async Task<AdminBadgeResponse> UpdateBadgeAsync(int badgeId, SaveBadgeRequest request)
    {
        var badge = await _db.Badges.FindAsync(badgeId)
            ?? throw new KeyNotFoundException("Badge was not found.");

        var title = request.Title.Trim();

        var titleUsedByAnotherBadge = await _db.Badges.AnyAsync(item =>
            item.Id != badgeId && item.Title.ToLower() == title.ToLower()
        );

        if (titleUsedByAnotherBadge)
        {
            throw new InvalidOperationException("A badge with that title already exists.");
        }

        badge.Title = title;
        badge.Description = request.Description.Trim();
        badge.XpRequirement = request.XpRequirement;
        badge.IconUrl = request.IconUrl.Trim();

        await _db.SaveChangesAsync();

        return ToAdminBadgeResponse(badge);
    }

    public async Task<IReadOnlyList<AdminTransportModeResponse>> GetTransportModesAsync()
    {
        return await _db
            .TransportModes.AsNoTracking()
            .OrderBy(mode => mode.ModeName)
            .Select(mode => new AdminTransportModeResponse(
                mode.Id,
                mode.ModeName,
                mode.EmissionFactorPerKm,
                mode.IsActive
            ))
            .ToListAsync();
    }

    public async Task<AdminTransportModeResponse> CreateTransportModeAsync(
        SaveTransportModeRequest request
    )
    {
        var name = request.Name.Trim();

        var exists = await _db.TransportModes.AnyAsync(mode =>
            mode.ModeName.ToLower() == name.ToLower()
        );

        if (exists)
        {
            throw new InvalidOperationException("A transport mode with that name already exists.");
        }

        var mode = new TransportMode
        {
            ModeName = name,
            EmissionFactorPerKm = request.EmissionFactorPerKm,
            IsActive = request.IsActive,
        };

        _db.TransportModes.Add(mode);
        await _db.SaveChangesAsync();

        return ToAdminTransportModeResponse(mode);
    }

    public async Task<AdminTransportModeResponse> UpdateTransportModeAsync(
        int transportModeId,
        SaveTransportModeRequest request
    )
    {
        var mode = await _db.TransportModes.FindAsync(transportModeId)
            ?? throw new KeyNotFoundException("Transport mode was not found.");

        var name = request.Name.Trim();

        var nameUsedByAnotherMode = await _db.TransportModes.AnyAsync(item =>
            item.Id != transportModeId && item.ModeName.ToLower() == name.ToLower()
        );

        if (nameUsedByAnotherMode)
        {
            throw new InvalidOperationException("A transport mode with that name already exists.");
        }

        mode.ModeName = name;
        mode.EmissionFactorPerKm = request.EmissionFactorPerKm;
        mode.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return ToAdminTransportModeResponse(mode);
    }

    private static AdminTransportModeResponse ToAdminTransportModeResponse(TransportMode mode)
    {
        return new AdminTransportModeResponse(
            mode.Id,
            mode.ModeName,
            mode.EmissionFactorPerKm,
            mode.IsActive
        );
    }

    private static AdminBadgeResponse ToAdminBadgeResponse(Badge badge)
    {
        return new AdminBadgeResponse(
            badge.Id,
            badge.Title,
            badge.Description,
            badge.XpRequirement,
            badge.IconUrl
        );
    }

    private static void ValidateRuleValues(SaveLeagueTierRuleRequest request)
    {
        if (request.MaxXp is not null && request.MaxXp < request.MinXp)
        {
            throw new InvalidOperationException(
                "Maximum XP must be greater than or equal to minimum XP."
            );
        }
    }

    private static void ValidateLeagueTierRules(IReadOnlyList<LeagueTierRule> rules)
    {
        if (rules.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one active league tier rule is required."
            );
        }

        if (rules[0].MinXp != 0)
        {
            throw new InvalidOperationException("The first active tier must start at 0 XP.");
        }

        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];

            if (rule.MaxXp is not null && rule.MaxXp < rule.MinXp)
            {
                throw new InvalidOperationException(
                    $"The {rule.TierName} tier has an invalid XP range."
                );
            }

            var isLastRule = index == rules.Count - 1;

            if (!isLastRule)
            {
                if (rule.MaxXp is null)
                {
                    throw new InvalidOperationException(
                        "Only the final active tier can have no maximum XP."
                    );
                }

                var nextRule = rules[index + 1];

                if (nextRule.MinXp != rule.MaxXp.Value + 1)
                {
                    throw new InvalidOperationException(
                        "Active tier ranges must be continuous, with no gaps or overlaps."
                    );
                }
            }
            else if (rule.MaxXp is not null)
            {
                throw new InvalidOperationException(
                    "The final active tier must have no maximum XP."
                );
            }
        }
    }

    private static LeagueTierRuleAdminResponse ToLeagueTierRuleResponse(LeagueTierRule rule)
    {
        return new LeagueTierRuleAdminResponse(
            rule.Id,
            rule.TierName,
            rule.MinXp,
            rule.MaxXp,
            rule.IsActive
        );
    }
}
