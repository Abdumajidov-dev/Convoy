# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is a **Convoy** - a fleet management API built with **Clean Architecture** principles, designed for tracking driver locations in real-time using **SignalR WebSocket** for instant bi-directional communication with mobile clients.

## Quick Start Commands

### Build and Run

```bash
# Build entire Convoy solution
dotnet build Convoy.sln

# Restore dependencies
dotnet restore Convoy.sln

# Run the API with network access (for mobile testing)
cd Convoy.Api
dotnet run --launch-profile http

# Run localhost only
dotnet run --launch-profile "http (localhost only)"
```

### Start PostgreSQL (Required)

```bash
# Start PostgreSQL in Docker
docker run --name convoy-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15

# Check if PostgreSQL is running
docker ps

# Restart PostgreSQL if needed
docker restart convoy-postgres

# View PostgreSQL logs
docker logs convoy-postgres
```

### Database Migrations

Always run from `Convoy.Api` directory with `--project` flag:

```bash
# Navigate to API directory
cd Convoy.Api

# Create migration
dotnet ef migrations add <MigrationName> --project ../Convoy.Data

# Apply to database
dotnet ef database update --project ../Convoy.Data

# Rollback migration
dotnet ef migrations remove --project ../Convoy.Data

# Generate SQL script
dotnet ef migrations script --project ../Convoy.Data
```

## Architecture: Convoy Solution

Convoy follows **Clean Architecture** with clear separation of layers:

```
Convoy.sln
├── Convoy.Api/           # Presentation Layer (REST API + SignalR)
│   ├── Controllers/      # UserController, LocationController, DailySummaryController, TestDataController
│   ├── Hubs/             # LocationHub (SignalR WebSocket)
│   ├── BackgroundServices/  # DailySummaryBackgroundService (midnight processing)
│   ├── Program.cs        # DI, CORS, SignalR, background services configuration
│   ├── appsettings.json  # Configuration (DB connection)
│   ├── wwwroot/          # Static files
│   │   └── signalr-test.html  # SignalR test client
│   └── FLUTTER_SIGNALR_GUIDE.md  # Flutter integration guide
│
├── Convoy.Service/       # Business Logic Layer
│   ├── Interfaces/       # IUserService, ILocationService, IDailySummaryService, IMockDataService
│   └── Services/         # UserService, LocationService, DailySummaryService, MockDataService
│
├── Convoy.Data/          # Data Access Layer
│   ├── Context/          # AppDbContext (EF Core)
│   ├── Interfaces/       # IUserRepository, ILocationRepository, IDailySummaryRepository, IHourlySummaryRepository
│   ├── Repositories/     # Repository implementations
│   ├── Migrations/       # EF Core migrations
│   └── Seeding/          # SeedData.cs (test data)
│
└── Convoy.Domain/        # Domain Layer (Core)
    ├── Entities/         # User, Location, DailySummary, HourlySummary entities
    └── DTOs/             # Data Transfer Objects
```

### Layer Dependencies

```
Convoy.Api
    ↓ (references)
Convoy.Service + Convoy.Data
    ↓ (references)
Convoy.Domain
```

**Key Principle**: Dependencies flow inward. Domain has no dependencies. Data and Service depend on Domain. API depends on Service and Data.

## Core Domain Models

### User Entity (`Convoy.Domain/Entities/User.cs`)

Represents drivers/fleet members:
- **Properties**: `Id`, `Name`, `Phone` (unique), `IsActive`, `CreatedAt`
- **Relationships**: One-to-many with `Location`, `DailySummary`, `HourlySummary`
- **Soft Delete**: Uses `IsActive` flag instead of hard delete
- **Constraints**: Phone number must be unique (database index)

### Location Entity (`Convoy.Domain/Entities/Location.cs`)

