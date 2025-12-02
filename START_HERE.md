# Convoy API - Tez Boshlash

## 🚀 Loyiha Tayyor!

Loyiha PostgreSQL bilan to'liq ishlaydigan holatda.

## ⚡ Tez Start

### 1. PostgreSQL ishga tushiring
```bash
# Docker bilan (Recommended)
docker run --name convoy-postgres \
  -e POSTGRES_PASSWORD=2001 \
  -e POSTGRES_DB=ConvoyDb \
  -p 5432:5432 \
  -d postgres:15

# Yoki Windows'da o'rnatilgan PostgreSQL ishlatishingiz mumkin
```

### 2. Firewall sozlang (MUHIM - Flutter uchun!)
`setup-firewall.bat` faylini o'ng tugma > **Run as administrator**

### 3. API'ni ishga tushiring
```bash
cd Convoy.Api
dotnet run --launch-profile http
```

**Yoki:** `start-api.bat` faylini ikki marta bosing

### 4. Browser'da oching
```
http://10.100.104.128:5084/swagger
```

## 📁 Muhim Fayllar

| Fayl | Tavsif |
|------|--------|
| `QISQA_QOLLANMA.md` | API endpoints va test usullari |
| `POSTGRESQL_SETUP.md` | PostgreSQL sozlash va troubleshooting |
| `README_UZLAT.md` | To'liq dokumentatsiya |
| `LOYIHA_TUZILISHI.md` | Loyiha strukturasi |

## 🔧 Sozlamalar

### Connection String
`appsettings.json` va `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;"
  }
}
```

**Password o'zgartirish uchun**: Yuqoridagi faylda `Password=2001` ni o'zgartiring.

## 📡 API Test Qilish

### 1. Swagger UI
Browser'da: `http://localhost:5084/swagger`

### 2. Test User'larni ko'rish
```bash
curl http://localhost:5084/api/user
```

### 3. Lokatsiya yuborish
```bash
curl -X POST http://localhost:5084/api/location \
  -H "Content-Type: application/json" \
  -d '[
    {
      "userId": 1,
      "timestamp": "2024-12-02T12:00:00",
      "latitude": "41.2995",
      "longitude": "69.2401"
    }
  ]'
```

## 🎯 Asosiy Endpoint'lar

| Method | URL | Tavsif |
|--------|-----|--------|
| GET | `/api/user` | Barcha userlar |
| POST | `/api/location` | Lokatsiya yuborish (array) |
| GET | `/api/location/user/{id}` | User tarixi |
| GET | `/api/location/latest` | Oxirgi lokatsiyalar |

## 🗄️ Database

- **Type**: PostgreSQL
- **Name**: ConvoyDb
- **Tables**: Users, Locations
- **Test Data**: 3 ta user avtomatik yaratiladi

### PostgreSQL'ga kirish (psql)
```bash
psql -h localhost -U postgres -d ConvoyDb
```

### Query misollari
```sql
-- Userlar
SELECT * FROM "Users";

-- Lokatsiyalar
SELECT * FROM "Locations" ORDER BY "Timestamp" DESC LIMIT 10;
```

## 🐛 Muammolar

### PostgreSQL ishlamayotgan bo'lsa
```bash
# Docker container'ni start qiling
docker start convoy-postgres

# Yoki yangi container yarating
docker run --name convoy-postgres -e POSTGRES_PASSWORD=2001 -p 5432:5432 -d postgres:15
```

### Database migration kerak bo'lsa
```bash
dotnet ef database update
```

### Port band bo'lsa
`Properties/launchSettings.json` da portni o'zgartiring

## 📱 Flutter Integration

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

const baseUrl = 'http://YOUR_IP:5084';

Future<void> sendLocation(int userId, double lat, double lon) async {
  final locations = [
    {
      'userId': userId,
      'timestamp': DateTime.now().toIso8601String(),
      'latitude': lat.toString(),
      'longitude': lon.toString(),
    }
  ];

  final response = await http.post(
    Uri.parse('$baseUrl/api/location'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode(locations),
  );

  print('Response: ${response.body}');
}
```

## 🔄 Migration Commands

```bash
# Yangi migration
dotnet ef migrations add MigrationName

# Database yangilash
dotnet ef database update

# Database o'chirish
dotnet ef database drop
```

## 📊 Loyiha Strukturasi

```
Convoy.Api/
├── Controllers/
│   ├── LocationController.cs    # POST/GET lokatsiyalar
│   └── UserController.cs         # User CRUD
├── Models/
│   ├── User.cs                   # User entity
│   ├── Location.cs               # Location entity
│   └── LocationDto.cs            # DTO
├── Data/
│   ├── AppDbContext.cs           # EF Core context
│   └── SeedData.cs               # Test data
├── Migrations/                   # Database migrations
└── Program.cs                    # App configuration
```

## 🎓 Keyingi Qadamlar

1. **Flutter App** - Mobil ilovani yaratish
2. **Admin Panel** - WPF yoki Web panel
3. **SignalR** - Real-time updates qo'shish
4. **Authentication** - JWT token qo'shish
5. **Session Tracking** - Ish vaqtini kuzatish

## 💡 Foydali Link'lar

- [PostgreSQL Setup](./POSTGRESQL_SETUP.md) - Database sozlash
- [Qisqa Qo'llanma](./QISQA_QOLLANMA.md) - API endpoints
- [To'liq README](./README_UZLAT.md) - Batafsil ma'lumot

## ⚠️ Production'ga chiqishdan oldin

1. ✅ Password'ni o'zgartiring
2. ✅ CORS policy'ni cheklang
3. ✅ Connection string'ni environment variable'ga o'tkazing
4. ✅ SSL/TLS qo'shing
5. ✅ Logging'ni sozlang

## 🆘 Yordam

Savollar bo'lsa yoki muammo bo'lsa:
1. `POSTGRESQL_SETUP.md` ga qarang
2. `QISQA_QOLLANMA.md` da misollar bor
3. GitHub Issues ochishingiz mumkin

---

**Loyiha tayyor! API'ni ishga tushiring va test qiling!** 🎉
