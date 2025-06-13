using System.Security.Cryptography;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUnitOfWork _uow;

        public PasswordResetService(IUnitOfWork uow) => _uow = uow;

        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            // Генерируем случайный токен (например, 32 байта)
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            string token = Convert.ToBase64String(bytes);

            var prt = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1) // срок годности 1 час
            };

            // Раньше: _uow.GetRepository<PasswordResetToken>().AddAsync(prt);
            await _uow.PasswordResetTokens.AddAsync(prt);
            await _uow.CommitAsync();
            return token;
        }

        public async Task<bool> ValidatePasswordResetTokenAsync(string token, Guid userId)
        {
            // Раньше: var repo = _uow.GetRepository<PasswordResetToken>();
            var repo = _uow.PasswordResetTokens;
            var prtQuery = await repo.FindAsync(t => t.Token == token
                                              && t.UserId == userId
                                              && t.ExpiresAt > DateTime.UtcNow);
            var found = prtQuery.FirstOrDefault();
            return found != null;
        }

        public async Task<string> CreateResetTokenAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Пользователь не найден");

            var token = await GeneratePasswordResetTokenAsync(user.Id);

            var resetLink = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(token)}&userId={user.Id}";

            return resetLink;
        }
    }
}
