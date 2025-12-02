# Visual Studio Browser Fix

## 🐛 Muammo

Visual Studio'dan run qilganda browser Swagger'ni ochmayotgan edi.

**Sabab:** `applicationUrl` `http://0.0.0.0:5084` bo'lganda, Visual Studio browser'ni `http://0.0.0.0:5084/swagger` bilan ochishga harakat qiladi, bu ishlamaydi.

## ✅ Yechim

`launchSettings.json` da `launchUrl` to'liq URL bilan o'zgartirildi:

### Oldingi (ishlamayotgan):
```json
{
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "http://0.0.0.0:5084"
}
```

VS browser'ni ochadi: `http://0.0.0.0:5084/swagger` ❌ (ishlamaydi)

### Yangi (ishlayotgan):
```json
{
  "launchBrowser": true,
  "launchUrl": "http://localhost:5084/swagger",
  "applicationUrl": "http://0.0.0.0:5084"
}
```

VS browser'ni ochadi: `http://localhost:5084/swagger` ✅ (ishlaydi!)

## 🔧 Tushuntirish

### applicationUrl
API qaysi interface'larda listen qilishini belgilaydi:
- `http://0.0.0.0:5084` - Barcha network interface'larda (localhost + 10.100.104.128)
- `http://localhost:5084` - Faqat localhost'da
- `http://10.100.104.128:5084` - Faqat network IP'da

**Bizda:** `http://0.0.0.0:5084` ishlatamiz (barcha interface'larda)

### launchUrl
Visual Studio run qilganda qaysi URL'ni ochishini belgilaydi:
- `"swagger"` - Relative path (applicationUrl'ga qo'shiladi)
- `"http://localhost:5084/swagger"` - Full URL (to'g'ri ishlaydi)

**Bizda:** `http://localhost:5084/swagger` ishlatamiz

## 🎯 Natija

Endi Visual Studio'dan run qilganda:
- ✅ API `http://0.0.0.0:5084` da listen qiladi
- ✅ Browser `http://localhost:5084/swagger` ni ochadi
- ✅ Localhost ishlaydi
- ✅ Network URL ham ishlaydi: `http://10.100.104.128:5084`

## 📋 Test Qilish

### 1. Visual Studio'da run qiling:
- "http" profile'ni tanlang
- Ctrl+F5 bosing

### 2. Browser avtomatik ochiladi:
```
http://localhost:5084/swagger
```

### 3. Network URL'ni qo'lda test qiling:
```
http://10.100.104.128:5084/swagger
```

### 4. Flutter'dan test qiling:
```dart
final response = await http.get(
  Uri.parse('http://10.100.104.128:5084/api/user'),
);
```

## 🔄 Alternative: Browser O'chirish

Agar browser avtomatik ochilishini xohlamasangiz:

```json
{
  "launchBrowser": false,
  "applicationUrl": "http://0.0.0.0:5084"
}
```

Keyin qo'lda browser'ni oching:
- Local: `http://localhost:5084/swagger`
- Network: `http://10.100.104.128:5084/swagger`

## 💡 Pro Tip

### Multiple Browser Windows
Agar VS har safar yangi browser window ochayotgan bo'lsa:

1. VS'da Tools > Options
2. Projects and Solutions > Web Projects
3. "Stop debugger when browser window is closed" ni belgining

Yoki browser'ni ochiq qoldiring, VS faqat tab ochadi.

## 📖 Related

- `launchSettings.json` - Launch configuration
- `VISUAL_STUDIO_SETUP.md` - VS setup guide
- `RUN_GUIDE.md` - Run instructions

---

**Muammo hal qilindi! VS endi to'g'ri ishlaydi!** ✅
