using VlaDO.DTOs;

namespace VlaDO.Services
{
    public interface IPasswordResetService
    {
        Task<string> GeneratePasswordResetTokenAsync(Guid userId);
        Task<bool> ValidatePasswordResetTokenAsync(string token, Guid userId);
        Task<string> CreateResetTokenAsync(ForgotPasswordRequestDto dto);
    }
}
