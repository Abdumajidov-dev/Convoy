using Convoy.Api.Data;
using Convoy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Userlarni olishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User topilmadi" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Userni olishda xatolik: Id {Id}", id);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    // POST: api/user
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createDto)
    {
        try
        {
            // Phone unique ekanligini tekshirish
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == createDto.Phone);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Bu telefon raqami allaqachon ro'yxatdan o'tgan" });
            }

            // DTO'dan User entity yaratish
            var user = new User
            {
                Name = createDto.Name,
                Phone = createDto.Phone,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Response DTO yaratish
            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User yaratishda xatolik");
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    // PUT: api/user/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] CreateUserDto updateDto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User topilmadi" });
            }

            // Phone unique tekshirish (o'zi bundan boshqa)
            var phoneExists = await _context.Users
                .AnyAsync(u => u.Phone == updateDto.Phone && u.Id != id);

            if (phoneExists)
            {
                return BadRequest(new { message = "Bu telefon raqami boshqa user tomonidan ishlatilmoqda" });
            }

            // Ma'lumotlarni yangilash
            user.Name = updateDto.Name;
            user.Phone = updateDto.Phone;
            user.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();

            // Response DTO
            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Userni yangilashda xatolik: Id {Id}", id);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }

    // DELETE: api/user/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User topilmadi" });
            }

            // Soft delete
            user.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Userni o'chirishda xatolik: Id {Id}", id);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }
}
