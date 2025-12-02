# PostgreSQL Setup - Convoy API

## O'rnatilgan Texnologiyalar

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 8.0 + Npgsql
- **API Testing**: Swagger/OpenAPI

## PostgreSQL Connection String

### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;"
  }
}
```

### Production (Docker)
Agar Docker'da ishlatmoqchi bo'lsangiz:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=172.17.0.1;Port=5432;Database=ConvoyDb;Username=postgres;Password=YourPassword;Minimum Pool Size=10;Maximum Pool Size=50;Connection Idle Lifetime=60;Command Timeout=120;Pooling=true;"
  }
}
```

## PostgreSQL O'rnatish

### Windows
1. PostgreSQL.org dan yuklang: https://www.postgresql.org/download/windows/
2. Default Port: 5432
3. Password: 2001 (yoki o'zingizni qo'ying)

### Docker bilan (Recommended)
```bash
docker run --name convoy-postgres \
  -e POSTGRES_PASSWORD=2001 \
  -e POSTGRES_DB=ConvoyDb \
  -p 5432:5432 \
  -d postgres:15
```

## Database Yaratish

### 1. Migration qo'llash
```bash
dotnet ef database update
```

### 2. Qo'lda yaratish (agar kerak bo'lsa)
```sql
CREATE DATABASE "ConvoyDb";
```

## Database Strukturasi

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL UNIQUE,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL
);
```

### Locations Table
```sql
CREATE TABLE "Locations" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Latitude" DOUBLE PRECISION NOT NULL,
    "Longitude" DOUBLE PRECISION NOT NULL,
    "Timestamp" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Speed" DOUBLE PRECISION,
    "Accuracy" DOUBLE PRECISION
);

CREATE INDEX "IX_Locations_UserId" ON "Locations" ("UserId");
CREATE INDEX "IX_Locations_Timestamp" ON "Locations" ("Timestamp");
```

## API Ishga Tushirish

```bash
# PostgreSQL serverini ishga tushiring
# Yoki Docker container'ni start qiling

# API ni ishga tushiring
dotnet run
```

API URL: `http://localhost:5084`
Swagger: `http://localhost:5084/swagger`

## PostgreSQL Connection Test

### pgAdmin orqali
1. pgAdmin ni oching
2. Server qo'shing:
   - Host: localhost
   - Port: 5432
   - Database: ConvoyDb
   - Username: postgres
   - Password: 2001

### psql CLI orqali
```bash
psql -h localhost -U postgres -d ConvoyDb
```

Keyin SQL query'lar:
```sql
-- Barcha userlarni ko'rish
SELECT * FROM "Users";

-- Barcha lokatsiyalarni ko'rish
SELECT * FROM "Locations" ORDER BY "Timestamp" DESC LIMIT 10;

-- User bilan birgalikda
SELECT u."Name", l."Latitude", l."Longitude", l."Timestamp"
FROM "Locations" l
JOIN "Users" u ON l."UserId" = u."Id"
ORDER BY l."Timestamp" DESC
LIMIT 10;
```

## Connection String Parametrlari

| Parameter | Description | Default |
|-----------|-------------|---------|
| Server/Host | PostgreSQL server address | localhost |
| Port | PostgreSQL port | 5432 |
| Database | Database nomi | ConvoyDb |
| User Id/Username | Username | postgres |
| Password | Parol | 2001 |
| Minimum Pool Size | Min connections | 10 |
| Maximum Pool Size | Max connections | 50 |
| Pooling | Connection pooling | true |
| Command Timeout | Query timeout (seconds) | 120 |

## NuGet Packages

Loyihada o'rnatilgan PostgreSQL packages:
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

## Muammolarni Hal Qilish

### 1. Connection xatosi
```
PostgreSQL Server is not running
```
**Yechim**: PostgreSQL serverini ishga tushiring yoki Docker container'ni start qiling
```bash
docker start convoy-postgres
```

### 2. Password xatosi
```
password authentication failed
```
**Yechim**: `appsettings.json` da password'ni tekshiring

### 3. Database yo'q
```
database "ConvoyDb" does not exist
```
**Yechim**: Database migration qo'llang
```bash
dotnet ef database update
```

### 4. Port band
```
port 5432 is already in use
```
**Yechim**: Boshqa PostgreSQL instance o'chiring yoki boshqa port ishlating

## Migration Commands

```bash
# Yangi migration yaratish
dotnet ef migrations add MigrationName

# Migration qo'llash
dotnet ef database update

# Oxirgi migration'ni bekor qilish
dotnet ef migrations remove

# Database o'chirish
dotnet ef database drop
```

## Production Deployment

### Docker Compose misoli
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: ConvoyDb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: YourSecurePassword
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  api:
    build: .
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=ConvoyDb;Username=postgres;Password=YourSecurePassword"
    ports:
      - "5084:8080"
    depends_on:
      - postgres

volumes:
  postgres_data:
```

### Environment Variable bilan
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=ConvoyDb;Username=postgres;Password=2001"
dotnet run
```

## Backup va Restore

### Backup
```bash
pg_dump -h localhost -U postgres ConvoyDb > backup.sql
```

### Restore
```bash
psql -h localhost -U postgres ConvoyDb < backup.sql
```

## Security

Production'da:
1. **Parolni o'zgartiring** - Default 2001 ishlatmang
2. **Environment variables** - Connection string'ni kod ichiga yozmang
3. **SSL/TLS** - PostgreSQL connection uchun SSL yoqing
4. **Firewall** - Faqat kerakli IP'larga ruxsat bering
5. **User permissions** - Minimal kerakli huquqlar bering

## PostGIS (Kelajakda)

Agar geo-spatial features kerak bo'lsa:
```sql
CREATE EXTENSION postgis;
```

Keyin migration'da:
```csharp
modelBuilder.HasPostgresExtension("postgis");
```

## Test Data

API ishga tushganda avtomatik 3 ta test user yaratiladi:
- Haydovchi 1: +998901234567
- Haydovchi 2: +998901234568
- Haydovchi 3: +998901234569

## Keyingi Qadamlar

Loyiha tayyor! Endi:
1. Flutter app yozishingiz mumkin
2. Admin panel (WPF/Web) yaratishingiz mumkin
3. SignalR qo'shishingiz mumkin (real-time)

Qo'shimcha savollar bo'lsa so'rang!
