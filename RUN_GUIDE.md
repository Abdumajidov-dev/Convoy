# Run Guide - To'g'ri Ishga Tushirish

## ⚠️ MUHIM: HTTP Profile Ishlatish

Default `dotnet run` HTTPS (port 7147) ishga tushiradi. Flutter uchun HTTP (port 5084) kerak!

## ✅ To'g'ri Usul

### HTTP bilan ishga tushirish:
```bash
dotnet run --launch-profile http
```

**Natija:**
```
Now listening on: http://0.0.0.0:5084
```

## ❌ Noto'g'ri Usul

### HTTPS (ishlamaydi Flutter uchun):
```bash
dotnet run  # Bu HTTPS ni ishga tushiradi
```

**Natija:**
```
Now listening on: https://localhost:7147  # ❌ Noto'g'ri!
```

## 🎯 Qisqa Qo'llanma

### 1. Backend ishga tushirish
```bash
cd C:\Users\abdum\OneDrive\Desktop\AdminPanel\Convoy.Api
dotnet run --launch-profile http
```

### 2. Tekshirish
Browser'da:
```
http://10.100.104.128:5084/swagger
```

### 3. Flutter'dan test
```dart
final response = await http.get(
  Uri.parse('http://10.100.104.128:5084/api/user'),
);
```

## 📋 Available Profiles

### 1. HTTP Profile (Flutter uchun)
```bash
dotnet run --launch-profile http
```
- Port: 5084
- Protocol: HTTP
- URL: `http://0.0.0.0:5084`

### 2. HTTPS Profile (Production)
```bash
dotnet run --launch-profile https
```
- Port: 7147 (HTTPS), 5084 (HTTP redirect)
- Protocol: HTTPS
- URL: `https://localhost:7147`

### 3. IIS Express
```bash
dotnet run --launch-profile "IIS Express"
```
- Port: 39738
- IIS Express bilan

## 🔧 launchSettings.json

Profile'lar `Properties/launchSettings.json` da:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://0.0.0.0:5084"  // ✅ Flutter uchun
    },
    "https": {
      "applicationUrl": "https://localhost:7147;http://localhost:5084"
    }
  }
}
```

## 🚀 Quick Start Script

### Windows (PowerShell):
```powershell
# start-api.ps1
Set-Location "C:\Users\abdum\OneDrive\Desktop\AdminPanel\Convoy.Api"
dotnet run --launch-profile http
```

Ishlatish:
```powershell
.\start-api.ps1
```

### Batch File:
```batch
@echo off
cd C:\Users\abdum\OneDrive\Desktop\AdminPanel\Convoy.Api
dotnet run --launch-profile http
pause
```

Saqlang: `start-api.bat`

## 🐛 Troubleshooting

### Port already in use
```
Error: Address already in use
```

**Yechim:**
```bash
# Running process'ni toping
netstat -ano | findstr :5084

# Process'ni to'xtating
taskkill /PID <process_id> /F

# Qayta ishga tushiring
dotnet run --launch-profile http
```

### HTTPS certificate error
Agar HTTPS kerak bo'lsa:
```bash
dotnet dev-certs https --trust
```

### Wrong port (7147 instead of 5084)
**Sabab:** HTTPS profile ishga tushgan

**Yechim:** `--launch-profile http` qo'shing

## 📱 Flutter Integration

### Base URL (HTTP bilan):
```dart
class ApiConfig {
  static const String baseUrl = 'http://10.100.104.128:5084';
}
```

### AndroidManifest.xml:
```xml
<application android:usesCleartextTraffic="true">
```

## 🔐 Production (HTTPS)

Production'da HTTPS ishlatish kerak:

### 1. SSL Certificate oling
- Let's Encrypt (bepul)
- Commercial SSL

### 2. HTTPS profile ishlatinig
```bash
dotnet run --launch-profile https
```

### 3. Flutter'da HTTPS:
```dart
static const String baseUrl = 'https://your-domain.com';
```

## ⚡ Environment Variables

Profile o'rniga environment variable:
```bash
# Windows
$env:ASPNETCORE_URLS="http://0.0.0.0:5084"
dotnet run

# Linux/Mac
export ASPNETCORE_URLS="http://0.0.0.0:5084"
dotnet run
```

## 📊 Status Check

API ishga tushgandan keyin:

### 1. Local test:
```bash
curl http://localhost:5084/api/user
```

### 2. Network test:
```bash
curl http://10.100.104.128:5084/api/user
```

### 3. Swagger:
```
http://10.100.104.128:5084/swagger
```

## 💡 Tips

1. **Always use HTTP profile** Flutter uchun
2. **Check port** - 5084 bo'lishi kerak
3. **Firewall** - port 5084 ochiq bo'lishi kerak
4. **Same network** - Flutter device bir xil WiFi'da
5. **Test first** - Browser'da test qilib, keyin Flutter'dan

## 🎓 Summary

| Scenario | Command | Port | Protocol |
|----------|---------|------|----------|
| **Flutter Development** | `dotnet run --launch-profile http` | 5084 | HTTP ✅ |
| Local HTTPS testing | `dotnet run --launch-profile https` | 7147 | HTTPS |
| Production | Configure web server | 443 | HTTPS |

---

**Flutter uchun har doim:** `dotnet run --launch-profile http` ✅
