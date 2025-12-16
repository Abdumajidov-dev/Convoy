# Flutter SignalR Integration Guide - Convoy API

## Overview

Convoy API endi **SignalR WebSocket** orqali real-time location tracking qo'llab-quvvatlaydi. Flutter client SignalR hub ga ulanadi va location update'larni real-time yuboradi.

## SignalR Hub Endpoint

```
ws://YOUR_SERVER_IP:5084/hubs/location
```

Production:
```
wss://YOUR_DOMAIN/hubs/location
```

## Flutter Setup

### 1. Add Dependencies

`pubspec.yaml` ga qo'shing:

```yaml
dependencies:
  signalr_netcore: ^1.3.7  # SignalR client
  geolocator: ^11.0.0      # GPS location
  permission_handler: ^11.0.0  # Location permissions
```

Install:
```bash
flutter pub get
```

### 2. SignalR Service

`lib/services/signalr_service.dart` yarating:

```dart
import 'package:signalr_netcore/signalr_client.dart';

class SignalRService {
  HubConnection? _hubConnection;
  bool _isConnected = false;

  // SignalR Hub URL
  static const String hubUrl = 'http://YOUR_SERVER_IP:5084/hubs/location';

  bool get isConnected => _isConnected;

  // Connect to SignalR Hub
  Future<void> connect() async {
    try {
      _hubConnection = HubConnectionBuilder()
          .withUrl(hubUrl)
          .withAutomaticReconnect()
          .build();

      // Event handlers
      _hubConnection!.on('LocationUpdated', _onLocationUpdated);
      _hubConnection!.on('LocationsReceived', _onLocationsReceived);
      _hubConnection!.on('LocationError', _onLocationError);

      // Connection state handlers
      _hubConnection!.onclose(({error}) {
        print('SignalR Disconnected: $error');
        _isConnected = false;
      });

      _hubConnection!.onreconnecting(({error}) {
        print('SignalR Reconnecting: $error');
        _isConnected = false;
      });

      _hubConnection!.onreconnected(({connectionId}) {
        print('SignalR Reconnected: $connectionId');
        _isConnected = true;
      });

      // Start connection
      await _hubConnection!.start();
      _isConnected = true;
      print('SignalR Connected successfully!');
    } catch (e) {
      print('SignalR Connection error: $e');
      _isConnected = false;
      rethrow;
    }
  }

  // Disconnect from SignalR Hub
  Future<void> disconnect() async {
    if (_hubConnection != null) {
      await _hubConnection!.stop();
      _hubConnection = null;
      _isConnected = false;
      print('SignalR Disconnected');
    }
  }

  // Send location updates (new format: userId va locations array)
  Future<void> sendLocations({
    required int userId,
    required List<Map<String, String>> locations,
  }) async {
    if (!_isConnected || _hubConnection == null) {
      throw Exception('SignalR not connected');
    }

    try {
      final locationData = {
        'userId': userId,
        'locations': locations,
      };

      await _hubConnection!.invoke('SendLocations', args: [locationData]);
      print('Locations sent: $locationData');
    } catch (e) {
      print('Error sending locations: $e');
      rethrow;
    }
  }

  // Helper method: bitta location yuborish uchun
  Future<void> sendSingleLocation({
    required int userId,
    required double latitude,
    required double longitude,
    String? timestamp,
    double? speed,
    double? accuracy,
  }) async {
    final locations = [
      {
        'latitude': latitude.toString(),
        'longitude': longitude.toString(),
        'timestamp': timestamp ?? DateTime.now().toUtc().toIso8601String(),
        'speed': speed?.toString() ?? '',
        'accuracy': accuracy?.toString() ?? '',
      }
    ];

    await sendLocations(userId: userId, locations: locations);
  }


  // Subscribe to specific user's updates
  Future<void> subscribeToUser(int userId) async {
    if (!_isConnected || _hubConnection == null) {
      throw Exception('SignalR not connected');
    }

    await _hubConnection!.invoke('SubscribeToUser', args: [userId]);
    print('Subscribed to User $userId');
  }

  // Event Handlers
  void _onLocationUpdated(List<Object>? arguments) {
    if (arguments != null && arguments.isNotEmpty) {
      print('Location Updated: ${arguments[0]}');
      // Handle real-time location update from any user
      // You can update UI, show on map, etc.
    }
  }

  void _onLocationReceived(List<Object>? arguments) {
    // Deprecated - use _onLocationsReceived
  }

  void _onLocationsReceived(List<Object>? arguments) {
    if (arguments != null && arguments.isNotEmpty) {
      print('Locations Received: ${arguments[0]}');
      // result format: {userId, savedCount, totalReceived, errors?}
    }
  }

  void _onLocationError(List<Object>? arguments) {
    if (arguments != null && arguments.isnotEmpty) {
      print('Location Error: ${arguments[0]}');
    }
  }
}
```

### 3. Location Tracking Service

