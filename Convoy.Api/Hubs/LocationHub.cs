using Convoy.Domain.DTOs;
using Convoy.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;

namespace Convoy.Api.Hubs;

public class LocationHub : Hub
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationHub> _logger;

    public LocationHub(ILocationService locationService, ILogger<LocationHub> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    // Client connects
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Admin (WPF) calls this to join Admin group and receive all location updates
    /// </summary>
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        _logger.LogInformation($"Client {Context.ConnectionId} joined Admin group");
        await Clients.Caller.SendAsync("AdminGroupJoined", "You are now receiving all location updates");
    }

    /// <summary>
    /// Leave Admin group
    /// </summary>
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
        _logger.LogInformation($"Client {Context.ConnectionId} left Admin group");
        await Clients.Caller.SendAsync("AdminGroupLeft", "You are no longer receiving location updates");
    }

    // Client disconnects
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Flutter client calls this method to send location updates
    /// Structure: { userId: 1, locations: [{lat, lng, timestamp, speed, accuracy}, ...] }
    /// </summary>
    public async Task SendLocations(LocationDto locationDto)
    {
        try
        {
            _logger.LogInformation($"Received {locationDto.Locations.Count} location(s) for UserId {locationDto.UserId} from ConnectionId: {Context.ConnectionId}");

            if (locationDto.Locations == null || locationDto.Locations.Count == 0)
            {
                await Clients.Caller.SendAsync("LocationError", "No locations provided");
                return;
            }

            int savedCount = 0;
            List<string> errors = new();

            foreach (var point in locationDto.Locations)
            {
                try
                {
                    // Parse coordinates
                    if (!double.TryParse(point.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) ||
                        !double.TryParse(point.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var lng))
                    {
                        errors.Add($"Invalid coordinates: Lat={point.Latitude}, Lng={point.Longitude}");
                        continue;
                    }

                    // Parse timestamp
                    if (!DateTime.TryParse(point.Timestamp, out var timestamp))
                    {
                        timestamp = DateTime.UtcNow;
                    }
                    else
                    {
                        timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                    }

                    // Parse optional fields
                    double? speed = null;
                    if (!string.IsNullOrEmpty(point.Speed))
                    {
                        if (double.TryParse(point.Speed, NumberStyles.Any, CultureInfo.InvariantCulture, out var speedValue))
                            speed = speedValue;
                    }

                    double? accuracy = null;
                    if (!string.IsNullOrEmpty(point.Accuracy))
                    {
                        if (double.TryParse(point.Accuracy, NumberStyles.Any, CultureInfo.InvariantCulture, out var accuracyValue))
                            accuracy = accuracyValue;
                    }

                    // Save to database
                    var result = await _locationService.AddLocationAsync(new Domain.Entities.Location
                    {
                        UserId = locationDto.UserId,
                        Latitude = lat,
                        Longitude = lng,
                        Timestamp = timestamp,
                        Speed = speed,
                        Accuracy = accuracy
                    });

                    if (result != null)
                    {
                        savedCount++;

                        // Broadcast FAQAT Admin group'iga (WPF desktop)
                        await Clients.Group("Admin").SendAsync("LocationUpdated", new
                        {
                            userId = result.UserId,
                            latitude = result.Latitude,
                            longitude = result.Longitude,
                            timestamp = result.Timestamp,
                            speed = result.Speed,
                            accuracy = result.Accuracy
                        });

                        _logger.LogInformation($"Location saved and broadcasted to Admin group: UserId={result.UserId}, Lat={result.Latitude}, Lng={result.Longitude}");
                    }
                    else
                    {
                        errors.Add($"Failed to save location at {timestamp}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error: {ex.Message}");
                    _logger.LogError(ex, "Error processing location point");
                }
            }

            // Send result to sender
            await Clients.Caller.SendAsync("LocationsReceived", new
            {
                userId = locationDto.UserId,
                savedCount,
                totalReceived = locationDto.Locations.Count,
                errors = errors.Count > 0 ? errors : null
            });

            _logger.LogInformation($"Processed {savedCount}/{locationDto.Locations.Count} locations for UserId {locationDto.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing locations");
            await Clients.Caller.SendAsync("LocationError", ex.Message);
        }
    }

    /// <summary>
    /// Subscribe to specific user's location updates
    /// </summary>
    public async Task SubscribeToUser(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        _logger.LogInformation($"Client {Context.ConnectionId} subscribed to User_{userId}");
        await Clients.Caller.SendAsync("SubscriptionConfirmed", $"Subscribed to User {userId}");
    }

    /// <summary>
    /// Unsubscribe from specific user's location updates
    /// </summary>
    public async Task UnsubscribeFromUser(int userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from User_{userId}");
        await Clients.Caller.SendAsync("UnsubscriptionConfirmed", $"Unsubscribed from User {userId}");
    }

    /// <summary>
    /// Get all active users
    /// </summary>
    public async Task GetActiveUsers()
    {
        // This could be enhanced to track online/offline status
        await Clients.Caller.SendAsync("ActiveUsersReceived", new
        {
            message = "Use REST API /api/user endpoint to get users"
        });
    }
}
