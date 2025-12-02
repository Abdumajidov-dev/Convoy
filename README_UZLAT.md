# Convoy API - Real-time Location Tracking Backend

Real-time haydovchi lokatsiyasini kuzatish tizimi uchun ASP.NET Core Web API backend.

## Texnologiyalar

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core 8.0
- **API Testing**: Swagger/OpenAPI

## Loyiha Strukturasi

```
Convoy.Api/
├── Controllers/
│   ├── LocationController.cs  # Lokatsiya operatsiyalari
│   └── UserController.cs       # User operatsiyalari
├── Models/
│   ├── User.cs                 # User modeli
│   ├── Location.cs             # Location modeli
│   └── LocationDto.cs          # DTO'lar
├── Data/
│   ├── AppDbContext.cs         # Database context
│   └── SeedData.cs             # Test ma'lumotlar
└── Program.cs                  # Application entry point
```

## O'rnatish va Ishga Tushirish

### 1. Database Migration (Allaqachon bajarilgan)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 2. Build va Run
```bash
dotnet build
dotnet run
```

API ishga tushgandan keyin:
- API: `https://localhost:7XXX` yoki `http://localhost:5XXX`
- Swagger UI: `https://localhost:7XXX/swagger`

## API Endpoints

### Location API

#### 1. Lokatsiyalarni yuborish (Array ichida)
```
POST /api/location
Content-Type: application/json

[
  {
    "userId": 1,
    "timestamp": "2024-12-02T12:00:00",
    "latitude": "41.2995",
    "longitude": "69.2401"
  },
  {
    "userId": 2,
    "timestamp": "2024-12-02T12:00:30",
    "latitude": "41.3111",
    "longitude": "69.2797"
  }
]
```

**Response:**
```json
{
  "success": true,
  "savedCount": 2,
  "totalReceived": 2,
  "errors": null
}
```

#### 2. User lokatsiya tarixini olish
```
GET /api/location/user/{userId}?limit=100
```

**Response:**
```json
{
  "userId": 1,
  "count": 50,
  "locations": [
    {
      "id": 1,
      "userId": 1,
      "latitude": 41.2995,
      "longitude": 69.2401,
      "timestamp": "2024-12-02T12:00:00",
      "speed": null,
      "accuracy": null
    }
  ]
}
```

#### 3. Barcha userlar uchun oxirgi lokatsiyalar
```
GET /api/location/latest
```

### User API

#### 1. Barcha userlarni olish
```
GET /api/user
```

#### 2. User yaratish
```
POST /api/user
Content-Type: application/json

{
  "name": "Yangi Haydovchi",
  "phone": "+998901234570",
  "isActive": true
}
```

#### 3. User ma'lumotlarini olish
```
GET /api/user/{id}
```

#### 4. User yangilash
```
PUT /api/user/{id}
```

#### 5. User o'chirish (Soft delete)
```
DELETE /api/user/{id}
```

## Test Ma'lumotlar

Loyiha birinchi marta ishga tushganda avtomatik ravishda 3 ta test user yaratiladi:
- **Haydovchi 1** - Phone: +998901234567
- **Haydovchi 2** - Phone: +998901234568
- **Haydovchi 3** - Phone: +998901234569

## Database

**Database**: `ConvoyDb` (SQL Server LocalDB)

**Jadvallar:**
- `Users` - Haydovchilar ma'lumotlari
- `Locations` - Lokatsiya ma'lumotlari

**Relationship:**
- User → Locations (One-to-Many)
- UserId Foreign Key bilan bog'langan
- Cascade delete yoqilgan

## Flutter Integration Misoli

```dart
// Location yuborish
Future<void> sendLocations(List<LocationData> locations) async {
  final url = 'http://your-api-url/api/location';

  final body = locations.map((loc) => {
    'userId': loc.userId,
    'timestamp': loc.timestamp.toIso8601String(),
    'latitude': loc.latitude.toString(),
    'longitude': loc.longitude.toString(),
  }).toList();

  final response = await http.post(
    Uri.parse(url),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode(body),
  );

  if (response.statusCode == 200) {
    print('Lokatsiyalar yuborildi');
  }
}
```

## Keyingi Qadamlar (Kengaytirish)

1. **SignalR Hub** - Real-time location broadcasting
2. **Authentication** - JWT token bilan autentifikatsiya
3. **Session Management** - Ish boshlash/tugatish funksiyasi
4. **Distance Calculation** - Session uchun masofa hisoblash
5. **Geofencing** - Hudud chegaralarini belgilash
6. **Background Jobs** - Ma'lumotlar tozalash va arxivlash
7. **Caching** - Redis bilan tezkor ma'lumotlar saqlash
8. **Logging** - Serilog bilan detailed logging

## Connection String O'zgartirish

Agar SQL Server o'rnatilgan bo'lsa, `appsettings.json` da connection string'ni o'zgartiring:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ConvoyDb;Trusted_Connection=true;"
  }
}
```

## Testing

Swagger UI orqali barcha endpoint'larni test qilish mumkin:
1. `dotnet run` bilan API'ni ishga tushiring
2. Brauzerda `https://localhost:7XXX/swagger` ochiladi
3. Har bir endpoint'ni "Try it out" tugmasi bilan test qiling

## Muammolar

Agar migration yoki database bilan muammo bo'lsa:
```bash
# Database'ni o'chirish
dotnet ef database drop

# Qayta yaratish
dotnet ef database update
```

## Yordam

Savollar yoki muammolar bo'lsa, repository Issues bo'limida savol bering.