`lib/services/location_service.dart`:

```dart
import 'package:geolocator/geolocator.dart';
import 'package:permission_handler/permission_handler.dart';
import 'signalr_service.dart';

class LocationService {
  final SignalRService _signalRService;
  final int userId;

  StreamSubscription<Position>? _positionStreamSubscription;
  bool _isTracking = false;

  LocationService({
    required this.userId,
    required SignalRService signalRService,
  }) : _signalRService = signalRService;

  // Request location permissions
  Future<bool> requestPermissions() async {
    final status = await Permission.location.request();
    return status.isGranted;
  }

  // Start real-time location tracking
  Future<void> startTracking({
    int intervalSeconds = 5, // Send location every 5 seconds
  }) async {
    if (_isTracking) {
      print('Already tracking');
      return;
    }

    // Check permissions
    final hasPermission = await requestPermissions();
    if (!hasPermission) {
      throw Exception('Location permission denied');
    }

    // Check if SignalR is connected
    if (!_signalRService.isConnected) {
      await _signalRService.connect();
    }

    // Location settings
    const LocationSettings locationSettings = LocationSettings(
      accuracy: LocationAccuracy.high,
      distanceFilter: 10, // meters
    );

    // Start listening to position stream
    _positionStreamSubscription = Geolocator.getPositionStream(
      locationSettings: locationSettings,
    ).listen(
      (Position position) async {
        try {
          // Send location via SignalR (new format)
          await _signalRService.sendSingleLocation(
            userId: userId,
            latitude: position.latitude,
            longitude: position.longitude,
            timestamp: position.timestamp?.toUtc().toIso8601String(),
            speed: position.speed,
            accuracy: position.accuracy,
          );

          print('Location sent: ${position.latitude}, ${position.longitude}');
        } catch (e) {
          print('Error sending location: $e');
        }
      },
      onError: (error) {
        print('Position stream error: $error');
      },
    );

    _isTracking = true;
    print('Location tracking started');
  }

  // Stop location tracking
  Future<void> stopTracking() async {
    if (!_isTracking) return;

    await _positionStreamSubscription?.cancel();
    _positionStreamSubscription = null;
    _isTracking = false;
    print('Location tracking stopped');
  }

  // Send single location (manual)
  Future<void> sendCurrentLocation() async {
    try {
      final position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );

      await _signalRService.sendSingleLocation(
        userId: userId,
        latitude: position.latitude,
        longitude: position.longitude,
        timestamp: position.timestamp?.toUtc().toIso8601String(),
        speed: position.speed,
        accuracy: position.accuracy,
      );

      print('Current location sent');
    } catch (e) {
      print('Error getting/sending current location: $e');
      rethrow;
    }
  }

  bool get isTracking => _isTracking;
}
```

### 4. Usage Example

`lib/screens/tracking_screen.dart`:

```dart
import 'package:flutter/material.dart';
import '../services/signalr_service.dart';
import '../services/location_service.dart';

class TrackingScreen extends StatefulWidget {
  final int userId;

  const TrackingScreen({Key? key, required this.userId}) : super(key: key);

  @override
  State<TrackingScreen> createState() => _TrackingScreenState();
}

class _TrackingScreenState extends State<TrackingScreen> {
  late SignalRService _signalRService;
  late LocationService _locationService;
  bool _isConnected = false;
  bool _isTracking = false;

  @override
  void initState() {
    super.initState();
    _signalRService = SignalRService();
    _locationService = LocationService(
      userId: widget.userId,
      signalRService: _signalRService,
    );
  }

  Future<void> _connect() async {
    try {
      await _signalRService.connect();
      setState(() => _isConnected = true);

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Connected to server')),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Connection failed: $e')),
      );
    }
  }

  Future<void> _disconnect() async {
    await _locationService.stopTracking();
    await _signalRService.disconnect();
    setState(() {
      _isConnected = false;
      _isTracking = false;
    });
  }

  Future<void> _startTracking() async {
    try {
      await _locationService.startTracking(intervalSeconds: 5);
      setState(() => _isTracking = true);

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Location tracking started')),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to start tracking: $e')),
      );
    }
  }

  Future<void> _stopTracking() async {
    await _locationService.stopTracking();
    setState(() => _isTracking = false);
  }

  @override
  void dispose() {
    _disconnect();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Driver Tracking - User ${widget.userId}'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              _isConnected ? 'Connected ✓' : 'Disconnected ✗',
              style: TextStyle(
                fontSize: 24,
                color: _isConnected ? Colors.green : Colors.red,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 20),
            Text(
              _isTracking ? 'Tracking Active 📍' : 'Tracking Stopped ⏸',
              style: TextStyle(
                fontSize: 20,
                color: _isTracking ? Colors.blue : Colors.grey,
              ),
            ),
            const SizedBox(height: 40),
            if (!_isConnected)
              ElevatedButton(
                onPressed: _connect,
                child: const Text('Connect to Server'),
              ),
            if (_isConnected && !_isTracking)
              ElevatedButton(
                onPressed: _startTracking,
                style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
                child: const Text('Start Tracking'),
              ),
            if (_isTracking)
              ElevatedButton(
                onPressed: _stopTracking,
                style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                child: const Text('Stop Tracking'),
              ),
            if (_isConnected)
              TextButton(
                onPressed: _disconnect,
                child: const Text('Disconnect'),
              ),
          ],
        ),
      ),
    );
  }
}
```

