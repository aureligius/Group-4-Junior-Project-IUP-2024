using System.Text;
using CarbonKickers.Application.Users;
using CarbonKickers.Infrastructure.Data;
using CarbonKickers.Infrastructure.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "CarbonKickersApi";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "CarbonKickersWpf";

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DATABASE_URL is missing from .env.");
}

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT_KEY is missing or shorter than 32 characters.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<TransportService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost("/api/transport", async (
    TransportService service,
    string name,
    double factor) =>
{
    var result = await service.AddTransportModeAsync(name, factor);
    return Results.Ok(result);
});

app.MapGet("/api/transport", async (TransportService service) =>
{
    var modes = await service.GetAllModesAsync();
    return Results.Ok(modes);
});

app.Run();