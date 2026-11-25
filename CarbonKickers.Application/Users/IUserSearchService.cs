using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Application.Users;

public interface IUserSearchService
{
    Task<IReadOnlyList<UserSearchResponse>> SearchAsync(Guid currentUserId, string query);
}