## SignalR Methods Available

### Client -> Server (Flutter calls these)

#### 1. SendLocations (NEW FORMAT)
**Yangi structure**: Bitta userId va uning locationlari array bo'lib keladi

```dart
// Multiple locations for one user
await signalRService.sendLocations(
  userId: 1,
  locations: [
    {
      'latitude': '41.2995',
      'longitude': '69.2401',
      'timestamp': '2024-12-03T10:00:00Z',
      'speed': '45.5',
      'accuracy': '10.0',
    },
    {
      'latitude': '41.3005',
      'longitude': '69.2411',
      'timestamp': '2024-12-03T10:00:05Z',
      'speed': '50.0',
      'accuracy': '8.0',
    },
  ],
);

// Single location (helper method)
await signalRService.sendSingleLocation(
  userId: 1,
  latitude: 41.2995,
  longitude: 69.2401,
  timestamp: DateTime.now().toUtc().toIso8601String(),
  speed: 45.5,
  accuracy: 10.0,
);
```

#### 2. SubscribeToUser
Muayyan user'ning location update'larini tingla:
```dart
await signalRService.subscribeToUser(userId: 1);
```

### Server -> Client (Flutter listens to these)

#### 1. LocationUpdated
Barcha clientlarga broadcast qilinadi (real-time update):
```dart
_hubConnection.on('LocationUpdated', (data) {
  print('User ${data[0]['userId']} location updated');
  // Update map, UI, etc.
});
```

#### 2. LocationsReceived (NEW)
Confirmation message with result (faqat sender'ga):
```dart
_hubConnection.on('LocationsReceived', (result) {
  print('User: ${result[0]['userId']}');
  print('Saved: ${result[0]['savedCount']}/${result[0]['totalReceived']}');
  if (result[0]['errors'] != null) {
    print('Errors: ${result[0]['errors']}');
  }
});
```

#### 3. LocationError
Error message:
```dart
_hubConnection.on('LocationError', (error) {
  print('Error: $error');
});
```

## Android Permissions

`android/app/src/main/AndroidManifest.xml`:

```xml
<manifest>
    <uses-permission android:name="android.permission.INTERNET"/>
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />

    <application>
        ...
    </application>
</manifest>
```

## iOS Permissions

`ios/Runner/Info.plist`:

```xml
<dict>
    <key>NSLocationWhenInUseUsageDescription</key>
    <string>We need your location to track delivery</string>
    <key>NSLocationAlwaysUsageDescription</key>
    <string>We need your location for background tracking</string>
    <key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
    <string>We need your location to track your route</string>
</dict>
```

## Testing

1. **Start Convoy API**:
   ```bash
   cd Convoy.Api
   dotnet run --launch-profile http
   ```

2. **Open test client in browser**:
   ```
   http://localhost:5084/signalr-test.html
   ```

3. **Run Flutter app**:
   ```bash
   flutter run
   ```

4. Test client browser'da har 5 sekundda location yuborilishini ko'rasiz va Flutter app ham real-time update'larni qabul qiladi.

## Important Notes

1. **UTC Timestamps**: Har doim UTC format ishlatiladi
2. **String Coordinates**: Latitude va Longitude string formatda yuboriladi
3. **Automatic Reconnect**: SignalR avtomatik qayta ulanadi disconnect bo'lganda
4. **Real-time Broadcast**: Barcha ulangan clientlar location update'larni oladi
5. **Background Location**: Android 10+ uchun background location permission kerak

## Production Deployment

1. HTTPS ishlatiladi:
   ```dart
   static const String hubUrl = 'wss://your-domain.com/hubs/location';
   ```

2. CORS configuration to'g'ri bo'lishi kerak (Program.cs):
   ```csharp
   policy.WithOrigins("https://your-flutter-domain.com")
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials(); // SignalR uchun kerak
   ```

3. SSL certificate to'g'ri configure qiling

## Troubleshooting

**Problem**: SignalR connection fails
- ✅ Server ishlab turishini tekshiring
- ✅ CORS settings to'g'ri ekanini tekshiring
- ✅ Firewall portni ochgan bo'lishini tekshiring

**Problem**: Location permission denied
- ✅ AndroidManifest.xml va Info.plist ni tekshiring
- ✅ Runtime permission request qiling

**Problem**: Background location not working
- ✅ Android: Background location permission kerak
- ✅ iOS: Always permission va background mode yoqilgan bo'lishi kerak