GPS tracking points:
- **Properties**: `Id`, `UserId`, `Latitude`, `Longitude`, `Timestamp`, `Speed?`, `Accuracy?`
- **Relationships**: Many-to-one with `User` (cascade delete)
- **Indexes**: `UserId` and `Timestamp` for query performance
- **Timestamps**: All stored in UTC
- **Retention**: Cleaned up after 7 days (kept only for aggregation; summaries retained)

### DailySummary Entity (`Convoy.Domain/Entities/DailySummary.cs`)

Daily aggregated statistics per user:
- **Properties**:
  - `Id`, `UserId`, `Date` (date only, no time)
  - `TotalLocations`, `TotalDistanceKm`
  - `FirstLocationTime`, `LastLocationTime`
  - `AverageSpeed`, `MaxSpeed`
  - Geographic bounds: `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude`
  - `CreatedAt`
- **Relationships**: Many-to-one with `User`, one-to-many with `HourlySummary`
- **Purpose**: Historical analytics and reporting without storing raw location data long-term
- **Generation**: Auto-created at midnight UTC by background service

### HourlySummary Entity (`Convoy.Domain/Entities/HourlySummary.cs`)

Hourly breakdown within daily summaries:
- **Properties**:
  - `Id`, `DailySummaryId`, `UserId`, `Hour` (0-23)
  - `LocationCount`, `DistanceKm`, `AverageSpeed`
  - `FirstLocationTime`, `LastLocationTime`
  - Geographic bounds: `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude`
- **Relationships**: Many-to-one with `DailySummary` and `User`
- **Purpose**: Hour-by-hour activity filtering for map visualization

## Data Access Patterns

### Repository Pattern

The Data layer uses Repository pattern with interfaces:

```csharp
// Defined in Convoy.Data/Interfaces/
IUserRepository
ILocationRepository
IDailySummaryRepository
IHourlySummaryRepository

// Implemented in Convoy.Data/Repositories/
UserRepository : IUserRepository
LocationRepository : ILocationRepository
DailySummaryRepository : IDailySummaryRepository
HourlySummaryRepository : IHourlySummaryRepository
```

### Service Layer Pattern

Services orchestrate business logic and call repositories:

```csharp
// Convoy.Service/Interfaces/
IUserService
ILocationService
IDailySummaryService
IMockDataService  // Test data generation

// Convoy.Service/Services/
UserService : IUserService
LocationService : ILocationService
DailySummaryService : IDailySummaryService
MockDataService : IMockDataService
```

### Dependency Injection

All services and repositories registered in `Convoy.Api/Program.cs`:

```csharp
// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ILocationRepository, LocationRepository>();
services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
services.AddScoped<IHourlySummaryRepository, HourlySummaryRepository>();

// Services
services.AddScoped<IUserService, UserService>();
services.AddScoped<ILocationService, LocationService>();
services.AddScoped<IDailySummaryService, DailySummaryService>();
services.AddScoped<IMockDataService, MockDataService>();

// Background Services
services.AddHostedService<DailySummaryBackgroundService>();

// DbContext
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

## Communication Methods

### SignalR WebSocket (Primary - Real-time)

**Endpoint**: `ws://localhost:5084/hubs/location` or `wss://domain/hubs/location`

Real-time bi-directional communication for instant location tracking. Clients connect once and send/receive location updates via WebSocket.

**Client -> Server Methods**:
- `SendLocation(locationDto)` - Send single location update
- `SendLocationBatch(locations[])` - Send multiple locations
- `SubscribeToUser(userId)` - Subscribe to specific user's updates
- `UnsubscribeFromUser(userId)` - Unsubscribe from user

**Server -> Client Events**:
- `LocationUpdated` - Broadcast when any location is received (all clients)
- `LocationReceived` - Confirmation message (sender only)
- `LocationError` - Error message (sender only)
- `BatchLocationReceived` - Batch processing result (sender only)

See `FLUTTER_SIGNALR_GUIDE.md` for complete Flutter integration guide.

**Test Client**: Open `http://localhost:5084/signalr-test.html` in browser

### REST API Endpoints (Legacy - Backup)

