using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VlaDO.DTOs;
using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public AuthService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        static string NormalizeEmail(string? e) => (e ?? "").Trim().ToLowerInvariant();

        public async Task RegisterAsync(RegisterDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            if (await _uow.Users.GetByEmailAsync(email) is not null)
                throw new InvalidOperationException("Имя пользователя или E-mail уже используются");

            if (await _uow.Users.GetByNameAsync(dto.Name) is not null)
                throw new InvalidOperationException("Имя пользователя или E-mail уже используются");

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _uow.Users.AddAsync(user);
            await _uow.CommitAsync();
        }


        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _uow.Users.GetByEmailAsync(NormalizeEmail(dto.Email));
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
