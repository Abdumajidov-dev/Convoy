using Convoy.Domain.DTOs;

namespace Convoy.Service.Interfaces;

public interface IMockDataService
{
    Task<MockLocationResponse> GenerateMockLocationsAsync(MockLocationRequest request);
    Task<int> ClearUserLocationsAsync(int userId);
}
