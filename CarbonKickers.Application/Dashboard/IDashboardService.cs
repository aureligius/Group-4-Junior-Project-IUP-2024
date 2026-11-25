using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Dashboard;

public interface IDashboardService
{
    /// <param name="utcOffsetMinutes">The caller's offset from UTC, used to decide which local day each trip belongs to.</param>
    Task<DashboardResponse> GetAsync(Guid userId, int utcOffsetMinutes = 0);
}