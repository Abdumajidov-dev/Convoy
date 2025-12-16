using Convoy.Data.Interfaces;
using Convoy.Domain.DTOs;
using Convoy.Domain.Entities;
using Convoy.Service.Interfaces;

namespace Convoy.Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllActiveUsersAsync()
    {
        var users = await _userRepository.GetAllActiveUsersAsync();
        return users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Phone = u.Phone,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto createDto)
    {
        // Phone uniqueness tekshirish
        if (await _userRepository.PhoneExistsAsync(createDto.Phone))
        {
            throw new InvalidOperationException("Bu telefon raqami allaqachon ro'yxatdan o'tgan");
        }

        var user = new User
        {
            Name = createDto.Name,
            Phone = createDto.Phone,
            IsActive = createDto.IsActive
        };

        var createdUser = await _userRepository.CreateAsync(user);

        return new UserResponseDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Phone = createdUser.Phone,
            IsActive = createdUser.IsActive,
            CreatedAt = createdUser.CreatedAt
        };
    }

    public async Task<UserResponseDto> UpdateAsync(int id, CreateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User topilmadi");
        }

        // Phone uniqueness tekshirish
        if (await _userRepository.PhoneExistsAsync(updateDto.Phone, id))
        {
            throw new InvalidOperationException("Bu telefon raqami boshqa user tomonidan ishlatilmoqda");
        }

        user.Name = updateDto.Name;
        user.Phone = updateDto.Phone;
        user.IsActive = updateDto.IsActive;

        var updatedUser = await _userRepository.UpdateAsync(user);

        return new UserResponseDto
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Phone = updatedUser.Phone,
            IsActive = updatedUser.IsActive,
            CreatedAt = updatedUser.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }
}
