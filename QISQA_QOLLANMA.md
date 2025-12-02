# Qisqa Qo'llanma

## API Ishga Tushirish

```bash
cd Convoy.Api
dotnet run
```

API ishga tushadi: `http://localhost:5084`

## Swagger orqali Test Qilish

1. Brauzerda oching: `http://localhost:5084/swagger`
2. Barcha endpoint'larni ko'rasiz

## Asosiy Endpoint'lar

### 1. Test Userlarni Ko'rish
```
GET http://localhost:5084/api/user
```

### 2. Lokatsiya Yuborish (Array ichida)
```
POST http://localhost:5084/api/location
Content-Type: application/json

[
  {
    "userId": 1,
    "timestamp": "2024-12-02T12:00:00",
    "latitude": "41.2995",
    "longitude": "69.2401"
  },
  {
    "userId": 2,
    "timestamp": "2024-12-02T12:00:30",
    "latitude": "41.3111",
    "longitude": "69.2797"
  }
]
```

### 3. User Lokatsiya Tarixini Ko'rish
```
GET http://localhost:5084/api/location/user/1?limit=50
```

### 4. Oxirgi Lokatsiyalar (Barcha Userlar)
```
GET http://localhost:5084/api/location/latest
```

## Curl bilan Test

### User Yaratish
```bash
curl -X POST http://localhost:5084/api/user \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Haydovchi","phone":"+998901234571","isActive":true}'
```

### Lokatsiya Yuborish
```bash
curl -X POST http://localhost:5084/api/location \
  -H "Content-Type: application/json" \
  -d '[{"userId":1,"timestamp":"2024-12-02T12:00:00","latitude":"41.2995","longitude":"69.2401"}]'
```

## Postman/Insomnia uchun

Import qilish uchun collection yaratish mumkin yoki to'g'ridan-to'g'ri Swagger UI dan foydalaning.

## Flutter Integration

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

// Base URL
const baseUrl = 'http://YOUR_IP:5084'; // Kompyuter IP manzilini kiriting

// Lokatsiya yuborish
Future<void> sendLocations() async {
  final locations = [
    {
      'userId': 1,
      'timestamp': DateTime.now().toIso8601String(),
      'latitude': '41.2995',
      'longitude': '69.2401',
    }
  ];

  final response = await http.post(
    Uri.parse('$baseUrl/api/location'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode(locations),
  );

  if (response.statusCode == 200) {
    print('Success: ${response.body}');
  } else {
    print('Error: ${response.statusCode}');
  }
}
```

## Muhim Eslatmalar

1. **Database**: Avtomatik LocalDB ishlatiladi (SQL Server Management Studio kerak emas)
2. **Test Data**: 3 ta test user avtomatik yaratiladi
3. **CORS**: Flutter/mobil app uchun CORS yoqilgan
4. **Port**: Default `http://localhost:5084`

## Keyingi Qadamlar

Agar SignalR (real-time) kerak bo'lsa yoki boshqa funksiyalar qo'shish kerak bo'lsa, aytib qo'ying!
