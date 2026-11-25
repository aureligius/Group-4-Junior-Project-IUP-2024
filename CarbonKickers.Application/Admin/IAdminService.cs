using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Admin;

public interface IAdminService
{
    Task<AdminSummaryResponse> GetSummaryAsync();

    Task<CreateWeeklyLeaguesResponse> CreateCurrentWeekLeaguesAsync();
    Task<IReadOnlyList<AdminBadgeResponse>> GetBadgesAsync();

    Task<AdminBadgeResponse> CreateBadgeAsync(SaveBadgeRequest request);

    Task<AdminBadgeResponse> UpdateBadgeAsync(int badgeId, SaveBadgeRequest request);
    Task<IReadOnlyList<AdminTransportModeResponse>> GetTransportModesAsync();

    Task<AdminTransportModeResponse> CreateTransportModeAsync(SaveTransportModeRequest request);

    Task<AdminTransportModeResponse> UpdateTransportModeAsync(
        int transportModeId,
        SaveTransportModeRequest request
    );
}
