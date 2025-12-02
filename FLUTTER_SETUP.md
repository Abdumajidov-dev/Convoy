# Flutter Integration - Convoy API

## Network Configuration

### Backend IP Address
Sizning kompyuter IP: **10.100.104.128**

API URL: `http://10.100.104.128:5084`

## Backend Setup

### 1. API'ni barcha network'da ochish
`launchSettings.json` allaqachon sozlangan:
```json
"applicationUrl": "http://0.0.0.0:5084"
```

### 2. API'ni ishga tushiring
```bash
cd Convoy.Api
dotnet run
```

API quyidagi manzillarda ochiladi:
- Local: `http://localhost:5084`
- Network: `http://10.100.104.128:5084`
- Any IP: `http://0.0.0.0:5084`

### 3. Firewall sozlash (Windows)
Agar Flutter'dan ulanib bo'lmasa:

```powershell
# PowerShell (Admin sifatida)
New-NetFirewallRule -DisplayName "Convoy API" -Direction Inbound -Protocol TCP -LocalPort 5084 -Action Allow
```

Yoki Windows Defender Firewall orqali:
1. Windows Security > Firewall & network protection
2. Advanced settings
3. Inbound Rules > New Rule
4. Port: TCP 5084
5. Allow the connection

## Flutter Integration

### Base Configuration

```dart
class ApiConfig {
  // Production IP (sizning kompyuter)
  static const String baseUrl = 'http://10.100.104.128:5084';

  // Local testing (faqat emulator uchun)
  static const String localUrl = 'http://localhost:5084';

  // Android emulator uchun
  static const String emulatorUrl = 'http://10.0.2.2:5084';
}
```

### API Service Class

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class ConvoyApiService {
  static const String baseUrl = 'http://10.100.104.128:5084';

  // Create User
  static Future<Map<String, dynamic>> createUser({
    required String name,
    required String phone,
  }) async {
    try {
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
        throw Exception('User yaratib bo\'lmadi: ${response.body}');
      }
    } catch (e) {
      print('Error creating user: $e');
      rethrow;
    }
  }

  // Send Location
  static Future<Map<String, dynamic>> sendLocation({
    required int userId,
    required double latitude,
    required double longitude,
  }) async {
    try {
      final locations = [
        {
          'userId': userId,
          'timestamp': DateTime.now().toIso8601String(),
          'latitude': latitude.toString(),
          'longitude': longitude.toString(),
        }
      ];

      final response = await http.post(
        Uri.parse('$baseUrl/api/location'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode(locations),
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      } else {
        throw Exception('Lokatsiya yuborilmadi: ${response.body}');
      }
    } catch (e) {
      print('Error sending location: $e');
      rethrow;
    }
  }

  // Get User Locations
  static Future<List<dynamic>> getUserLocations({
    required int userId,
    int limit = 50,
  }) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/location/user/$userId?limit=$limit'),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['locations'] as List;
      } else {
        throw Exception('Lokatsiyalar olinmadi: ${response.body}');
      }
    } catch (e) {
      print('Error getting locations: $e');
      rethrow;
    }
  }

  // Get Latest Locations (All Users)
  static Future<List<dynamic>> getLatestLocations() async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/location/latest'),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['locations'] as List;
      } else {
        throw Exception('Latest lokatsiyalar olinmadi: ${response.body}');
      }
    } catch (e) {
      print('Error getting latest locations: $e');
      rethrow;
    }
  }

  // Test Connection
  static Future<bool> testConnection() async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/user'),
      ).timeout(const Duration(seconds: 5));

      return response.statusCode == 200;
    } catch (e) {
      print('Connection test failed: $e');
      return false;
    }
  }
}
```

### pubspec.yaml
```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  geolocator: ^10.1.0  # Location uchun
```

### Permission Setup

#### Android (android/app/src/main/AndroidManifest.xml)
```xml
<manifest>
    <!-- Internet permission -->
    <uses-permission android:name="android.permission.INTERNET" />

    <!-- Location permissions -->
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />

    <application
        android:usesCleartextTraffic="true">
        <!-- ... -->
    </application>
</manifest>
```

#### iOS (ios/Runner/Info.plist)
```xml
<dict>
    <key>NSLocationWhenInUseUsageDescription</key>
    <string>Lokatsiyangizni kuzatish uchun ruxsat kerak</string>

    <key>NSLocationAlwaysUsageDescription</key>
    <string>Doimiy lokatsiyani kuzatish uchun ruxsat kerak</string>

    <key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
    <string>Lokatsiya kuzatish uchun ruxsat kerak</string>
</dict>
```

## Example Usage

### Simple Test
```dart
import 'package:flutter/material.dart';

class TestApiScreen extends StatefulWidget {
  @override
  _TestApiScreenState createState() => _TestApiScreenState();
}