**POST /api/location** - Submit location array directly
- Accepts: `[{userId, latitude, longitude, timestamp}]`
- Returns: `{savedCount, totalReceived, errors?}`

**POST /api/location/batch** - Submit wrapped batch
- Accepts: `{locations: [{userId, latitude, longitude, timestamp}]}`
- Returns: Same as above

**GET /api/location/user/{userId}?limit=100** - Get user location history
- Returns: Array of locations (newest first)

**GET /api/location/latest** - Get latest location for all users
- Returns: Array with most recent location per user

### User Management

**GET /api/user** - List all active users

**GET /api/user/{id}** - Get user by ID

**POST /api/user** - Create new user
- Validates phone uniqueness

**PUT /api/user/{id}** - Update user
- Validates phone uniqueness

**DELETE /api/user/{id}** - Soft delete user
- Sets `IsActive = false`

### Daily Summary Analytics

**GET /api/dailysummary/user/{userId}** - Get daily summaries for a user
- Optional query params: `startDate`, `endDate`, `limit`
- Returns aggregated daily statistics with hourly breakdowns

**POST /api/dailysummary/process-yesterday** - Manually trigger yesterday's data processing
- Useful for testing or recovery
- Returns processing results

### Test Data Generation (Development Only)

**POST /api/test/generate-mock-locations** - Generate realistic GPS mock data
- Accepts: `{userId, startDate, endDate, pointsPerHour, speed}`
- Creates simulated location trails for testing analytics
- Only available in Development environment
- Example use: Generate data to test "Where was Employee X yesterday?" scenarios

**GET /api/test/health** - Check if test endpoints are available

## Critical Implementation Details

### 1. UTC DateTime Handling

**IMPORTANT**: All DateTime values must be UTC to avoid PostgreSQL timezone issues.

```csharp
// When creating timestamps
var timestamp = DateTime.UtcNow;

// When receiving from client
var utcTime = DateTime.SpecifyKind(clientTimestamp, DateTimeKind.Utc);
```

### 2. String Coordinate Parsing

Location endpoints accept lat/lng as **strings** for mobile client compatibility:

```csharp
// DTOs use string
public string Latitude { get; set; }
public string Longitude { get; set; }

// Parse with InvariantCulture
double.Parse(dto.Latitude, CultureInfo.InvariantCulture)
```

### 3. Batch Processing with Partial Success

Location submissions process individually - some can fail while others succeed:

```csharp
// Response includes
{
  "savedCount": 8,
  "totalReceived": 10,
  "errors": ["Item 3: Invalid userId", "Item 7: Invalid coordinates"]
}
```

This allows mobile clients to retry only failed items.

### 4. SignalR Real-time Communication

All location updates are broadcast to connected clients in real-time:

```csharp
// In LocationHub
await Clients.All.SendAsync("LocationUpdated", new
{
    userId = result.UserId,
    latitude = result.Latitude,
    longitude = result.Longitude,
    timestamp = result.Timestamp,
    speed = result.Speed,
    accuracy = result.Accuracy
});
```

Clients can subscribe to specific users or receive all updates.

### 5. CORS Configuration

Currently allows **all origins** for mobile testing and SignalR:

```csharp
// In Program.cs
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

**Production**: Restrict to specific origins and use `.AllowCredentials()` for SignalR.

### 6. Cascade Deletes

Deleting a `User` (hard delete) **cascades** to all their `Locations`, `DailySummary`, and `HourlySummary`:

```csharp
// In AppDbContext
entity.HasOne(e => e.User)
      .WithMany(u => u.Locations)
      .OnDelete(DeleteBehavior.Cascade);
