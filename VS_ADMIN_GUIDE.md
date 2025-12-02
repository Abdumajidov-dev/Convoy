# Visual Studio Admin Mode - Convoy API

## 🔴 Muammo

Visual Studio oddiy holatda ishga tushirilganda API faqat `localhost` da ishlaydi, lekin network IP manzilida (`10.100.104.128:5084`) ishlamaydi.

**Sabab:** Windows'da `http://0.0.0.0:5084` yoki network IP'da tinglash uchun Administrator huquqi kerak.

## ✅ Yechim

### Variant 1: Visual Studio'ni Administrator sifatida ishga tushirish (TAVSIYA ETILADI)

#### Windows 11/10:
1. **Visual Studio** ikonasini toping (Desktop yoki Start Menu)
2. **O'ng tugma** bosing
3. **"Run as administrator"** ni tanlang
4. UAC (User Account Control) dialog chiqadi - **"Yes"** bosing
5. Visual Studio administrator rejimida ochiladi

#### Har safar Administrator rejimida ochish:
1. Visual Studio ikonasini toping
2. **O'ng tugma** > **Properties**
3. **Compatibility** tabiga o'ting
4. ☑️ **"Run this program as an administrator"** ni belgilang
5. **Apply** > **OK**

**Endi har safar Visual Studio avtomatik administrator sifatida ochiladi!**

---

### Variant 2: Launch Profile tanlash

Loyihada endi 2 ta profile mavjud:

#### 1. `http (Admin Required)` - Network uchun ✅
- **Ishlaydi:** Network IP (`10.100.104.128:5084`) va localhost
- **Kerak:** Visual Studio admin rejimida bo'lishi kerak
- **Foydalanish:** Flutter app bilan test qilish uchun

```
applicationUrl: http://0.0.0.0:5084
```

#### 2. `http (localhost only)` - Faqat localhost
- **Ishlaydi:** Faqat localhost (`localhost:5084`)
- **Kerak emas:** Admin huquqi
- **Foydalanish:** API'ni test qilish uchun (Swagger)

```
applicationUrl: http://localhost:5084
```

### Qaysi profile tanlash?

**Visual Studio'da yuqorida:**
```
[▼ http (Admin Required)] [▶ Start]
```

- **Flutter app bilan ishlasangiz** → `http (Admin Required)` + VS admin rejimida
- **Faqat API test qilsangiz** → `http (localhost only)` + VS oddiy rejimda

---

### Variant 3: URL Reservation (Bir marta sozlash)

Administrator huquqisiz ishlatish uchun URL'ni Windows'ga ro'yxatdan o'tkazishingiz mumkin:

#### CMD (Administrator)da bajaring:

```cmd
netsh http add urlacl url=http://+:5084/ user=Everyone
```

yoki tayyorlangan scriptni ishlatib:

```cmd
setup-url-reservation.bat
```

**Keyin Visual Studio'ni oddiy holatda ishga tushirishingiz mumkin va network IP ishlaydi!**

#### URL Reservation o'chirish (agar kerak bo'lsa):
```cmd
netsh http delete urlacl url=http://+:5084/
```

---

## 🧪 Test qilish

### 1. Visual Studio'ni admin rejimida ishga tushiring
### 2. Profile: `http (Admin Required)` ni tanlang
### 3. **F5** yoki **Ctrl+F5** bosing

### 4. Output window'da quyidagilarni ko'rishingiz kerak:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5084
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 5. Browser avtomatik ochiladi:
```
http://10.100.104.128:5084/swagger
```

### 6. Ikkala URL ham ishlashi kerak:
- ✅ `http://localhost:5084/swagger`
- ✅ `http://10.100.104.128:5084/swagger`

### 7. Flutter'dan test qiling:
```dart
final response = await http.get(
  Uri.parse('http://10.100.104.128:5084/api/user'),
);
```

---

## 🔍 Muammolarni Tuzatish

