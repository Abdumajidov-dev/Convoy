using Convoy.Domain.DTOs;

namespace Convoy.Service.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllActiveUsersAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(CreateUserDto createDto);
    Task<UserResponseDto> UpdateAsync(int id, CreateUserDto updateDto);
    Task<bool> DeleteAsync(int id);
}
