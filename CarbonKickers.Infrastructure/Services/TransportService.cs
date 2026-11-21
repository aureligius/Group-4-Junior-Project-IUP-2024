using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public sealed class TransportService
{
    private readonly AppDbContext _context;

    public TransportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TransportModeResponse>> GetAllModesAsync()
    {
        return await _context
            .TransportModes.AsNoTracking()
            .OrderBy(mode => mode.ModeName)
            .Select(mode => new TransportModeResponse(
                mode.Id,
                mode.ModeName,
                mode.EmissionFactorPerKm
            ))
            .ToListAsync();
    }
}
