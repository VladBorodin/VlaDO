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
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<ClientType> _clientTypeRepository;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepository,
            IGenericRepository<ClientType> clientTypeRepository,
            IConfiguration config)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _clientTypeRepository = clientTypeRepository ?? throw new ArgumentNullException(nameof(clientTypeRepository));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email уже зарегистрирован");

            var clientTypeExists = await _clientTypeRepository.GetByIdAsync(dto.ClientTypeId);
            if (clientTypeExists == null)
                throw new InvalidOperationException("Выбранный тип клиента не существует.");

            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                ClientTypeId = dto.ClientTypeId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(newUser);
            return true;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
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
