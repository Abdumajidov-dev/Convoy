using Convoy.Domain.Entities;

namespace Convoy.Data.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllActiveUsersAsync();
    Task<IEnumerable<User>> GetAllAsync(); // Barcha user'lar (active va inactive)
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id); // Soft delete
    Task<bool> ExistsAsync(int id);
    Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null);
}
