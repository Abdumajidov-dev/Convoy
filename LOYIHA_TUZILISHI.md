# Convoy API - Loyiha Tuzilishi

## Yaratilgan Fayl va Papkalar

```
Convoy.Api/
├── Controllers/
│   ├── LocationController.cs      # Lokatsiya API (POST, GET)
│   └── UserController.cs           # User CRUD operatsiyalari
│
├── Models/
│   ├── User.cs                     # User entity
│   ├── Location.cs                 # Location entity
│   └── LocationDto.cs              # Data transfer objects
│
├── Data/
│   ├── AppDbContext.cs             # EF Core DbContext
│   └── SeedData.cs                 # Test ma'lumotlar
│
├── Migrations/                     # Database migrations
│   └── 20241202074302_InitialCreate.cs
│
├── Program.cs                      # App konfiguratsiya
├── appsettings.json                # Connection string va sozlamalar
├── Convoy.Api.csproj               # NuGet packages
│
└── Dokumentatsiya/
    ├── README_UZLAT.md             # To'liq dokumentatsiya
    ├── QISQA_QOLLANMA.md           # Tezkor qo'llanma
    └── LOYIHA_TUZILISHI.md         # Ushbu fayl
```

## Texnologiyalar

- **.NET 8.0** - Framework
- **ASP.NET Core Web API** - Backend
- **Entity Framework Core 8.0** - ORM
- **SQL Server LocalDB** - Database
- **Swagger/OpenAPI** - API documentation

## Database Schema

### Users Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK, Identity) | Unique ID |
| Name | nvarchar(100) | Haydovchi ismi |
| Phone | nvarchar(20) | Telefon raqami (Unique) |
| IsActive | bit | Aktiv holat |
| CreatedAt | datetime2 | Yaratilgan vaqt |

### Locations Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK, Identity) | Unique ID |
| UserId | int (FK) | User ID |
| Latitude | float | Kenglik |
| Longitude | float | Uzunlik |
| Timestamp | datetime2 | Vaqt |
| Speed | float (nullable) | Tezlik |
| Accuracy | float (nullable) | Aniqlik |

## API Endpoints

### Location Controller (`/api/location`)

1. **POST** `/api/location`
   - Lokatsiyalar yuborish (array ichida)
   - Request body: `List<LocationDto>`
   - Response: Success count va errors

2. **GET** `/api/location/user/{userId}`
   - User lokatsiya tarixi
   - Query parameter: `limit` (default: 100)
   - Response: User locations list

3. **GET** `/api/location/latest`
   - Barcha userlar uchun oxirgi lokatsiyalar
   - Response: Latest locations grouped by user

### User Controller (`/api/user`)

1. **GET** `/api/user`
   - Barcha aktiv userlar

2. **GET** `/api/user/{id}`
   - Bitta user ma'lumotlari

3. **POST** `/api/user`
   - Yangi user yaratish

4. **PUT** `/api/user/{id}`
   - User yangilash

5. **DELETE** `/api/user/{id}`
   - User o'chirish (soft delete)

## Asosiy Features

✅ **Array ichida objectlar qabul qilish** - Bir requestda ko'p lokatsiyalar
✅ **String formatda lat/long qabul qilish** - Automatic parsing
✅ **Error handling** - Har bir lokatsiya uchun alohida error handling
✅ **Batch processing** - Ko'p ma'lumotlarni bir vaqtda saqlash
✅ **CORS support** - Flutter/mobile app uchun
✅ **Test data seeding** - Avtomatik test userlar yaratish
✅ **Swagger UI** - API testing va documentation
✅ **Logging** - Built-in .NET logging
✅ **Database indexing** - Tezkor qidiruv uchun

## Ishlatish

### 1. API Ishga Tushirish
```bash
cd Convoy.Api
dotnet run
```

### 2. Swagger'da Test Qilish
Browser'da: `http://localhost:5084/swagger`

### 3. Flutter Integration
```dart
// Lokatsiya yuborish misoli
final response = await http.post(
  Uri.parse('http://YOUR_IP:5084/api/location'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode([
    {
      'userId': 1,
      'timestamp': DateTime.now().toIso8601String(),
      'latitude': position.latitude.toString(),
      'longitude': position.longitude.toString(),
    }
  ]),
);
```

## Test Ma'lumotlar

API birinchi ishga tushganda 3 ta test user yaratadi:

1. **Haydovchi 1** - +998901234567
2. **Haydovchi 2** - +998901234568
3. **Haydovchi 3** - +998901234569

Bu userlar ID: 1, 2, 3 bilan lokatsiya yuborish uchun foydalanishingiz mumkin.

## Keyingi Bosqichlar

Loyihani kengaytirish uchun qo'shilishi mumkin:

1. **SignalR Hub** - Real-time location broadcasting (Admin panel uchun)
2. **Authentication** - JWT token autentifikatsiya
3. **Session Management** - Ish boshlash/tugatish tracking
4. **Geofencing** - Hudud chegaralarini belgilash
5. **Distance Calculation** - Session davomida masofa hisoblash
6. **Background Services** - Eski ma'lumotlarni arxivlash
7. **Redis Caching** - Tezkor oxirgi lokatsiya caching
8. **Push Notifications** - Haydovchilarga bildirishnomalar

## Muammolar va Yechimlar

### Database xatosi
```bash
dotnet ef database drop
dotnet ef database update
```

### Port band
`Properties/launchSettings.json` da portni o'zgartiring

### Migration xatosi
```bash
dotnet ef migrations remove
dotnet ef migrations add NewMigration
dotnet ef database update
```

## Xulosa

Backend tayyor! Asosiy funksiyalar:
- Lokatsiya qabul qilish va saqlash
- User CRUD operatsiyalari
- Test ma'lumotlar
- API documentation

Keyingi qadam: Flutter app yoki Admin panel yaratish.
