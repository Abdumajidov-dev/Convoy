# WPF Admin Panel - SignalR Integration Guide

## Overview

WPF Admin Panel barcha haydovchilarning real-time location'larini SignalR orqali kuzatadi.

## Architecture

```
Haydovchi 1 (Flutter) → SignalR → Server
Haydovchi 2 (Flutter) → SignalR → Server
Haydovchi 3 (Flutter) → SignalR → Server
                                    ↓
                    Faqat ADMIN group'iga broadcast
                                    ↓
WPF Admin Panel ← LocationUpdated ← Server
```

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.0" />
```

## SignalR Service (C#)

### 1. SignalR Service Class

`Services/SignalRService.cs`:

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ConvoyAdminPanel.Services
{
    public class SignalRService
    {
        private HubConnection? _hubConnection;
        private bool _isConnected = false;

        public event EventHandler<LocationUpdateEventArgs>? LocationUpdated;
        public event EventHandler<string>? ConnectionStateChanged;

        public bool IsConnected => _isConnected;

        // SignalR Hub URL
        private const string HubUrl = "http://localhost:5084/hubs/location";

        public async Task ConnectAsync()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(HubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                // Event handlers
                _hubConnection.On<LocationUpdateData>("LocationUpdated", OnLocationUpdated);

                // Connection state handlers
                _hubConnection.Closed += OnConnectionClosed;
                _hubConnection.Reconnecting += OnReconnecting;
                _hubConnection.Reconnected += OnReconnected;

                // Start connection
                await _hubConnection.StartAsync();
                _isConnected = true;
                ConnectionStateChanged?.Invoke(this, "Connected");

                // IMPORTANT: Join Admin group to receive location updates
                await _hubConnection.InvokeAsync("JoinAdminGroup");
                Console.WriteLine("Joined Admin group - receiving all location updates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection error: {ex.Message}");
                _isConnected = false;
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, "Disconnected");
            }
        }

        private void OnLocationUpdated(LocationUpdateData data)
        {
            LocationUpdated?.Invoke(this, new LocationUpdateEventArgs(data));
        }

        private Task OnConnectionClosed(Exception? error)
        {
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, $"Disconnected: {error?.Message}");
            return Task.CompletedTask;
        }

        private Task OnReconnecting(Exception? error)
        {
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, "Reconnecting...");
            return Task.CompletedTask;
        }

        private Task OnReconnected(string? connectionId)
        {
            _isConnected = true;
            ConnectionStateChanged?.Invoke(this, "Reconnected");

            // Rejoin Admin group after reconnection
            _ = _hubConnection?.InvokeAsync("JoinAdminGroup");

            return Task.CompletedTask;
        }
    }

    // Location update data model
    public class LocationUpdateData
    {
        public int UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public double? Speed { get; set; }
        public double? Accuracy { get; set; }
    }

    // Event args
    public class LocationUpdateEventArgs : EventArgs
    {
        public LocationUpdateData Data { get; }

        public LocationUpdateEventArgs(LocationUpdateData data)
        {
            Data = data;
        }
    }
}
```

### 2. MainWindow.xaml.cs Integration

```csharp
using ConvoyAdminPanel.Services;
using System.Windows;

namespace ConvoyAdminPanel
{
    public partial class MainWindow : Window
    {
        private readonly SignalRService _signalRService;
        private Dictionary<int, DriverMarker> _driverMarkers = new();

        public MainWindow()
        {
            InitializeComponent();

            _signalRService = new SignalRService();
            _signalRService.LocationUpdated += OnLocationUpdated;
            _signalRService.ConnectionStateChanged += OnConnectionStateChanged;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _signalRService.ConnectAsync();
                StatusText.Text = "Connected to SignalR";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Connection failed: {ex.Message}";
            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await _signalRService.DisconnectAsync();
        }

        private void OnLocationUpdated(object? sender, LocationUpdateEventArgs e)
        {
            // UI thread'da ishlatish uchun
            Dispatcher.Invoke(() =>
            {
                var data = e.Data;

                // Map'da marker'ni yangilash
                UpdateDriverMarker(data.UserId, data.Latitude, data.Longitude);

                // Log qilish
                LogTextBox.AppendText(
                    $"[{DateTime.Now:HH:mm:ss}] Driver {data.UserId}: " +
                    $"{data.Latitude:F6}, {data.Longitude:F6}" +
                    (data.Speed.HasValue ? $" | Speed: {data.Speed:F1} km/h" : "") +
                    "\n"
                );

                LogTextBox.ScrollToEnd();
            });
        }

        private void OnConnectionStateChanged(object? sender, string state)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"SignalR: {state}";
            });
        }

        private void UpdateDriverMarker(int userId, double lat, double lng)
        {
            // Map implementation (Google Maps, Bing Maps, yoki boshqa)
            // Bu sizning map control'ingizga bog'liq

            if (_driverMarkers.TryGetValue(userId, out var marker))
            {
                // Update existing marker
                marker.UpdatePosition(lat, lng);
            }
            else
            {
                // Create new marker
                var newMarker = new DriverMarker(userId, lat, lng);
                _driverMarkers[userId] = newMarker;
                // Add to map...
            }
        }
    }
}
```

