# Timestamp Format - PostgreSQL Fix

## Muammo

PostgreSQL `timestamp with time zone` faqat UTC vaqtni qabul qiladi. Agar local time yuborsangiz, xato chiqadi:

```
ArgumentException: Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported.
```

## Yechim

Backend avtomatik ravishda har qanday timestamp formatini UTC ga o'tkazadi.

## Qo'llab-quvvatlanadigan Formatlar

Endi siz **har qanday** formatda timestamp yuborishingiz mumkin:

### 1. ISO Format (Recommended)
```json
{
  "timestamp": "2024-12-02T12:00:00"
}
```

### 2. Simple Format
```json
{
  "timestamp": "2024-12-02 12:00:00"
}
```

### 3. With Timezone (avtomatik UTC ga o'tadi)
```json
{
  "timestamp": "2025-12-02 14:30:13.798754+05"
}
```

### 4. With Milliseconds
```json
{
  "timestamp": "2024-12-02 12:00:00.123"
}
```

### 5. ISO with Timezone
```json
{
  "timestamp": "2024-12-02T12:00:00+05:00"
}
```

## Test Misoli

### Test Data (Sizning yuborgan)
```json
[
  {
    "userId": 1,
    "timestamp": "2025-12-02 14:30:13.798754+05",
    "latitude": "40.39138291446486",
    "longitude": "71.78895924838807"
  }
]
```

**Bu endi ishlaydi!** Backend avtomatik UTC ga o'tkazadi.

### Curl Test
```bash
curl -X POST http://localhost:5084/api/location \
  -H "Content-Type: application/json" \
  -d '[
    {
      "userId": 1,
      "timestamp": "2025-12-02 14:30:13.798754+05",
      "latitude": "40.39138291446486",
      "longitude": "71.78895924838807"
    }
  ]'
```

## Flutter Integration

### Option 1: ISO Format (Simple)
```dart
final locations = [
  {
    'userId': 1,
    'timestamp': DateTime.now().toIso8601String(),
    'latitude': lat.toString(),
    'longitude': lon.toString(),
  }
];
```

### Option 2: Custom Format
```dart
final now = DateTime.now();
final timestamp = "${now.year}-${now.month.toString().padLeft(2,'0')}-${now.day.toString().padLeft(2,'0')} "
                  "${now.hour.toString().padLeft(2,'0')}:${now.minute.toString().padLeft(2,'0')}:${now.second.toString().padLeft(2,'0')}";

final locations = [
  {
    'userId': 1,
    'timestamp': timestamp,
    'latitude': lat.toString(),
    'longitude': lon.toString(),
  }
];
```

### Option 3: UTC (Best Practice)
```dart
final locations = [
  {
    'userId': 1,
    'timestamp': DateTime.now().toUtc().toIso8601String(),
    'latitude': lat.toString(),
    'longitude': lon.toString(),
  }
];
```

## Backend Qanday Ishlaydi

LocationController.cs:
```csharp
// Parse timestamp
if (!DateTime.TryParse(dto.Timestamp, out var timestamp))
{
    // error
}

// UTC ga o'tkazish
var utcTimestamp = timestamp.Kind == DateTimeKind.Utc
    ? timestamp
    : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);

// Saqlash
var location = new Location
{
    Timestamp = utcTimestamp
};
```

## Database'da Qanday Saqlanadi

PostgreSQL'da barcha vaqtlar UTC'da saqlanadi:
```sql
SELECT "Timestamp" FROM "Locations" LIMIT 1;
-- Result: 2024-12-02 12:00:00+00
```

## To'g'ri va Noto'g'ri

### ✅ To'g'ri (Ishlaydi)
```json
"timestamp": "2024-12-02T12:00:00"
"timestamp": "2024-12-02 12:00:00"
"timestamp": "2025-12-02 14:30:13.798754+05"
"timestamp": "2024-12-02T12:00:00Z"
"timestamp": "2024-12-02T12:00:00+00:00"
```

### ❌ Noto'g'ri (Format xatosi)
```json
"timestamp": "02-12-2024 12:00:00"  // Noto'g'ri format
"timestamp": "12/02/2024"          // Vaqt yo'q
"timestamp": "invalid"             // Parse bo'lmaydi
```

## Testing

1. API ni ishga tushiring:
```bash
dotnet run
```

2. Swagger'da test qiling:
```
http://localhost:5084/swagger
```

3. Location endpoint'ga boring va har qanday formatda timestamp yuboring

4. Success response kelishi kerak:
```json
{
  "success": true,
  "savedCount": 1,
  "totalReceived": 1,
  "errors": null
}
```

## Muhim Eslatma

Backend har qanday formatni qabul qiladi va UTC ga o'tkazadi. Siz faqat to'g'ri format yuborishingiz kerak (`DateTime.TryParse` parse qila oladigan format).

**Recommended**: ISO format ishlatish eng yaxshi:
```dart
DateTime.now().toIso8601String()
```
