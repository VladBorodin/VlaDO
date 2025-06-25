using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Repositories;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    /// <summary>
    /// Контроллер для регистрации и аутентификации пользователей.
    /// </summary>
    [ApiController, Route("api/auth")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Сервис авторизации и генерации токенов.
        /// </summary>
        private readonly IAuthService _auth;

        /// <summary>
        /// Репозиторий пользователей.
        /// </summary>
        private readonly IUserRepository _users;

        /// <summary>
        /// Конструктор контроллера аутентификации.
        /// </summary>
        /// <param name="a">Сервис авторизации.</param>
        /// <param name="u">Репозиторий пользователей.</param>
        public AuthController(IAuthService a, IUserRepository u) { _auth = a; _users = u; }

        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="dto">Данные для регистрации.</param>
        /// <returns>Код 200 при успехе или 409 при конфликте.</returns>
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

        /// <summary>
        /// Выполняет вход пользователя.
        /// </summary>
        /// <param name="dto">Данные для входа.</param>
        /// <returns>JWT-токен при успехе или 401 при ошибке.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _auth.LoginAsync(dto);
            return token is null ? Unauthorized("Неверные данные") : Ok(new { token });
        }
    }
}