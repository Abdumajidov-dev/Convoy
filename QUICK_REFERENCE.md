# Quick Reference Card - Convoy API

## 🚀 Tez Boshlash

### Visual Studio orqali:
1. `Convoy.Api.sln` ni oching
2. Yuqorida dropdown'dan **"http"** ni tanlang
3. **Ctrl+F5** bosing

### Terminal orqali:
```bash
dotnet run --launch-profile http
```

### Batch file:
`start-api.bat` ni ikki marta bosing

---

## 🔥 Firewall (Birinchi Marta)

`setup-firewall.bat` ni **o'ng tugma** > **Run as administrator**

---

## 🌐 URL'lar

| Service | URL |
|---------|-----|
| **Swagger** | http://10.100.104.128:5084/swagger |
| **Local** | http://localhost:5084/swagger |
| **Flutter** | http://10.100.104.128:5084 |

---

## 📡 API Endpoints

### Users
```
GET    /api/user              - Barcha userlar
GET    /api/user/{id}         - Bitta user
POST   /api/user              - User yaratish
PUT    /api/user/{id}         - User yangilash
DELETE /api/user/{id}         - User o'chirish
```

### Locations
```
POST   /api/location          - Lokatsiya yuborish (array)
GET    /api/location/user/{id}?limit=50  - User tarixi
GET    /api/location/latest   - Oxirgi lokatsiyalar
```

---

## 🧪 Test Examples

### Create User
```json
POST /api/user
{
  "name": "Test User",
  "phone": "+998901234570",
  "isActive": true
}
```

### Send Location
```json
POST /api/location
[
  {
    "userId": 1,
    "timestamp": "2024-12-02 12:00:00",
    "latitude": "41.2995",
    "longitude": "69.2401"
  }
]
```

---

## 📱 Flutter Integration

```dart
class ApiConfig {
  static const String baseUrl = 'http://10.100.104.128:5084';
}

// Test
final response = await http.get(
  Uri.parse('$baseUrl/api/user'),
);
```

**AndroidManifest.xml:**
```xml
<uses-permission android:name="android.permission.INTERNET" />
<application android:usesCleartextTraffic="true">
```

---

## 🗄️ Database

**Type:** PostgreSQL
**Name:** ConvoyDb
**Connection:**
```
Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;
```

### Docker:
```bash
docker run --name convoy-postgres \
  -e POSTGRES_PASSWORD=2001 \
  -e POSTGRES_DB=ConvoyDb \
  -p 5432:5432 \
  -d postgres:15
```

---

## 🐛 Troubleshooting

### API ishlamayapti?
```bash
# Port tekshirish
netstat -ano | findstr :5084

# Process'ni o'chirish
taskkill /F /IM Convoy.Api.exe
```

### Firewall issue?
```powershell
# Admin PowerShell
New-NetFirewallRule -DisplayName "Convoy API Port 5084" -Direction Inbound -Protocol TCP -LocalPort 5084 -Action Allow
```

### Flutter ulanolmayapti?
- [ ] API ishga tushganmi?
- [ ] Firewall ochiqmi?
- [ ] Bir xil WiFi'damisiz?
- [ ] `usesCleartextTraffic="true"` bormi?

### Port 7147 ishga tushyapti?
Visual Studio'da **"http"** profile'ni tanlamadingiz!

---

## 📋 Checklist

### Setup (Birinchi Marta):
- [ ] PostgreSQL ishga tushirish
- [ ] `setup-firewall.bat` (Admin)
- [ ] Visual Studio'da "http" tanlash
- [ ] Run qilish (Ctrl+F5)
- [ ] Swagger test qilish

### Development:
- [ ] PostgreSQL running
- [ ] API running (port 5084)
- [ ] Swagger works
- [ ] Flutter can connect

---

## 🎯 Common Commands

```bash
# Build
dotnet build

# Run (HTTP)
dotnet run --launch-profile http

# Migration
dotnet ef migrations add MigrationName
dotnet ef database update

# Restore packages
dotnet restore

# Clean
dotnet clean
```

---

## 📁 Important Files

| File | Purpose |
|------|---------|
| `Convoy.Api.sln` | Visual Studio solution |
| `start-api.bat` | Quick start script |
| `setup-firewall.bat` | Firewall setup |
| `appsettings.json` | Configuration |
| `launchSettings.json` | Run profiles |

---

## 📖 Documentation

| File | Content |
|------|---------|
| `START_HERE.md` | Quick start guide |
| `VISUAL_STUDIO_SETUP.md` | VS instructions |
| `FLUTTER_SETUP.md` | Flutter integration |
| `FIREWALL_SETUP.md` | Firewall details |
| `API_EXAMPLES.md` | API examples |

---

## 💡 Pro Tips

1. **Ctrl+F5** ishlatinig (Debug emas, tezroq)
2. Visual Studio'da **"http"** profile tanlang
3. Firewall'ni birinchi marta sozlang
4. Output window'ni tekshiring
5. Swagger'da test qiling

---

## 🆘 Help

**Muammo bo'lsa:**
1. Output window'ni tekshiring
2. Firewall ochiq ekanligini tekshiring
3. `TROUBLESHOOTING.md` ni o'qing (agar mavjud bo'lsa)
4. Documentation'ni tekshiring

**Eng kop uchraydigan muammolar:**
- Port 7147 (HTTPS) ishga tushyapti → "http" ni tanlang
- Flutter ulanolmayapti → Firewall ochish kerak
- Database error → PostgreSQL ishlamayapti

---

**Bu kartani saqlab qo'ying - tez reference uchun!** 📌
