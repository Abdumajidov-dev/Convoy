namespace Convoy.Domain.DTOs;

/// <summary>
/// Kunlik summary ma'lumotlari
/// </summary>
public class DailySummaryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalLocations { get; set; }
    public DateTime? FirstLocationTime { get; set; }
    public DateTime? LastLocationTime { get; set; }
    public double TotalDistanceKm { get; set; }
    public double? AverageSpeed { get; set; }
    public double? MaxSpeed { get; set; }

    // Geografik chegara
    public double? MinLatitude { get; set; }
    public double? MaxLatitude { get; set; }
    public double? MinLongitude { get; set; }
    public double? MaxLongitude { get; set; }

    // Soatlik ma'lumotlar
    public List<HourlySummaryDto> HourlySummaries { get; set; } = new();
}

/// <summary>
/// Soatlik summary ma'lumotlari
/// </summary>
public class HourlySummaryDto
{
    public int Id { get; set; }
    public int Hour { get; set; }
    public int LocationCount { get; set; }
    public double DistanceKm { get; set; }
    public double? AverageSpeed { get; set; }
    public DateTime? FirstLocationTime { get; set; }
    public DateTime? LastLocationTime { get; set; }

    // Geografik chegara
    public double? MinLatitude { get; set; }
    public double? MaxLatitude { get; set; }
    public double? MinLongitude { get; set; }
    public double? MaxLongitude { get; set; }
}

/// <summary>
/// Daily summary filter (query parameters)
/// </summary>
public class DailySummaryFilterDto
{
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeHourlySummaries { get; set; } = true;
}

/// <summary>
/// Soatlik filter (haritada ko'rsatish uchun)
/// </summary>
public class HourlyFilterDto
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>
    /// Qaysi soatlarni ko'rsatish (masalan: [9, 10, 11] = 09:00-11:59)
    /// </summary>
    public List<int>? Hours { get; set; }
}
