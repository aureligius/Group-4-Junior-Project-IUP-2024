using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(Guid userId);
}