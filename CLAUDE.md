# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Convoy API is a real-time location tracking system built with ASP.NET Core 8.0 and PostgreSQL. It tracks driver locations with batch upload support, designed for mobile fleet management applications.

## Development Commands

### Prerequisites
```bash
# Start PostgreSQL (required before running API)
docker run --name convoy-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15
```

### Running the Application
```bash
# Run with HTTP profile (recommended for local network testing)
dotnet run --launch-profile http

# Run with HTTPS profile
dotnet run --launch-profile https

# Restore packages
dotnet restore

# Build project
dotnet build
```

### Database Operations
```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Windows-Specific Setup
```bash
# Setup Windows Firewall rules (run as Administrator)
setup-firewall.bat

# Setup URL reservation for network access (run as Administrator)
# This allows running without admin rights
setup-url-reservation.bat

# Start API with convenience script
start-api.bat
```

### Visual Studio Development

**Important:** To run the API with network access (not just localhost) in Visual Studio, you must:

1. **Run Visual Studio as Administrator**, OR
2. **Setup URL reservation once** using `setup-url-reservation.bat`

**Launch Profiles:**
- `http (Admin Required)` - Listens on `0.0.0.0:5084` (all network interfaces, requires admin or URL reservation)
- `http (localhost only)` - Listens on `localhost:5084` (no admin required)
- `https` - HTTPS with localhost certificate
- `IIS Express` - IIS Express hosting

See `VS_ADMIN_GUIDE.md` for detailed setup instructions.

## Architecture

### Core Models

**User Model** (`Models/User.cs`)
- Represents drivers/users in the system
- Has one-to-many relationship with Location
- Phone number must be unique
- Supports soft delete via IsActive flag

**Location Model** (`Models/Location.cs`)
- Stores GPS coordinates with timestamp
- Foreign key relationship to User
- Optional fields: Speed, Accuracy
- Timestamps are stored in UTC

**DTOs** (`Models/LocationDto.cs`, `Models/UserDto.cs`)
- `LocationDto`: Accepts location data as strings from mobile clients
- `LocationBatchDto`: Supports array-based batch location uploads
- `CreateUserDto`: User creation without ID
- `UserResponseDto`: User data without navigation properties

### Database Context

`Data/AppDbContext.cs` configures:
- User entity with unique phone index
- Location entity with UserId and Timestamp indexes
- Cascade delete from User to Locations
- EF Core conventions and relationships

### Data Seeding

`Data/SeedData.cs` automatically seeds 3 test users on startup if database is empty. This runs during application initialization in Program.cs.

### Controllers

**LocationController** (`Controllers/LocationController.cs`)
- `POST /api/location` - Accepts array of location objects, parses string coordinates to doubles, converts timestamps to UTC
- `GET /api/location/user/{userId}` - Returns location history for specific user (default limit: 100)
- `GET /api/location/latest` - Returns latest location for each user using GroupBy

**UserController** (`Controllers/UserController.cs`)
- Full CRUD operations for users
- Soft delete implementation (sets IsActive = false)
- Phone uniqueness validation on create/update
- Returns DTOs instead of entities to avoid circular references

### Application Configuration

**Program.cs**
- PostgreSQL connection via Npgsql
- CORS policy "AllowAll" enabled for mobile app integration
- Automatic data seeding on startup
- Swagger enabled in Development environment

**Connection String** (appsettings.json)
```
Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;
```

### Key Design Patterns

1. **String to Type Conversion**: Location endpoints accept lat/lng/timestamp as strings and parse them to proper types with error handling
2. **UTC Timestamp Handling**: All timestamps converted to UTC before database storage to avoid timezone issues with PostgreSQL
3. **Batch Processing**: Location submission supports arrays, processes each item individually, returns detailed success/error counts
4. **DTO Pattern**: Separates external API contracts from internal entities
5. **Soft Delete**: Users are deactivated rather than deleted to preserve location history

### Network Configuration

The application is configured to listen on `http://0.0.0.0:5084` in the HTTP profile, making it accessible on the local network. The specific machine IP is `10.100.104.128` (see launchSettings.json).

### Important Notes

- All DateTime fields should be handled in UTC to maintain consistency with PostgreSQL
- Location submission accepts string coordinates for better mobile client compatibility
- Phone numbers must be unique across all users
- The API uses Swagger UI for testing: `http://localhost:5084/swagger`
- Firewall rules must be configured on Windows for network access