class _TestApiScreenState extends State<TestApiScreen> {
  String _status = 'Not tested';

  Future<void> _testConnection() async {
    setState(() => _status = 'Testing...');

    try {
      final isConnected = await ConvoyApiService.testConnection();
      setState(() {
        _status = isConnected ? '✅ Connected!' : '❌ Connection failed';
      });
    } catch (e) {
      setState(() => _status = '❌ Error: $e');
    }
  }

  Future<void> _sendTestLocation() async {
    setState(() => _status = 'Sending location...');

    try {
      final result = await ConvoyApiService.sendLocation(
        userId: 1,
        latitude: 40.39138291446486,
        longitude: 71.78895924838807,
      );

      setState(() {
        _status = '✅ Location sent! Saved: ${result['savedCount']}';
      });
    } catch (e) {
      setState(() => _status = '❌ Error: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('API Test')),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(_status, style: TextStyle(fontSize: 18)),
            SizedBox(height: 20),
            ElevatedButton(
              onPressed: _testConnection,
              child: Text('Test Connection'),
            ),
            SizedBox(height: 10),
            ElevatedButton(
              onPressed: _sendTestLocation,
              child: Text('Send Test Location'),
            ),
          ],
        ),
      ),
    );
  }
}
```

### Background Location Tracking
```dart
import 'package:geolocator/geolocator.dart';
import 'dart:async';

class LocationService {
  Timer? _timer;
  int userId;

  LocationService({required this.userId});

  // Start tracking (har 30 sekundda)
  void startTracking() {
    _timer = Timer.periodic(Duration(seconds: 30), (timer) async {
      await _sendCurrentLocation();
    });
  }

  // Stop tracking
  void stopTracking() {
    _timer?.cancel();
  }

  // Send current location
  Future<void> _sendCurrentLocation() async {
    try {
      // Permission check
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
      }

      if (permission == LocationPermission.denied ||
          permission == LocationPermission.deniedForever) {
        print('Location permission denied');
        return;
      }

      // Get current position
      Position position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );

      // Send to API
      await ConvoyApiService.sendLocation(
        userId: userId,
        latitude: position.latitude,
        longitude: position.longitude,
      );

      print('Location sent: ${position.latitude}, ${position.longitude}');
    } catch (e) {
      print('Error sending location: $e');
    }
  }
}
```

## Testing Checklist

### Backend
- [ ] API ishga tushganmi? (`dotnet run`)
- [ ] Firewall ochiqmi? (port 5084)
- [ ] `http://10.100.104.128:5084/swagger` ochilmoqdami?

### Flutter
- [ ] `http` package o'rnatilganmi?
- [ ] Base URL to'g'rimi? (`http://10.100.104.128:5084`)
- [ ] Internet permission bormi? (AndroidManifest.xml)
- [ ] `android:usesCleartextTraffic="true"` qo'shilganmi?
- [ ] Test connection ishlayaptimi?

## Common Issues

### 1. Connection Refused
**Problem**: Flutter ulanolmayapti

**Solution**:
- Backend ishga tushganligini tekshiring
- IP to'g'ri ekanligini tekshiring: `ipconfig` (Windows) yoki `ifconfig` (Mac/Linux)
- Firewall port 5084 ochiq ekanligini tekshiring
- Bir xil WiFi network'da ekanligingizni tekshiring

### 2. CLEARTEXT communication not permitted
**Problem**: Android 9+ da HTTP ishlamaydi

**Solution**: `AndroidManifest.xml` ga qo'shing:
```xml
<application android:usesCleartextTraffic="true">
```

### 3. TimeoutException
**Problem**: Request timeout

**Solution**:
- Backend ishga tushganligini tekshiring
- Network connectivity tekshiring
- Timeout vaqtini oshiring:
```dart
await http.get(url).timeout(Duration(seconds: 10));
```

## Production Deployment

Production uchun:
1. HTTPS qo'shing (SSL certificate)
2. Authentication qo'shing (JWT token)
3. Base URL'ni environment variable'dan oling
4. Error handling'ni yaxshilang
5. Retry logic qo'shing

```dart
// Environment config
class ApiConfig {
  static String get baseUrl {
    return const String.fromEnvironment(
      'API_URL',
      defaultValue: 'http://10.100.104.128:5084',
    );
  }
}
```

## Helpful Commands

```bash
# Backend IP'ni topish (Windows)
ipconfig

# API test qilish (curl)
curl http://10.100.104.128:5084/api/user

# Flutter run
flutter run

# Flutter logs
flutter logs
```

## Support

Muammo bo'lsa:
1. Backend logs tekshiring
2. Flutter console'ni tekshiring
3. Network'ni tekshiring (ping)
4. Firewall'ni tekshiring

Qo'shimcha yordam kerak bo'lsa, so'rang!