```

**Prefer soft delete** to preserve history.

### 7. Background Services - Automated Daily Processing

`DailySummaryBackgroundService` runs continuously and:
- Waits until midnight UTC each day
- Processes previous day's location data into `DailySummary` and `HourlySummary` entities
- Calculates distances, speeds, and geographic bounds
- Cleans up raw `Location` records older than 7 days (retention policy)
- Retries on failure after 1 hour delay

This enables long-term analytics without storing massive amounts of raw location data.

### 8. Mock Data Generation

`IMockDataService` generates realistic test data for development/testing:
- Creates GPS trails with configurable speed and density
- Useful for testing analytics features without real mobile clients
- Only accessible via `/api/test` endpoints in Development environment
- Generates data for specified date ranges to test historical queries

## Database Configuration

### Connection Strings

The application uses environment-specific appsettings files:

**appsettings.json** (Base configuration):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;"
  },
  "DeploymentUrl": "https://convoy-production-2969.up.railway.app"
}
```

**appsettings.Development.json** (Local development):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;"
  },
  "DeploymentUrl": "http://localhost:5084"
}
```

**appsettings.Production.json** (Railway deployment):
- Uses `DATABASE_URL` environment variable (auto-configured by Railway)
- Connection string is empty in file, set via environment variable
- `DeploymentUrl`: `https://convoy-production-2969.up.railway.app`

**Note**: Local development uses port **5432**. When running PostgreSQL in Docker, map port 5432:5432.

### Entity Framework Migrations

Always run from `Convoy.Api` directory with `--project` flag:

```bash
# Create migration
dotnet ef migrations add InitialCreate --project ../Convoy.Data

# Apply to database
dotnet ef database update --project ../Convoy.Data

# Rollback
dotnet ef migrations remove --project ../Convoy.Data

# Generate SQL script
dotnet ef migrations script --project ../Convoy.Data
```

### Data Seeding

Automatic seeding runs on startup via `SeedData.Initialize()` in `Program.cs`:

- Seeds 3 test users if database is empty
- Test users: Haydovchi 1, 2, 3 with phone numbers +998901234567-69

### Migrations

Latest migration: `AddDailySummaryEntities` (2024-12-09)
- Added `DailySummary` and `HourlySummary` tables
- Configured relationships and indexes for analytics queries

## Network Configuration

### Launch Profiles

Available in `Convoy.Api/Properties/launchSettings.json`:

- **`http`** - Network access on `0.0.0.0:5084` (recommended for mobile)
- **`http (localhost only)`** - Localhost only
- **`https`** - HTTPS on port 7147

### Windows Firewall (First-time Setup)

Required for network access from mobile devices:

```powershell
# Run as Administrator
netsh advfirewall firewall add rule name="Convoy API HTTP" dir=in action=allow protocol=TCP localport=5084
netsh advfirewall firewall add rule name="Convoy API HTTPS" dir=in action=allow protocol=TCP localport=7147
```

### Accessing from Mobile Devices

1. Ensure PC and mobile on same WiFi
2. Run API with network profile: `dotnet run --launch-profile http`
3. Find PC's local IP: `ipconfig` (look for IPv4 Address)
4. Access from mobile: `http://<PC-IP>:5084/swagger`

## Testing

### SignalR Test Client

1. Start the API: `dotnet run --launch-profile http`
2. Open browser: `http://localhost:5084/signalr-test.html`
3. Click "Connect" to establish WebSocket connection
4. Send test locations and see real-time broadcasts
5. Use "Start Simulation" for automatic location updates every 5 seconds

### Swagger UI (REST API)

Available at: `http://localhost:5084/swagger` (Development only)

**Example Location Batch**:

```json
{
  "locations": [
    {
      "userId": 1,
      "latitude": "41.2995",
      "longitude": "69.2401",
      "timestamp": "2024-12-03T10:30:00Z",
      "speed": "45.5",
      "accuracy": "10.0"
    }
  ]
}
```

Or directly as array for `/api/location`:

```json
[
  {
    "userId": 1,
    "latitude": "41.2995",
    "longitude": "69.2401",
    "timestamp": "2024-12-03T10:30:00Z",
    "speed": "45.5",
    "accuracy": "10.0"
  }
]
```

## Common Development Workflows

