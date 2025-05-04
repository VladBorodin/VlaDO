namespace VlaDO.Services;

using VlaDO.DTOs;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto);
}
