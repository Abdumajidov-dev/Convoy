# Convoy API - Real-time Location Tracking System

Backend API for tracking driver locations in real-time using ASP.NET Core and PostgreSQL.

## Quick Start

```bash
# 1. Start PostgreSQL
docker run --name convoy-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15

# 2. Setup Firewall (Windows - Run as Admin)
setup-firewall.bat

# 3. Run API
dotnet run --launch-profile http

# 4. Open Swagger
# http://10.100.104.128:5084/swagger
```

## Documentation

- **[START_HERE.md](./START_HERE.md)** - Quick start guide
- **[VISUAL_STUDIO_SETUP.md](./VISUAL_STUDIO_SETUP.md)** - Visual Studio instructions
- **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** - Quick reference card
- **[FLUTTER_SETUP.md](./FLUTTER_SETUP.md)** - Flutter integration
- **[FIREWALL_SETUP.md](./FIREWALL_SETUP.md)** - Firewall configuration
- **[POSTGRESQL_SETUP.md](./POSTGRESQL_SETUP.md)** - Database setup
- **[API_EXAMPLES.md](./API_EXAMPLES.md)** - API examples
- **[README_UZLAT.md](./README_UZLAT.md)** - Full documentation (Uzbek)

## Features

- Location tracking with batch upload support
- PostgreSQL database with EF Core
- RESTful API with Swagger documentation
- CORS enabled for mobile apps
- Test data seeding
- Array-based location submission

## Tech Stack

- .NET 8.0
- ASP.NET Core Web API
- PostgreSQL + Npgsql
- Entity Framework Core
- Swagger/OpenAPI

## API Endpoints

- `POST /api/location` - Submit locations (array)
- `GET /api/location/user/{id}` - Get user location history
- `GET /api/location/latest` - Get latest locations for all users
- `GET /api/user` - Get all users
- `POST /api/user` - Create new user

## License

MIT