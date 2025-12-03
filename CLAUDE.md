# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Convoy API is a real-time location tracking system built with ASP.NET Core 8.0 and PostgreSQL. It tracks driver locations with batch upload support, designed for mobile fleet management applications.

## Quick Start

### 1. Start PostgreSQL
```bash
docker run --name convoy-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15
```

### 2. Setup Windows Firewall (First Time Only - Run as Administrator)
```powershell
# Allow incoming connections on port 5084
netsh advfirewall firewall add rule name="Convoy API HTTP" dir=in action=allow protocol=TCP localport=5084
netsh advfirewall firewall add rule name="Convoy API HTTPS" dir=in action=allow protocol=TCP localport=7147
```

### 3. Run the API
```bash
# For network access (mobile devices on same WiFi)
dotnet run --launch-profile http

# For localhost only
dotnet run --launch-profile "http (localhost only)"
```

### 4. Test the API
- Swagger UI: `http://localhost:5084/swagger`
- Network access: `http://0.0.0.0:5084/swagger` (or use your machine's local IP)

## Common Development Commands

### Build and Run
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the API (with hot reload)
dotnet run

# Run specific launch profile
dotnet run --launch-profile http
dotnet run --launch-profile "http (localhost only)"
```

### Database Management
```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# View migration SQL without applying
dotnet ef migrations script
```

### Checking Ports and Processes
```bash
# Check if port 5084 is in use
netstat -ano | findstr :5084

# Kill process by PID (if needed)
taskkill /PID <process_id> /F

# Check firewall rules
netsh advfirewall firewall show rule name="Convoy API HTTP"
```

## Architecture Overview

### Project Structure
```
Convoy.Api/
├── Controllers/          # API endpoints
│   ├── LocationController.cs   # Location tracking endpoints
│   └── UserController.cs       # User CRUD operations
├── Data/                # Database context and seeding
│   ├── AppDbContext.cs         # EF Core DbContext
│   └── SeedData.cs             # Initial test data
├── Models/              # Domain models and DTOs
│   ├── User.cs                 # User entity
│   ├── Location.cs             # Location entity
│   ├── LocationDto.cs          # Location DTOs
│   └── UserDto.cs              # User DTOs
├── Migrations/          # EF Core migrations
└── Program.cs           # App configuration and startup
```

### Core Models

**User Model** (`Models/User.cs`)
- Represents drivers/users in the system
- One-to-many relationship with Location
- Phone number must be unique (enforced by database index)
- Supports soft delete via `IsActive` flag
- All timestamps stored in UTC

**Location Model** (`Models/Location.cs`)
- Stores GPS coordinates with timestamp
- Foreign key relationship to User
- Optional fields: `Speed`, `Accuracy`
- Cascade delete when User is deleted
- Indexed on `UserId` and `Timestamp` for query performance

**DTOs** (`Models/LocationDto.cs`, `Models/UserDto.cs`)
- `LocationDto`: Accepts location data as strings from mobile clients for flexibility
- `LocationBatchDto`: Wrapper for batch location uploads
- `CreateUserDto`: User creation without ID (for POST requests)
- `UserResponseDto`: User data without navigation properties to avoid circular references

### Database Context (`Data/AppDbContext.cs`)

Configures:
- User entity with unique phone index
- Location entity with UserId and Timestamp indexes for query optimization
- Cascade delete from User to Locations
- EF Core conventions and relationships

**Important**: The DbContext uses Npgsql for PostgreSQL. All DateTime values must be in UTC to avoid timezone conversion issues.

### Data Seeding (`Data/SeedData.cs`)

Automatically seeds 3 test users on startup if database is empty:
- Haydovchi 1 (+998901234567)
- Haydovchi 2 (+998901234568)
- Haydovchi 3 (+998901234569)

This runs during application initialization in `Program.cs` before the app starts accepting requests.

### API Endpoints

**LocationController** (`Controllers/LocationController.cs`)

Two endpoints for location submission (both accept the same data format):
- `POST /api/location` - Accepts JSON array directly: `[{userId, latitude, longitude, timestamp}]`
- `POST /api/location/batch` - Accepts JSON object: `{locations: [{userId, latitude, longitude, timestamp}]}`

Both endpoints:
- Accept lat/lng/timestamp as strings and parse them to proper types
- Convert timestamps to UTC before storage
- Validate that UserId exists
- Return detailed success/error counts for batch operations
- Log request bodies for debugging mobile clients

Query endpoints:
- `GET /api/location/user/{userId}?limit=100` - Returns location history for specific user (newest first)
- `GET /api/location/latest` - Returns latest location for each user using GroupBy

**UserController** (`Controllers/UserController.cs`)

Standard CRUD operations:
- `GET /api/user` - List all active users
- `GET /api/user/{id}` - Get specific user
- `POST /api/user` - Create new user (validates phone uniqueness)
- `PUT /api/user/{id}` - Update user (validates phone uniqueness)
- `DELETE /api/user/{id}` - Soft delete (sets `IsActive = false`)

All endpoints return DTOs to avoid circular references from navigation properties.

### Application Configuration (`Program.cs`)

Key configurations:
- PostgreSQL connection via Npgsql
- CORS policy "AllowAll" enabled for mobile app integration (allows any origin, method, header)
- Automatic data seeding on startup
- Swagger enabled in Development environment only
- HTTPS redirection enabled

**Connection String** (appsettings.json):
```
Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;
```

### Launch Profiles (`Properties/launchSettings.json`)

Available profiles:
- `http` - Network access on `0.0.0.0:5084` (recommended for mobile testing)
- `http (localhost only)` - Localhost only on `localhost:5084`
- `https` - HTTPS on `localhost:7147` with HTTP on `localhost:5084`
- `Convoy.Api` - Both HTTP and HTTPS on `0.0.0.0`
- `IIS Express` - IIS hosting

Use `dotnet run --launch-profile <profile-name>` to select a profile.

## Key Design Patterns and Decisions

### 1. String to Type Conversion
Location endpoints accept latitude, longitude, and timestamp as strings from mobile clients. This provides better compatibility with various mobile frameworks that may serialize numbers differently. The API parses these strings to appropriate types (`double`, `DateTime`) with proper error handling.

### 2. UTC Timestamp Handling
All timestamps are converted to UTC before database storage. PostgreSQL's timestamp handling can cause timezone issues if local times are stored. The API ensures consistency by:
- Creating DateTime values with `DateTime.UtcNow`
- Converting incoming timestamps to UTC using `DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)`

### 3. Batch Processing with Error Handling
Location submission supports arrays and processes each item individually. If some items fail validation, others still get saved. The response includes:
- `savedCount` - Number of successfully saved locations
- `totalReceived` - Total locations in the request
- `errors` - Array of error messages (null if no errors)

This allows mobile clients to retry only failed items if needed.

### 4. DTO Pattern
The API uses DTOs (Data Transfer Objects) to separate external API contracts from internal entity models. This:
- Prevents circular reference errors in JSON serialization
- Allows different validation rules for create vs update operations
- Keeps internal entity structure flexible

### 5. Soft Delete Pattern
Users are deactivated (`IsActive = false`) rather than deleted from the database. This:
- Preserves location history even when users are "deleted"
- Allows users to be reactivated if needed
- Maintains referential integrity for historical data

### 6. Request Body Logging
Both location endpoints log the raw request body for debugging. This is particularly useful when troubleshooting issues with mobile clients that may send data in unexpected formats.

## Network Configuration

The application listens on `http://0.0.0.0:5084` in the `http` profile, making it accessible on the local network for mobile device testing. Windows Firewall rules must be configured to allow incoming connections on ports 5084 (HTTP) and 7147 (HTTPS).

## Important Implementation Notes

- **DateTime Handling**: Always use UTC for DateTime fields. PostgreSQL timestamp columns expect UTC values.
- **Phone Uniqueness**: Phone numbers have a unique index in the database. Creation/update will fail if phone already exists.
- **String Coordinates**: Location endpoints accept lat/lng as strings for mobile client compatibility. Parse with `CultureInfo.InvariantCulture` to handle different decimal separators.
- **Soft Deletes**: When querying users, filter by `IsActive = true` to exclude soft-deleted users.
- **CORS**: The "AllowAll" policy is enabled for mobile app integration. In production, this should be restricted to specific origins.
- **Cascade Deletes**: Deleting a User (hard delete) will cascade delete all their Locations. Soft delete is preferred.

## Testing with Swagger

Swagger UI is available at `http://localhost:5084/swagger` in Development mode. It provides interactive API documentation and testing capabilities.

Example location batch submission:
```json
{
  "locations": [
    {
      "userId": 1,
      "latitude": "41.2995",
      "longitude": "69.2401",
      "timestamp": "2024-12-02T10:30:00Z"
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
    "timestamp": "2024-12-02T10:30:00Z"
  }
]
```