### Muammo 1: "Permission denied" yoki "Access denied"
**Yechim:**
- Visual Studio admin rejimida ochilganini tekshiring
- Title bar'da "(Administrator)" yozuvi bo'lishi kerak
- Yoki URL Reservation qiling

### Muammo 2: Network IP ishlamayapti, localhost ishlayapti
**Sabab:** Visual Studio admin rejimida emas yoki noto'g'ri profile tanlangan

**Yechim:**
```
1. Visual Studio'ni yoping
2. O'ng tugma > Run as administrator
3. Profile: "http (Admin Required)" ni tanlang
4. F5 bosing
```

### Muammo 3: Firewall blocking
**Yechim:**
```cmd
setup-firewall.bat (Admin sifatida)
```

### Muammo 4: Port already in use
**Yechim:**
```cmd
# Qaysi process port ishlatyapti?
netstat -ano | findstr :5084

# Process'ni to'xtatish
taskkill /PID <process_id> /F
```

---

## 📋 Checklist

### Har safar Visual Studio'dan ishga tushirganda:

- [ ] PostgreSQL ishga tushganmi?
  ```cmd
  docker ps
  ```

- [ ] Visual Studio **admin rejimida** ochilganmi?
  - Title bar: "Visual Studio 2022 (Administrator)"

- [ ] To'g'ri profile tanlanganmi?
  - Network uchun: `http (Admin Required)`
  - Localhost uchun: `http (localhost only)`

- [ ] Firewall sozlanganmi? (birinchi marta)
  ```cmd
  setup-firewall.bat
  ```

### Run qilgandan keyin:

- [ ] Output: "Now listening on: http://0.0.0.0:5084"
- [ ] Browser Swagger'ni ochdi
- [ ] Localhost ishlayapti: `http://localhost:5084/swagger`
- [ ] Network IP ishlayapti: `http://10.100.104.128:5084/swagger`
- [ ] Flutter app'dan test qildingiz

---

## 🎯 Tavsiya

**Eng yaxshi yechim:**

1. **Visual Studio'ni har doim admin rejimida ochish:**
   - Properties > Compatibility > ☑️ Run as administrator

2. **URL Reservation sozlash:**
   ```cmd
   setup-url-reservation.bat
   ```

3. **Profile:** `http (Admin Required)` ni standart qilish

**Keyin har safar faqat F5 bosasiz va hammasi ishlaydi!** ✅

---

## 🔗 Bog'liq Fayllar

- `Properties/launchSettings.json` - Profile settings
- `setup-firewall.bat` - Firewall sozlash
- `setup-url-reservation.bat` - URL reservation
- `VISUAL_STUDIO_SETUP.md` - Visual Studio umumiy setup
- `FIREWALL_SETUP.md` - Firewall batafsil

---

## 💡 Qo'shimcha Ma'lumot

### Nima uchun Admin kerak?

Windows'da HTTP Server yaratish uchun quyidagi holatda admin kerak:
- `http://0.0.0.0:*` - Barcha network interfacelar
- `http://<IP>:*` - Maxsus IP manzil
- Port < 1024 - Well-known portlar

Admin **kerak emas:**
- `http://localhost:*` - Faqat local
- `http://127.0.0.1:*` - Loopback

### netsh http nima?

Windows HTTP Server API'si uchun CLI tool:
- URL reservation - Qaysi URL'ni kim ishlatishi mumkin
- SSL certificate binding
- Timeout settings

### Command Line'dan ishga tushirish

Admin CMD'da:
```cmd
cd C:\Users\abdum\OneDrive\Desktop\AdminPanel\Convoy.Api
dotnet run --launch-profile "http (Admin Required)"
```

Yoki oddiy CMD'da (URL reservation qilingan bo'lsa):
```cmd
dotnet run
```

---

**Visual Studio'ni admin rejimida ishga tushiring va network IP ishlaydi!** 🚀
