# Convoy API - Setup Guide

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
- Network access: `http://10.100.104.128:5084/swagger`

## Database Commands

```bash
# Create migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Visual Studio Setup

**Launch Profiles:**
- `http` - Network access on `0.0.0.0:5084` (recommended for mobile testing)
- `http (localhost only)` - Localhost only, no admin required
- `https` - HTTPS with certificate
- `Convoy.Api` - HTTP + HTTPS
- `IIS Express` - IIS hosting

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
- `POST /api/location` - Accepts JSON array: `[{userId, latitude, longitude, timestamp}]`
- `POST /api/location/batch` - Accepts JSON object: `{locations: [{userId, latitude, longitude, timestamp}]}`
- `GET /api/location/user/{userId}` - Returns location history for specific user (default limit: 100)
- `GET /api/location/latest` - Returns latest location for each user using GroupBy
- Includes request body logging for debugging mobile clients

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
