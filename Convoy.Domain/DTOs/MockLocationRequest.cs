namespace Convoy.Domain.DTOs;

public class MockLocationRequest
{
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class MockLocationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PointsGenerated { get; set; }
    public int DaysGenerated { get; set; }
    public int EstimatedVisits { get; set; }
}
