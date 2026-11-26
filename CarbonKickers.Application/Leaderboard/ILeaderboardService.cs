using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Leaderboard;

public interface ILeaderboardService
{
    Task<WeeklyLeaderboardResponse> GetWeeklyAsync(Guid currentUserId, int maximumEntries = 10);
}
