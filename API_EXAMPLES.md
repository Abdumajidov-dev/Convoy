# API Examples - Convoy API

## User API

### 1. Create User (Locations bo'lmaydi)

**Request:**
```http
POST http://localhost:5084/api/user
Content-Type: application/json

{
  "name": "Yangi Haydovchi",
  "phone": "+998901234570",
  "isActive": true
}
```

**Response:**
```json
{
  "id": 4,
  "name": "Yangi Haydovchi",
  "phone": "+998901234570",
  "isActive": true,
  "createdAt": "2024-12-02T09:00:00Z"
}
```

### 2. Get All Users

**Request:**
```http
GET http://localhost:5084/api/user
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Haydovchi 1",
    "phone": "+998901234567",
    "isActive": true,
    "createdAt": "2024-12-02T08:00:00Z"
  },
  {
    "id": 2,
    "name": "Haydovchi 2",
    "phone": "+998901234568",
    "isActive": true,
    "createdAt": "2024-12-02T08:00:00Z"
  }
]
```

### 3. Get Single User

**Request:**
```http
GET http://localhost:5084/api/user/1
```

**Response:**
```json
{
  "id": 1,
  "name": "Haydovchi 1",
  "phone": "+998901234567",
  "isActive": true,
  "createdAt": "2024-12-02T08:00:00Z"
}
```

### 4. Update User

**Request:**
```http
PUT http://localhost:5084/api/user/1
Content-Type: application/json

{
  "name": "Updated Name",
  "phone": "+998901234567",
  "isActive": true
}
```

**Response:**
```json
{
  "id": 1,
  "name": "Updated Name",
  "phone": "+998901234567",
  "isActive": true,
  "createdAt": "2024-12-02T08:00:00Z"
}
```

### 5. Delete User (Soft delete)

**Request:**
```http
DELETE http://localhost:5084/api/user/1
```

**Response:**
```
204 No Content
```

---

## Location API

### 1. Send Locations (Array)

**Request:**
```http
POST http://localhost:5084/api/location
Content-Type: application/json

[
  {
    "userId": 1,
    "timestamp": "2024-12-02 12:00:00",
    "latitude": "41.2995",
    "longitude": "69.2401"
  },
  {
    "userId": 1,
    "timestamp": "2024-12-02 12:00:30",
    "latitude": "41.2996",
    "longitude": "69.2402"
  },
  {
    "userId": 2,
    "timestamp": "2024-12-02 12:00:00",
    "latitude": "41.3111",
    "longitude": "69.2797"
  }
]
```

**Timestamp Format Options:**
- `"2024-12-02 12:00:00"` - Simple format
- `"2024-12-02T12:00:00"` - ISO format
- `"2025-12-02 14:30:13.798754+05"` - With timezone (auto-converted to UTC)
- `"2024-12-02 12:00:00.000"` - With milliseconds

**Response:**
```json
{
  "success": true,
  "savedCount": 3,
  "totalReceived": 3,
  "errors": null
}
```

### 2. Get User Location History

**Request:**
```http
GET http://localhost:5084/api/location/user/1?limit=10
```

**Response:**
```json
{
  "userId": 1,
  "count": 10,
  "locations": [
    {
      "id": 15,
      "userId": 1,
      "latitude": 41.2996,
      "longitude": 69.2402,
      "timestamp": "2024-12-02T12:00:30Z",
      "speed": null,
      "accuracy": null
    },
    {
      "id": 14,
      "userId": 1,
      "latitude": 41.2995,
      "longitude": 69.2401,
      "timestamp": "2024-12-02T12:00:00Z",
      "speed": null,
      "accuracy": null
    }
  ]
}
```

### 3. Get Latest Locations (All Users)

**Request:**
```http
GET http://localhost:5084/api/location/latest
```

**Response:**
```json
{
  "count": 3,
  "locations": [
    {
      "id": 15,
      "userId": 1,
      "latitude": 41.2996,
      "longitude": 69.2402,
      "timestamp": "2024-12-02T12:00:30Z",
      "speed": null,
      "accuracy": null,
      "user": {
        "id": 1,
        "name": "Haydovchi 1",
        "phone": "+998901234567",
        "isActive": true,
        "createdAt": "2024-12-02T08:00:00Z"
      }
    },
    {
      "id": 16,
      "userId": 2,
      "latitude": 41.3111,
      "longitude": 69.2797,
      "timestamp": "2024-12-02T12:00:00Z",
      "speed": null,
      "accuracy": null,
      "user": {
        "id": 2,
        "name": "Haydovchi 2",
        "phone": "+998901234568",
        "isActive": true,
        "createdAt": "2024-12-02T08:00:00Z"
      }
    }
  ]
}
```

