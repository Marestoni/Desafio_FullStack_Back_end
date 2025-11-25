using EduGraphScheduler.Application.DTOs;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduGraphScheduler.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var user = await _userRepository.GetByMicrosoftGraphIdAsync(request.Username);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning("Login failed: User {Username} not found", request.Username);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        var token = GenerateJwtToken(user.UserPrincipalName, user.Mail);
        var expires = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes());

        return new LoginResponse
        {
            Token = token,
            Expires = expires,
            User = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Mail = user.Mail,
                JobTitle = user.JobTitle,
                Department = user.Department
            }
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

        var existingUser = await _userRepository.GetByMicrosoftGraphIdAsync(request.Username);
        if (existingUser != null)
        {
            throw new ArgumentException("Usuário já existe");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            MicrosoftGraphId = request.Username,
            DisplayName = request.DisplayName,
            Mail = request.Email,
            UserPrincipalName = request.Username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);

        _logger.LogInformation("User {Username} registered successfully", request.Username);

        var token = GenerateJwtToken(newUser.UserPrincipalName, newUser.Mail);
        var expires = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes());

        return new LoginResponse
        {
            Token = token,
            Expires = expires,
            User = new UserDto
            {
                Id = newUser.Id,
                DisplayName = newUser.DisplayName,
                Mail = newUser.Mail,
                JobTitle = newUser.JobTitle,
                Department = newUser.Department
            }
        };
    }

    public string GenerateJwtToken(string username, string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new ArgumentNullException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"] ?? "EduGraphScheduler";
        var audience = jwtSettings["Audience"] ?? "EduGraphSchedulerUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<User> CreateOrGetMockUserAsync(string username)
    {
        var user = await _userRepository.GetByMicrosoftGraphIdAsync(username);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                MicrosoftGraphId = username,
                DisplayName = "Demo User",
                UserPrincipalName = username,
                Mail = $"{username}@edu.com",
                JobTitle = "Student",
                Department = "Education",
                CreatedAt = DateTime.UtcNow,
                LastSyncedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
        }

        return user;
    }

    private int GetJwtExpiryMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return int.TryParse(jwtSettings["ExpiryInMinutes"], out int expiry) ? expiry : 60;
    }
}