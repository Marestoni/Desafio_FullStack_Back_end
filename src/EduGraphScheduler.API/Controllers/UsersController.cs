using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduGraphScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithEventsDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserWithEventsAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("sync")]
    public async Task<ActionResult> SyncUsers()
    {
        await _userService.SyncUsersFromMicrosoftGraphAsync();
        return Ok(new { message = "Users synchronization started" });
    }
}