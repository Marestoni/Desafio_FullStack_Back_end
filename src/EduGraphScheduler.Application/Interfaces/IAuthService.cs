using EduGraphScheduler.Application.DTOs;

namespace EduGraphScheduler.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    string GenerateJwtToken(string username, string email);
}