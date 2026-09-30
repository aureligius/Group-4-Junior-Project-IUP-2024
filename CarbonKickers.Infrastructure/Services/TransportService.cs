//Buat Testing

using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarbonKickers.Infrastructure.Services;

public class TransportService
{
    private readonly AppDbContext _context;

    public TransportService(AppDbContext context)
    {
        _context = context;
    }

    // Add New Record
    public async Task<TransportMode> AddTransportModeAsync(string name, double emissionFactor)
    {
        var newMode = new TransportMode
        {
            ModeName = name,
            EmissionFactorPerKm = emissionFactor
        };

        _context.TransportModes.Add(newMode);
        await _context.SaveChangesAsync(); // Pushes the INSERT to Neon
        
        return newMode;
    }

    // Fect All Record
    public async Task<List<TransportMode>> GetAllModesAsync()
    {
        return await _context.TransportModes.ToListAsync();
    }
}