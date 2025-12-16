using Convoy.Api.BackgroundServices;
using Convoy.Api.Hubs;
using Convoy.Data.Context;
using Convoy.Data.Interfaces;
using Convoy.Data.Repositories;
using Convoy.Data.Seeding;
using Convoy.Service.Interfaces;
using Convoy.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Get database connection string from environment variable (Railway) or configuration
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convert Railway DATABASE_URL format to Npgsql format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    try
    {
        var uri = new Uri(connectionString);
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Connection string conversion error: {ex.Message}");
    }
}

// Database connection - PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
builder.Services.AddScoped<IHourlySummaryRepository, HourlySummaryRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IMockDataService, MockDataService>(); // Mock data generation service
builder.Services.AddScoped<IDailySummaryService, DailySummaryService>();

// SignalR qo'shish (Real-time location tracking)
builder.Services.AddSignalR();

// Background Services
builder.Services.AddHostedService<DailySummaryBackgroundService>();

// CORS policy (Flutter app va SignalR uchun)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // Production uchun specific origins ishlatish kerak
        if (builder.Environment.IsProduction())
        {
            // Railway deployment URL va frontend URL'lar
            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
                ?? new[] { "*" };

            if (allowedOrigins[0] == "*")
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        }
        else
        {
            // Development uchun hamma origin ruxsat
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Circular reference handling
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // Property naming - snake_case for JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;

        // Write indented JSON for better readability in development
        options.JsonSerializerOptions.WriteIndented = true;

        // Default values handling
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// Seed data (test uchun)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed data qo'shishda xatolik");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Production uchun ham Swagger'ni yoqish (API documentation)
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Convoy API V1");
        c.RoutePrefix = "swagger"; // Root URL'da Swagger ochilmasligi uchun
    });
}

// Railway HTTPS'ni o'zi boshqaradi, shuning uchun HTTPS redirection kerak emas
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// CORS'ni yoqish
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// SignalR Hub endpoint
app.MapHub<LocationHub>("/hubs/location");

app.Run();
