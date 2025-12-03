using Convoy.Domain.DTOs;
using Convoy.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Convoy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: api/user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllActiveUsersAsync();
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
            var user = await _userService.GetByIdAsync(id);

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
            var user = await _userService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            var user = await _userService.UpdateAsync(id, updateDto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("topilmadi"))
            {
                return NotFound(new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
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
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "User topilmadi" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Userni o'chirishda xatolik: Id {Id}", id);
            return StatusCode(500, new { message = "Server xatosi" });
        }
    }
}
