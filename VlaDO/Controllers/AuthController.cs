using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Services;

namespace VlaDO.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Пароли не совпадают" });

            try
            {
                await _authService.RegisterAsync(dto);
                return Ok(new { message = "Регистрация успешна" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Вход в систему и получение JWT-токена.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var token = await _authService.LoginAsync(dto);
            if (token is null)
                return Unauthorized(new { message = "Неверные учётные данные" });

            return Ok(new { token });
        }
    }
}