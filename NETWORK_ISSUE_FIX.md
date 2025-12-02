# Network URL Issue - Quick Fix

## 🐛 Muammo

Visual Studio'dan run qilganda:
- ✅ `http://localhost:5084` - Ishlaydi
- ❌ `http://10.100.104.128:5084` - **ISHLAMAYDI!**

Lekin `start-api.bat` dan run qilganda:
- ✅ Ikkala URL ham ishlaydi

## ⚡ Tez Yechim (2 ta variant)

### Variant 1: URL Reservation (RECOMMENDED)

**Bir marta qiling:**

`setup-url-reservation.bat` ni **Admin** sifatida ishga tushiring

**Natija:**
- ✅ VS'ni oddiy user sifatida ishlatishingiz mumkin
- ✅ Network URL ishlaydi
- ✅ Firewall allaqachon sozlangan

**Keyin:**
VS'ni oddiy ochib run qiling - hammasi ishlaydi!

---

### Variant 2: VS ni Admin Sifatida Ochish

**Har safar:**
1. Visual Studio'ni yoping
2. VS icon'ni o'ng tugma > **Run as administrator**
3. Run qiling

**Yoki doim admin sifatida:**
1. VS icon > Properties > Compatibility
2. ✅ "Run this program as an administrator"
3. OK

---

## 🎯 Qaysi Variantni Tanlash?

| Variant | Pros | Cons |
|---------|------|------|
| **URL Reservation** | Bir marta sozlash, keyin oddiy user | Script admin kerak |
| **VS Admin** | Darhol ishlaydi | Har safar admin |

**Tavsiya:** URL Reservation (Variant 1) ✅

---

## 📋 Step-by-Step (URL Reservation)

### 1. Setup
`setup-url-reservation.bat` ni **o'ng tugma** > **Run as administrator**

Ko'rasiz:
```
[SUCCESS] URL Reservation muvaffaqiyatli qo'shildi!
URL: http://*:5084/
User: Everyone
```

### 2. VS'ni Oching
Oddiy ochish - admin kerak emas!

### 3. Run Qiling
Ctrl+F5 bosing

### 4. Test
```
http://localhost:5084/swagger           ✅
http://10.100.104.128:5084/swagger      ✅
```

---

## 🧪 Test Qilish

### Browser'da:
```
http://10.100.104.128:5084/swagger
```

### Flutter'dan:
```dart
final response = await http.get(
  Uri.parse('http://10.100.104.128:5084/api/user'),
);
print(response.statusCode); // 200 bo'lishi kerak
```

### PowerShell:
```powershell
Invoke-WebRequest http://10.100.104.128:5084/api/user
```

---

## 🔧 URL Reservation Nima?

Windows HTTP.sys'ga aytadi:
> "Port 5084'da har kim (Everyone) HTTP server ishga tushirishi mumkin"

Bu admin huquqlarisiz `0.0.0.0:5084` da listen qilishga ruxsat beradi.

---

## 🗑️ URL Reservation O'chirish

Agar kerak bo'lmasa:

```powershell
# Admin PowerShell
netsh http delete urlacl url=http://*:5084/
```

---

## ✅ Checklist

- [ ] `setup-url-reservation.bat` Admin sifatida ishga tushirildi
- [ ] "SUCCESS" ko'rsatildi
- [ ] VS'ni oddiy ochganingizni tasdiqlang (admin emas)
- [ ] Run qilganingizni tasdiqlang
- [ ] `http://10.100.104.128:5084/swagger` ishlayaptimi?
- [ ] Flutter'dan test qildingizmi?

---

## 🆘 Hali Ham Ishlamasa

### 1. Firewall tekshiring:
```
setup-firewall.bat  # Admin sifatida
```

### 2. Network tekshiring:
```bash
ipconfig
# WiFi adapter'ni toping: 10.100.104.128
```

### 3. Telefon bir xil WiFi'dami?
Settings > WiFi > Check network name

### 4. Ping test:
```bash
ping 10.100.104.128
```

---

## 📖 Batafsil Ma'lumot

- `VS_ADMIN_FIX.md` - To'liq tushuntirish
- `FIREWALL_SETUP.md` - Firewall sozlash
- `NETWORK_SETUP.md` - Network troubleshooting

---

**Tez yechim: `setup-url-reservation.bat` ni Admin sifatida ishga tushiring!** ⚡
