using CarbonKickers.Domain.Entities;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Leagues;

public interface ILeagueService
{
    Task JoinEligibleCurrentLeagueAsync(User user);

    Task<CurrentLeagueResponse?> GetCurrentLeagueAsync(
        Guid userId);
}