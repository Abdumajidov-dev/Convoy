# Railway Deployment Guide - Convoy API

Bu qo'llanma Convoy API'ni Railway platformasiga deploy qilish bo'yicha to'liq ko'rsatmalarni o'z ichiga oladi.

## 📋 Tayyorgarlik

Railway'da deployment qilish uchun quyidagi fayllar yaratildi va sozlandi:

- ✅ `Dockerfile` - .NET 8.0 multi-stage build
- ✅ `railway.toml` - Railway konfiguratsiya fayli
- ✅ `.dockerignore` - Docker build uchun ignore patterns
- ✅ `Convoy.Api/appsettings.Production.json` - Production sozlamalar
- ✅ `Convoy.Api/Program.cs` - Environment variables va production CORS sozlamalari

## 🚀 Railway'ga Deploy Qilish Bosqichlari

### 1. Railway'da Yangi Proyekt Yaratish

1. [railway.app](https://railway.app/) saytiga o'ting va hisobingizga kiring
2. **"New Project"** tugmasini bosing
3. **"Deploy from GitHub repo"** ni tanlang
4. GitHub repositoriyangizni ulang va tanlang
5. **"Deploy Now"** tugmasini bosing

### 2. PostgreSQL Database Qo'shish

1. Proyektingiz ichida **"New"** → **"Database"** → **"Add PostgreSQL"** ni bosing
2. Railway avtomatik ravishda PostgreSQL database yaratadi
3. Database yaratilgandan so'ng, **environment variables** avtomatik qo'shiladi:
   - `PGHOST`
   - `PGPORT`
   - `PGUSER`
   - `PGPASSWORD`
   - `PGDATABASE`
   - `DATABASE_URL` (to'liq connection string)

### 3. Environment Variables Sozlash

Railway proyektingizda **"Variables"** bo'limiga o'ting va quyidagilarni qo'shing:

#### Majburiy Variables:

```bash
# Database connection (avtomatik qo'shiladi PostgreSQL qo'shganda)
DATABASE_URL=${DATABASE_URL}

# ASP.NET Core environment
ASPNETCORE_ENVIRONMENT=Production

# Port (Railway avtomatik belgilaydi)
PORT=8080
```

#### Ixtiyoriy Variables:

```bash
# CORS - frontend URL'lar (vergul bilan ajratilgan)
ALLOWED_ORIGINS=https://your-frontend.com,https://app.your-domain.com

# PHP API credentials (agar kerak bo'lsa)
PHP_API_URL=https://garant-hr.uz/api/
PHP_API_USERNAME=your_username
PHP_API_PASSWORD=your_password
```

**Eslatma:** Agar `ALLOWED_ORIGINS` o'rnatilmasa, barcha originlarga ruxsat beriladi (`*`).

### 4. Database Migration Qo'llash

Railway'da database migration qo'llash uchun ikki yo'l bor:

#### A usul: Railway CLI orqali (Tavsiya etiladi)

1. Railway CLI ni o'rnating:
```bash
npm i -g @railway/cli
# yoki
brew install railway
```

2. Railway proyektingizga login qiling:
```bash
railway login
railway link
```

3. Migration qo'llang:
```bash
railway run dotnet ef database update --project Convoy.Data --startup-project Convoy.Api
```

#### B usul: Local mashinadan Remote Database'ga

1. Railway'dan PostgreSQL connection ma'lumotlarini oling:
   - **"Variables"** bo'limidan `DATABASE_URL` ni ko'chirib oling

2. Local appsettings.Development.json'da connection string'ni o'zgartiring (vaqtincha):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgres:PASSWORD@containers-us-west-XXX.railway.app:PORT/railway"
  }
}
```

3. Migration qo'llang:
```bash
cd Convoy.Api
dotnet ef database update --project ../Convoy.Data
```

4. **Muhim:** Local connection string'ni qaytadan o'zgartiring!

### 5. Deploy Natijasini Tekshirish

1. **"Deployments"** bo'limida deploy holatini kuzating
2. Build muvaffaqiyatli tugagach, **"View Logs"** orqali loglarni tekshiring
3. Railway avtomatik URL beradi: `https://convoy-production-XXXX.up.railway.app`

### 6. API'ni Test Qilish

Deploy muvaffaqiyatli bo'lgach:

```bash
# Health check
curl https://your-railway-url.railway.app/swagger

# User list
curl https://your-railway-url.railway.app/api/user

# Location submission (SignalR)
# Flutter yoki web client orqali
wss://your-railway-url.railway.app/hubs/location
```

## 🔧 Production Sozlamalari

### Database Connection

Production'da connection string environment variable'dan olinadi:

```csharp
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
```

### CORS Policy

Production'da CORS ikkita rejimda ishlaydi:

1. **Agar `ALLOWED_ORIGINS` o'rnatilgan bo'lsa:**
   - Faqat belgilangan origin'larga ruxsat
   - SignalR uchun credentials enabled

2. **Agar `ALLOWED_ORIGINS` o'rnatilmagan bo'lsa:**
   - Barcha origin'larga ruxsat (`*`)
   - Hech qanday credential yoq

### HTTPS Redirection

Railway o'z proxy serveri orqali HTTPS ni boshqaradi, shuning uchun `UseHttpsRedirection()` production'da o'chirilgan:

```csharp
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
```

### Swagger Documentation

Production'da ham Swagger yoqilgan, lekin root URL'da emas:

```
https://your-railway-url.railway.app/swagger
```

## 📝 Railway Configuration Files

### railway.toml

