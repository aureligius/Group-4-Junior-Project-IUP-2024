using CarbonKickers.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using CarbonKickers.Infrastructure.Services;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DATABASE_URL is missing. Check your .env file location and name.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<TransportService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();


// EndPoint
app.MapPost("/api/transport", async (TransportService service, string name, double factor) => 
{
    var result = await service.AddTransportModeAsync(name, factor);
    return Results.Ok(result);
});

// Get Endpoint
app.MapGet("/api/transport", async (TransportService service) =>
{
    var modes = await service.GetAllModesAsync();
    return Results.Ok(modes);
});


app.Run();