### 3. XAML (MainWindow.xaml)

```xml
<Window x:Class="ConvoyAdminPanel.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Convoy Admin Panel" Height="600" Width="1000">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="200"/>
        </Grid.RowDefinitions>

        <!-- Status Bar -->
        <Border Grid.Row="0" Background="#2C3E50" Padding="10">
            <TextBlock x:Name="StatusText"
                       Text="Connecting..."
                       Foreground="White"
                       FontSize="14"/>
        </Border>

        <!-- Map Area -->
        <Border Grid.Row="1" Background="#ECF0F1" Margin="5">
            <TextBlock Text="MAP AREA - Integrate Google Maps or Bing Maps here"
                       HorizontalAlignment="Center"
                       VerticalAlignment="Center"
                       FontSize="20"
                       Foreground="#95A5A6"/>
            <!-- Add your map control here -->
        </Border>

        <!-- Log Area -->
        <Border Grid.Row="2" Background="White" Margin="5" BorderBrush="#BDC3C7" BorderThickness="1">
            <ScrollViewer>
                <TextBox x:Name="LogTextBox"
                         IsReadOnly="True"
                         TextWrapping="Wrap"
                         FontFamily="Consolas"
                         FontSize="12"
                         BorderThickness="0"
                         VerticalScrollBarVisibility="Auto"/>
            </ScrollViewer>
        </Border>
    </Grid>
</Window>
```

## App.xaml.cs (Dependency Injection)

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ConvoyAdminPanel
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Services
            services.AddSingleton<SignalRService>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
```

## Important Notes

### 1. Admin Group

**WPF Admin ulanishda ALBATTA** `JoinAdminGroup()` chaqirishi kerak:

```csharp
await _hubConnection.InvokeAsync("JoinAdminGroup");
```

Bu qilinmasa, `LocationUpdated` event'lari kelmaydi!

### 2. Reconnection

Auto-reconnect bo'lganda ham qayta join qilish kerak:

```csharp
private Task OnReconnected(string? connectionId)
{
    // Rejoin Admin group
    _ = _hubConnection?.InvokeAsync("JoinAdminGroup");
    return Task.CompletedTask;
}
```

### 3. Thread Safety

SignalR event'lar background thread'da keladi, UI yangilash uchun `Dispatcher.Invoke()` ishlatish:

```csharp
Dispatcher.Invoke(() =>
{
    // UI yangilash
    LogTextBox.AppendText("...");
});
```

### 4. Connection URL

Development:
```csharp
private const string HubUrl = "http://localhost:5084/hubs/location";
```

Production:
```csharp
private const string HubUrl = "https://your-domain.com/hubs/location";
```

## Testing

1. **Server'ni ishga tushiring**:
   ```bash
   cd Convoy.Api
   dotnet run --launch-profile http
   ```

2. **WPF Admin'ni ishga tushiring**

3. **Flutter haydovchi app'ni ishga tushiring** va location yuboring

4. **WPF Admin'da real-time ko'rasiz**:
   - Map'da marker yangilanadi
   - Log'da location ma'lumotlari ko'rinadi

## SignalR Flow

```
1. WPF Admin ishga tushdi
   ↓
2. SignalR'ga ulandi
   ↓
3. JoinAdminGroup() chaqirdi
   ↓
4. "Admin" group'iga qo'shildi
   ↓
5. Haydovchi location yubordi
   ↓
6. Server "Admin" group'iga broadcast qildi
   ↓
7. WPF Admin LocationUpdated event'ini oldi
   ↓
8. Map yangilandi, log ko'rsatildi
```

## Haydovchilar (Flutter)

Flutter haydovchilar **hech narsa qilmaydi**, faqat location yuboradi:

```dart
// Haydovchi faqat location yuboradi
await connection.invoke('SendLocations', data);

// LocationUpdated'ni eshitmaydi!
// Chunki u "Admin" group'ida emas
```

## Summary

- ✅ WPF Admin: `JoinAdminGroup()` → `LocationUpdated` oladi
- ✅ Flutter haydovchilar: Location yuboradi, boshqa hech narsa
- ✅ Real-time: Map darhol yangilanadi
- ✅ Auto-reconnect: Qayta join qiladi

To'liq Admin panel ready! 🚀
