# Visual Studio Setup - Convoy API

## 🎯 Visual Studio orqali ishlatish

### 1️⃣ Loyihani Ochish

**Option 1: Solution File**
- `Convoy.Api.sln` ni ikki marta bosing
- Visual Studio avtomatik ochiladi

**Option 2: Visual Studio ichidan**
- File > Open > Project/Solution
- `Convoy.Api.sln` ni tanlang

### 2️⃣ Launch Profile Tanlash

Visual Studio'da yuqorida **Play tugmasi** yonida dropdown bor:

```
[▼ https] [▶ Start]
```

Bu dropdown'ni bosing va **"http"** ni tanlang:

```
[▼ http] [▶ Start]   ✅ To'g'ri!
```

**MUHIM:** "https" emas, "http" ni tanlang!

### 3️⃣ Run Qilish

**F5** yoki **▶ Start** tugmasini bosing

API ishga tushadi:
```
Now listening on: http://0.0.0.0:5084
```

Browser avtomatik Swagger'ni ochadi:
```
http://localhost:5084/swagger
```

## 🔧 Profile Settings

### launchSettings.json ni Ko'rish/O'zgartirish

1. Solution Explorer'da: `Properties` > `launchSettings.json`
2. Yoki: Project'ni o'ng tugma > Properties > Debug > General

**Available Profiles:**
- **http** ✅ - Flutter uchun (Port 5084)
- **https** - HTTPS bilan (Port 7147)
- **IIS Express** - IIS orqali

### Default Profile O'zgartirish

`launchSettings.json` ni oching va "http" profilni eng yuqoriga qo'ying yoki Visual Studio'da:

1. Project'ni o'ng tugma > **Properties**
2. **Debug** > **General**
3. **Open debug launch profiles UI**
4. "http" ni default qiling

## 🎮 Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **F5** | Start Debugging |
| **Ctrl+F5** | Start Without Debugging (Recommended) |
| **Shift+F5** | Stop Debugging |
| **Ctrl+Shift+F5** | Restart |

**Tip:** **Ctrl+F5** ishlatish yaxshiroq - debug overhead yo'q, tezroq ishlaydi.

## 📊 Output Windows

### 1. Output Window (Debug)
- View > Output (Ctrl+Alt+O)
- Dropdown: "Show output from: **Convoy.Api**"

### 2. Debug Console
- Debug > Windows > Output

### 3. Terminal
- View > Terminal (Ctrl+`)

## 🐛 Debugging

### Breakpoint Qo'yish
1. Code qatorining chap tomoniga bosing (qizil nuqta paydo bo'ladi)
2. F5 bosib debug mode'da run qiling
3. Request kelganda breakpoint'da to'xtaydi

### Hot Reload
Visual Studio 2022'da Hot Reload mavjud:
- Code o'zgartirsangiz, avtomatik yangilanadi
- To'xtatib qayta run qilish shart emas

## 🔥 Firewall (Birinchi Marta)

Visual Studio birinchi marta run qilganda Windows Firewall dialog chiqadi:

```
Windows Defender Firewall has blocked some features of this app
```

**"Allow access"** tugmasini bosing!

Agar dialog chiqmasa yoki "Cancel" bosgan bo'lsangiz:
- `setup-firewall.bat` ni **Admin** sifatida ishga tushiring

## 🌐 Network'da Ochish

### Visual Studio run qilgandan keyin:

**Local:**
```
http://localhost:5084/swagger
```

**Network (Flutter uchun):**
```
http://10.100.104.128:5084/swagger
```

Ikkala URL ham ishlaydi!

## 📋 Checklist

Run qilishdan oldin:
- [ ] PostgreSQL ishga tushganmi?
- [ ] "http" profile tanlanganmi?
- [ ] Firewall sozlanganmi? (birinchi marta uchun)

Run qilgandan keyin:
- [ ] Output window'da "Now listening on: http://0.0.0.0:5084" ko'rinmoqdami?
- [ ] Swagger ochildimi?
- [ ] Flutter'dan test qildingizmi?

## 🎓 Tips & Tricks

### 1. Multiple Startup Projects
Agar kelajakda Admin Panel (WPF) qo'shsangiz:
- Solution'ni o'ng tugma > **Set Startup Projects**
- Multiple startup projects'ni belgilang

### 2. Environment Variables
`launchSettings.json` da:
```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

### 3. Watch Window
Debug paytida o'zgaruvchilarni kuzatish:
- Debug > Windows > Watch > Watch 1

### 4. Immediate Window
Debug paytida code bajarish:
- Debug > Windows > Immediate (Ctrl+Alt+I)

## 🔄 Package Restore

Agar NuGet packages yuklanmasa:
1. Solution'ni o'ng tugma > **Restore NuGet Packages**
2. Yoki: Tools > NuGet Package Manager > Package Manager Console
   ```powershell
   dotnet restore
   ```

## 🚀 Quick Start (VS orqali)

### Birinchi Marta:
1. `Convoy.Api.sln` ni oching
2. `setup-firewall.bat` ni Admin sifatida ishga tushiring
3. Visual Studio'da "http" ni tanlang
4. **Ctrl+F5** bosing

### Keyingi safar:
1. Visual Studio'ni oching (yaqinda ochilgan fayllar ro'yxatida bo'ladi)
2. **Ctrl+F5** bosing

That's it! ✅

## ⚙️ Configuration

### appsettings.json
Development settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ConvoyDb;User Id=postgres;Password=2001;Port=5432;"
  }
}
```

### Change Port
Agar port o'zgartirmoqchi bo'lsangiz:
1. `Properties/launchSettings.json` oching
2. "http" profile'da:
   ```json
   "applicationUrl": "http://0.0.0.0:5084"
   ```
   Bu qatorni o'zgartiring (masalan: 5085)

## 🐛 Common Issues

### Issue 1: "Project is out of date"
**Yechim:** Build > Rebuild Solution

### Issue 2: Port already in use
**Yechim:**
- Stop qiling (Shift+F5)
- Yoki Task Manager'da Convoy.Api.exe'ni o'chiring

### Issue 3: NuGet packages yuklanmayapti
**Yechim:**
- Tools > Options > NuGet Package Manager
- Package Sources'ni tekshiring
- "Restore NuGet Packages on build" yoqing

### Issue 4: Swagger ochilmayapti
**Yechim:**
- Output window'ni tekshiring
- Browser'da qo'lda oching: `http://localhost:5084/swagger`

## 📱 Flutter Testing (VS ishlaganda)

Visual Studio'da API run qilib:
1. Flutter'dan request yuboring
2. Visual Studio Output window'da request'ni ko'rasiz
3. Breakpoint qo'yib debug qilishingiz mumkin!

## 🎉 Production Build

### Release Mode:
1. Yuqorida "Debug" o'rniga **"Release"** ni tanlang
2. Build > Build Solution
3. Output: `bin/Release/net8.0/`

### Publish:
1. Project'ni o'ng tugma > **Publish**
2. Target: Folder, IIS, Azure, Docker...
3. Settings'ni sozlang
4. **Publish** tugmasini bosing

---

**Visual Studio bilan ishlatish oson! Faqat "http" ni tanlab F5 bosing!** ✅
