using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserRepository _users;
        public AuthController(IAuthService a, IUserRepository u) { _auth = a; _users = u; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _users.GetByEmailAsync(dto.Email) != null)
                return Conflict("E-mail уже зарегистрирован");

            if (await _users.GetByNameAsync(dto.Name) != null)
                return Conflict("Имя пользователя занято");

            await _auth.RegisterAsync(dto);
            return Ok("Регистрация успешна");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _auth.LoginAsync(dto);
            return token is null ? Unauthorized("Неверные данные") : Ok(new { token });
        }
    }
}