---

## Curl Examples

### Create User
```bash
curl -X POST http://localhost:5084/api/user \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Haydovchi",
    "phone": "+998901234571",
    "isActive": true
  }'
```

### Get All Users
```bash
curl http://localhost:5084/api/user
```

### Send Locations
```bash
curl -X POST http://localhost:5084/api/location \
  -H "Content-Type: application/json" \
  -d '[
    {
      "userId": 1,
      "timestamp": "2024-12-02T12:00:00",
      "latitude": "41.2995",
      "longitude": "69.2401"
    }
  ]'
```

### Get User Locations
```bash
curl http://localhost:5084/api/location/user/1?limit=10
```

### Get Latest Locations
```bash
curl http://localhost:5084/api/location/latest
```

---

## Flutter Integration

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class ConvoyApi {
  static const baseUrl = 'http://YOUR_IP:5084';

  // Create User
  static Future<Map<String, dynamic>> createUser(String name, String phone) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/user'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'name': name,
        'phone': phone,
        'isActive': true,
      }),
    );

    if (response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception('User yaratib bo\'lmadi');
    }
  }

  // Send Location
  static Future<void> sendLocation(int userId, double lat, double lon) async {
    final now = DateTime.now();

    // Format: "2024-12-02 12:00:00" yoki ISO format
    final timestamp = now.toIso8601String(); // yoki: "${now.year}-${now.month.toString().padLeft(2,'0')}-${now.day.toString().padLeft(2,'0')} ${now.hour.toString().padLeft(2,'0')}:${now.minute.toString().padLeft(2,'0')}:${now.second.toString().padLeft(2,'0')}"

    final locations = [
      {
        'userId': userId,
        'timestamp': timestamp,
        'latitude': lat.toString(),
        'longitude': lon.toString(),
      }
    ];

    final response = await http.post(
      Uri.parse('$baseUrl/api/location'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(locations),
    );

    if (response.statusCode == 200) {
      print('Location sent: ${response.body}');
    } else {
      throw Exception('Lokatsiya yuborilmadi: ${response.body}');
    }
  }

  // Get User Locations
  static Future<List<dynamic>> getUserLocations(int userId, {int limit = 50}) async {
    final response = await http.get(
      Uri.parse('$baseUrl/api/location/user/$userId?limit=$limit'),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return data['locations'];
    } else {
      throw Exception('Lokatsiyalar olinmadi');
    }
  }
}
```

### Usage Example
```dart
void main() async {
  // Create user
  final user = await ConvoyApi.createUser('Test User', '+998901234572');
  print('User created: ${user['id']}');

  // Send location
  await ConvoyApi.sendLocation(user['id'], 41.2995, 69.2401);
  print('Location sent!');

  // Get locations
  final locations = await ConvoyApi.getUserLocations(user['id']);
  print('Total locations: ${locations.length}');
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Bu telefon raqami allaqachon ro'yxatdan o'tgan"
}
```

### 404 Not Found
```json
{
  "message": "User topilmadi"
}
```

### 500 Server Error
```json
{
  "message": "Server xatosi",
  "error": "Detailed error message"
}
```

---

## Important Notes

✅ **User Response:** `Locations` collection yo'q - faqat kerakli ma'lumotlar qaytadi
✅ **Create User:** Faqat name, phone, isActive kerak
✅ **Update User:** Faqat name, phone, isActive yangilanadi
✅ **Location Array:** Bir requestda ko'p lokatsiyalar yuborish mumkin
✅ **Timestamp:** ISO 8601 format (YYYY-MM-DDTHH:mm:ss)
✅ **Coordinates:** String formatda yuboriladi, backend parse qiladi

---

## Testing with Postman

1. Import collection yoki qo'lda endpoint'larni qo'shing
2. Environment variables:
   - `baseUrl`: `http://localhost:5084`
3. Har bir endpoint'ni test qiling
4. Response'larni tekshiring

**Swagger UI:** `http://localhost:5084/swagger` - Eng oson test usuli!