### Testing Analytics Features

1. Start the API: `dotnet run --launch-profile http`
2. Generate mock data via Swagger or curl:
```bash
curl -X POST http://localhost:5084/api/test/generate-mock-locations \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "startDate": "2024-12-08T00:00:00Z",
    "endDate": "2024-12-08T23:59:59Z",
    "pointsPerHour": 12,
    "speed": 45.0
  }'
```
3. Manually trigger daily summary processing:
```bash
curl -X POST http://localhost:5084/api/dailysummary/process-yesterday
```
4. Query the summaries:
```bash
curl http://localhost:5084/api/dailysummary/user/1?startDate=2024-12-08&endDate=2024-12-09
```

### Adding a New Entity

1. Create entity class in `Convoy.Domain/Entities/`
2. Add `DbSet<T>` to `AppDbContext` in `Convoy.Data/Context/`
3. Configure entity in `OnModelCreating()` of `AppDbContext`
4. Create migration: `dotnet ef migrations add Add<EntityName> --project ../Convoy.Data`
5. Apply migration: `dotnet ef database update --project ../Convoy.Data`

### Adding a New Repository

1. Create interface in `Convoy.Data/Interfaces/`
2. Implement in `Convoy.Data/Repositories/`
3. Register in `Program.cs`: `services.AddScoped<IMyRepository, MyRepository>()`

### Adding a New Service

1. Create interface in `Convoy.Service/Interfaces/`
2. Implement in `Convoy.Service/Services/`
3. Inject required repositories via constructor
4. Register in `Program.cs`: `services.AddScoped<IMyService, MyService>()`

### Adding a New Controller

1. Create controller in `Convoy.Api/Controllers/`
2. Inherit from `ControllerBase`
3. Add `[ApiController]` and `[Route("api/[controller]")]` attributes
4. Inject required services via constructor
5. Controller automatically discovered (no registration needed)

### Adding a Background Service

1. Create service class in `Convoy.Api/BackgroundServices/`
2. Inherit from `BackgroundService`
3. Implement `ExecuteAsync(CancellationToken)` method
4. Register in `Program.cs`: `services.AddHostedService<MyBackgroundService>()`
5. Use `IServiceProvider` to create scopes for scoped dependencies

## Client Integration Guides

The repository includes comprehensive integration guides for different client platforms:

### Flutter Integration

**File**: `Convoy.Api/FLUTTER_SIGNALR_GUIDE.md`

Complete Flutter integration guide with:
- SignalR client setup with `signalr_netcore` package
- Real-time location tracking service
- GPS integration with `geolocator` package
- Background location tracking
- Full code examples and usage patterns

**Quick Start**:
1. Add dependencies: `signalr_netcore`, `geolocator`, `permission_handler`
2. Connect to hub: `ws://YOUR_IP:5084/hubs/location`
3. Send locations: `hubConnection.invoke('SendLocation', ...)`
4. Listen for updates: `hubConnection.on('LocationUpdated', ...)`

### WPF Integration

**File**: `Convoy.Api/WPF_SIGNALR_GUIDE.md`

WPF SignalR integration guide for desktop clients.

## Troubleshooting

### Port Already in Use

```bash
# Find process using port 5084
netstat -ano | findstr :5084

# Kill process by PID
taskkill /PID <process_id> /F
```

### Migration Errors

```bash
# Ensure you're in Convoy.Api directory
cd Convoy.Api

# Always use --project flag
dotnet ef database update --project ../Convoy.Data
```

### PostgreSQL Connection Issues

```bash
# Check if PostgreSQL is running
docker ps

# Restart PostgreSQL container
docker restart convoy-postgres

# View PostgreSQL logs
docker logs convoy-postgres
```

### Background Service Not Running

Check application logs for background service status:
- Look for "Daily Summary Background Service started" on startup
- Look for "Next daily summary processing scheduled at..." messages
- If service crashes, it logs errors and retries after 1 hour
- Manually trigger processing via `/api/dailysummary/process-yesterday` endpoint

