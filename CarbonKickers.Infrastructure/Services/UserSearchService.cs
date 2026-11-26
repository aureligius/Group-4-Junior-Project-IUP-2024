using CarbonKickers.Application.Users;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class UserSearchService : IUserSearchService
{
    private readonly AppDbContext _db;

    public UserSearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserSearchResponse>> SearchAsync(
        Guid currentUserId,
        string query
    )
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await _db
            .Users.AsNoTracking()
            .Where(user =>
                user.Id != currentUserId && user.Username.ToLower().Contains(normalizedQuery)
            )
            .OrderBy(user => user.Username)
            .Take(10)
            .Select(user => new UserSearchResponse(user.Id, user.Username))
            .ToListAsync();
    }
}