```toml
[build]
builder = "DOCKERFILE"
dockerfilePath = "Dockerfile"

[deploy]
startCommand = "dotnet Convoy.Api.dll"
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 10
```

### Dockerfile

Multi-stage build:
- **Build stage:** .NET SDK 8.0 - dependencies restore va compile
- **Runtime stage:** .NET ASP.NET 8.0 - faqat runtime, kichik image size

Port 8080 da ishga tushadi (Railway PORT variable'i).

## 🐛 Troubleshooting

### 1. Build Fails

**Xatolik:** `Project file does not exist`

**Yechim:** `.dockerignore` faylda loyiha fayllari exclude qilinmaganligini tekshiring.

---

**Xatolik:** `Unable to restore packages`

**Yechim:**
- Internet ulanishini tekshiring
- NuGet source'larni tekshiring
- Railway build loglarini o'qing

### 2. Database Connection Error

**Xatolik:** `Connection refused` yoki `Password authentication failed`

**Yechim:**
- `DATABASE_URL` environment variable to'g'ri o'rnatilganligini tekshiring
- PostgreSQL service ishlab turganligini Railway dashboard'dan tekshiring
- Connection string formatini tekshiring:
  ```
  postgresql://user:password@host:port/database
  ```

### 3. Migration Fails

**Xatolik:** `The database does not exist`

**Yechim:**
- Railway PostgreSQL service yaratilganligini tekshiring
- DATABASE_URL to'g'ri connection string ekanligini tasdiqlang
- Railway CLI orqali migration qo'llang

### 4. CORS Errors

**Xatolik:** `No 'Access-Control-Allow-Origin' header`

**Yechim:**
- `ALLOWED_ORIGINS` environment variable'ini to'g'ri o'rnating
- Yoki uni butunlay o'chirib tashlang (barcha origin'larga ruxsat berish uchun)
- Frontend URL'da trailing slash (`/`) yo'qligini tekshiring

### 5. App Crashes After Deploy

**Yechim:**
- Railway **"Logs"** bo'limini tekshiring
- `ASPNETCORE_ENVIRONMENT=Production` o'rnatilganligini tasdiqlang
- Barcha zarur environment variables o'rnatilganligini tekshiring

## 📊 Monitoring va Logs

### Railway Dashboard

- **"Metrics"** - CPU, Memory, Network ishlatilishi
- **"Logs"** - Real-time application logs
- **"Deployments"** - Deploy tarixi va holati

### Application Logs

Production'da logging level:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🔐 Security Best Practices

### 1. Environment Variables

- **Hech qachon** parollar va connection string'larni code'ga qo'shmang
- Barcha sensitive data uchun environment variables ishlating
- `.env` fayllarni `.dockerignore` ga qo'shing

### 2. CORS Configuration

Production'da faqat kerakli origin'larga ruxsat bering:

```bash
ALLOWED_ORIGINS=https://your-app.com,https://admin.your-app.com
```

### 3. Database Credentials

Railway avtomatik ravishda secure PostgreSQL credentials yaratadi. Ularni o'zgartirmang.

## 🚀 Continuous Deployment

Railway avtomatik ravishda GitHub repository'ga push qilinganda deploy qiladi:

1. GitHub'da code o'zgartirishlar qiling
2. `git push origin main`
3. Railway avtomatik ravishda detect qiladi va deploy qiladi
4. **"Deployments"** bo'limida progressni kuzating

### Manual Deploy

Agar avtomatik deploy o'chirilgan bo'lsa:

1. Railway dashboard'da **"Deploy"** tugmasini bosing
2. Yoki Railway CLI orqali:
```bash
railway up
```

## 📱 Flutter/Mobile Client Configuration

Production URL'ni Flutter client'da yangilang:

```dart
// Old (Development)
final hubUrl = 'http://192.168.1.100:5084/hubs/location';

// New (Production)
final hubUrl = 'https://your-railway-url.railway.app/hubs/location';
```

SignalR WebSocket production'da `wss://` (secure WebSocket) ishlatadi.

## 🔄 Updates va Rollback

### Yangi Version Deploy Qilish

1. Code'ni update qiling
2. GitHub'ga push qiling
3. Railway avtomatik deploy qiladi

### Rollback (Oldingi Versiyaga Qaytish)

1. Railway dashboard'da **"Deployments"** bo'limiga o'ting
2. Oldingi muvaffaqiyatli deployment'ni toping
3. **"Redeploy"** tugmasini bosing

## 📞 Support va Qo'shimcha Ma'lumot

- **Railway Docs:** https://docs.railway.app/
- **Railway Discord:** https://discord.gg/railway
- **Convoy API Docs:** `CLAUDE.md` va `FLUTTER_SIGNALR_GUIDE.md`

---

## ✅ Deployment Checklist

Deploy qilishdan oldin:

- [ ] Barcha code changes commit va push qilingan
- [ ] PostgreSQL service Railway'da qo'shilgan
- [ ] `DATABASE_URL` environment variable o'rnatilgan
- [ ] `ASPNETCORE_ENVIRONMENT=Production` o'rnatilgan
- [ ] (Ixtiyoriy) `ALLOWED_ORIGINS` frontend URL'lari bilan o'rnatilgan
- [ ] Database migrations qo'llanilgan
- [ ] Railway build muvaffaqiyatli tugagan
- [ ] Swagger UI production'da ochiladi
- [ ] API endpoints test qilingan
- [ ] SignalR WebSocket ulanishi test qilingan
- [ ] Flutter/Mobile client production URL bilan yangilangan

**Deploy tayyor!** 🎉
