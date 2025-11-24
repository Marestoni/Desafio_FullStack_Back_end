using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduGraphScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed login attempt for user {Username}: {Error}", request.Username, ex.Message);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Registration failed for user {Username}: {Error}", request.Username, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("validate")]
    public IActionResult Validate()
    {
        // Este endpoint é protegido por autenticação
        // Se chegou aqui, o token é válido
        var username = User.Identity?.Name;
        return Ok(new { message = $"Token válido para usuário: {username}" });
    }
}