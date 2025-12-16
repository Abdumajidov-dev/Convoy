using Convoy.Data.Context;
using Convoy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Convoy.Data.Seeding;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        // Agar user mavjud bo'lsa, seed qilmaslik
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // Test userlar yaratish
        var users = new[]
        {
            new User
            {
                Name = "Haydovchi 1",
                Phone = "+998901234567",
                IsActive = true
            },
            new User
            {
                Name = "Haydovchi 2",
                Phone = "+998901234568",
                IsActive = true
            },
            new User
            {
                Name = "Haydovchi 3",
                Phone = "+998901234569",
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        Console.WriteLine("Test ma'lumotlar qo'shildi!");
    }
}
