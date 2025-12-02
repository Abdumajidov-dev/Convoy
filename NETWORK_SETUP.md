# Network Setup - Flutter uchun

## 🌐 Sizning IP: 10.100.104.128

## Tez Sozlash

### 1. Backend'ni ishga tushiring

**⚠️ MUHIM:** HTTP profile ishlatish kerak!

```bash
cd Convoy.Api
dotnet run --launch-profile http
```

**Yoki:** `start-api.bat` faylini ikki marta bosing

API quyidagi manzillarda ochiladi:
- **Network:** `http://10.100.104.128:5084`
- **Local:** `http://localhost:5084`
- **Swagger:** `http://10.100.104.128:5084/swagger`

### 2. Firewall sozlash (MUHIM!)

#### Windows PowerShell (Admin sifatida):
```powershell
New-NetFirewallRule -DisplayName "Convoy API Port 5084" -Direction Inbound -Protocol TCP -LocalPort 5084 -Action Allow
```

#### Yoki Windows Defender Firewall orqali:
1. Windows Security ni oching
2. Firewall & network protection
3. Advanced settings
4. Inbound Rules > New Rule
5. Port > TCP > 5084
6. Allow the connection
7. Name: "Convoy API"

### 3. Network Test

#### Browser'da:
```
http://10.100.104.128:5084/swagger
```

#### Curl bilan:
```bash
curl http://10.100.104.128:5084/api/user
```

## Flutter Configuration

### Base URL
```dart
class ApiConfig {
  static const String baseUrl = 'http://10.100.104.128:5084';
}
```

### AndroidManifest.xml
```xml
<manifest>
    <uses-permission android:name="android.permission.INTERNET" />

    <application
        android:usesCleartextTraffic="true">
        <!-- ... -->
    </application>
</manifest>
```

### Test Code
```dart
final response = await http.get(
  Uri.parse('http://10.100.104.128:5084/api/user'),
);

if (response.statusCode == 200) {
  print('✅ Connected!');
  print(response.body);
} else {
  print('❌ Error: ${response.statusCode}');
}
```

## Muammolarni Hal Qilish

### ❌ Connection Refused
**Sabab:** Backend ishlamayapti yoki firewall yopiq

**Yechim:**
1. Backend ishga tushganligini tekshiring
2. Firewall ochiq ekanligini tekshiring
3. Antivirus'ni vaqtincha o'chiring

### ❌ TimeoutException
**Sabab:** Network muammosi

**Yechim:**
1. Bir xil WiFi network'da ekanligingizni tekshiring
2. IP to'g'ri ekanligini tekshiring
3. Backend ishga tushganligini tekshiring

### ❌ CLEARTEXT not permitted
**Sabab:** Android HTTP'ni bloklayapti

**Yechim:** AndroidManifest.xml ga qo'shing:
```xml
<application android:usesCleartextTraffic="true">
```

## Test Checklist

Backend:
- [ ] `dotnet run` ishga tushganmi?
- [ ] `http://10.100.104.128:5084/swagger` ochilmoqdami?
- [ ] Firewall port 5084 ochiqmi?

Flutter:
- [ ] Base URL: `http://10.100.104.128:5084`
- [ ] Internet permission bormi?
- [ ] `usesCleartextTraffic="true"` qo'shilganmi?
- [ ] Bir xil WiFi network'damisiz?

## IP O'zgarsa

Agar kompyuter IP'si o'zgarsa (masalan, boshqa WiFi):

1. Yangi IP'ni toping:
```bash
# Windows
ipconfig

# Mac/Linux
ifconfig
```

2. Flutter'da BaseURL'ni yangilang
3. Backend'ni qayta ishga tushiring
4. Firewall qayta sozlang (agar kerak bo'lsa)

## Production

Production uchun:
- HTTPS qo'shing
- Domain name ishlating
- SSL certificate oling
- Firewall'ni specific IP'larga cheklang

## Quick Commands

```bash
# Backend ishga tushirish
dotnet run

# IP topish
ipconfig

# API test
curl http://10.100.104.128:5084/api/user

# Firewall status
netsh advfirewall firewall show rule name="Convoy API Port 5084"
```

---

**Flutter'da ishlatish uchun IP:** `http://10.100.104.128:5084`