### No Data in Daily Summaries

Common causes:
1. Background service hasn't run yet (wait until after midnight UTC)
2. No location data exists for previous days
3. Manually trigger: `curl -X POST http://localhost:5084/api/dailysummary/process-yesterday`
4. Check logs for processing errors

## Project Files Reference

**Convoy Projects**:
- `Convoy.Api/Convoy.Api.csproj` - Web API project
- `Convoy.Service/Convoy.Service.csproj` - Service layer (class library)
- `Convoy.Data/Convoy.Data.csproj` - Data access layer (class library)
- `Convoy.Domain/Convoy.Domain.csproj` - Domain models (class library)

**Key Files**:
- `Convoy.Api/Program.cs` - Application entry point and DI configuration
- `Convoy.Data/Context/AppDbContext.cs` - EF Core DbContext with entity configuration
- `Convoy.Api/appsettings.json` - Application configuration

## Best Practices for This Codebase

1. **Always use UTC** for DateTime fields when working with PostgreSQL
2. **Prefer SignalR over REST** for real-time location tracking (lower latency, bi-directional)
3. **Soft delete users** rather than hard delete to preserve location history and summaries
4. **Parse coordinates with InvariantCulture** to handle different decimal separators
5. **Run migrations from Convoy.Api** with `--project ../Convoy.Data` flag
6. **Use CORS AllowAll only in development** - restrict in production and add `.AllowCredentials()` for SignalR
7. **Validate UserId exists** before accepting location data
8. **Broadcast location updates** to all connected clients for real-time map updates
9. **Handle SignalR reconnection** gracefully on mobile clients (use `.withAutomaticReconnect()`)
10. **Return detailed error messages** in batch operations for client retry logic
11. **Use mock data service** for testing analytics without real mobile devices
12. **Monitor background service logs** for daily summary processing status
13. **Query DailySummary instead of raw Locations** for historical data (performance + retention)
14. **Raw locations are auto-deleted after 7 days** - summaries are permanent

## Deployment

### Railway Deployment

**Production URL**: https://convoy-production-2969.up.railway.app

**File**: `RAILWAY_DEPLOYMENT.md`

Complete Railway deployment guide (in Uzbek) covering:
- PostgreSQL database setup on Railway
- Environment variable configuration
- Database migration application
- Production CORS settings
- Continuous deployment setup
- Troubleshooting common issues

**Key Environment Variables**:
- `DATABASE_URL` - PostgreSQL connection (auto-configured by Railway)
- `ASPNETCORE_ENVIRONMENT=Production`
- `PORT=8080` (Railway default)
- `ALLOWED_ORIGINS` - Comma-separated list of allowed frontend URLs

**Dockerfile**: Multi-stage build using .NET 8.0 SDK and ASP.NET runtime, exposing port 8080.

**Deployment Status**:
- Build: ✅ Successful
- Container: ✅ Running on port 8080
- Background Service: ✅ Daily summary processing active
- Database: ✅ Connected to Railway PostgreSQL
- Test Data: ✅ Seeded (3 test users)

### Production Considerations

1. **Database Connection**: Uses `DATABASE_URL` environment variable in production
2. **CORS**: Configure `ALLOWED_ORIGINS` for specific domains, or leave unset for `*` (all origins)
3. **HTTPS**: Railway handles HTTPS via proxy, so `UseHttpsRedirection()` is disabled in production
4. **Swagger**: Available in production at `/swagger` endpoint
5. **Port**: Application listens on port 8080 for Railway/cloud deployment

### Testing Production API

```bash
# Swagger UI
https://convoy-production-2969.up.railway.app/swagger

# SignalR Hub (WebSocket)
wss://convoy-production-2969.up.railway.app/hubs/location

# REST API Endpoints
curl https://convoy-production-2969.up.railway.app/api/user
curl https://convoy-production-2969.up.railway.app/api/location/latest
```
