namespace CarbonKickers.Infrastructure.Services;

using CarbonKickers.Domain.Interfaces;

public class GoogleMapsService : IMapsService
{
    public async Task<double> CalculateDistanceAsync(string origin, string destination)
    {
       // PEER Buat masukin API GOOGLE MAPS
       // Buat Sementara pakai dummy files
        return await Task.FromResult(12.5);
    }
}