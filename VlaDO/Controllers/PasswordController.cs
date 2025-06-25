using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Services;
using VlaDO.Repositories;
using System.Net.Mail;
using System.Net;

namespace VlaDO.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за восстановление и сброс пароля.
    /// Обрабатывает запросы на отправку писем и изменение пароля по токену.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        /// <summary>
        /// Единица работы с репозиториями базы данных.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Сервис, управляющий логикой генерации и валидации токенов сброса пароля.
        /// </summary>
        private readonly IPasswordResetService _prService;

        /// <summary>
        /// Конфигурация приложения (доступ к SMTP и фронтенду).
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Создает экземпляр контроллера восстановления пароля.
        /// </summary>
        /// <param name="uow">Интерфейс для доступа к репозиториям.</param>
        /// <param name="prService">Сервис токенов сброса пароля.</param>
        /// <param name="config">Конфигурация приложения.</param>
        public PasswordController(IUnitOfWork uow, IPasswordResetService prService, IConfiguration config)
        {
            _uow = uow;
            _prService = prService;
            _config = config;
        }

        /// <summary>
        /// Обрабатывает запрос на восстановление пароля по email.
        /// Генерирует токен сброса и отправляет ссылку на почту пользователя.
        /// </summary>
        /// <param name="dto">DTO с адресом электронной почты пользователя.</param>
        /// <returns>HTTP 200, если письмо успешно отправлено; иначе ошибка.</returns>
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var userRepo = _uow.Users;
            var user = await userRepo.FindAsync(u => u.Email == dto.Email);
            var foundUser = user.FirstOrDefault();
            if (foundUser == null)
                return NotFound(new { error = "Пользователь с таким email не найден" });

            var token = await _prService.GeneratePasswordResetTokenAsync(foundUser.Id);
            string resetUrl = $"{_config["FrontendBaseUrl"]}/reset-password";
            resetUrl += $"?userId={foundUser.Id}&token={Uri.EscapeDataString(token)}";

            try
            {
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = int.Parse(_config["Smtp:Port"]);
                var smtpUser = _config["Smtp:User"];
                var smtpPass = _config["Smtp:Pass"];
                var fromEmail = _config["Smtp:From"];

                using var msg = new MailMessage();
                msg.From = new MailAddress(fromEmail);
                msg.To.Add(foundUser.Email);
                msg.Subject = "Сброс пароля VlaDO";
                msg.Body = $@"
                    <html>
                      <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
                        <div style='max-width: 600px; margin: auto; background-color: #fff; border-radius: 8px; padding: 20px; border: 1px solid #ddd;'>
                          <h2 style='color: #333;'>Сброс пароля для VlaDO</h2>
                          <p>Здравствуйте!</p>
                          <p>Вы запросили сброс пароля. Пожалуйста, перейдите по ссылке ниже:</p>
                          <div style='margin: 20px 0; padding: 15px; background-color: #f0f0f0; border-left: 4px solid #007bff;'>
                            <a href='{resetUrl}' style='font-size: 16px; color: #007bff; text-decoration: none;'>Нажмите здесь, чтобы сбросить пароль</a>
                          </div>
                          <p style='color: #666;'>Ссылка действительна в течение 1 часа.</p>
                          <p style='font-size: 14px; color: #999;'>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.</p>
                        </div>
                      </body>
                    </html>";
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };
                await client.SendMailAsync(msg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при отправке письма", detail = ex.Message });
            }

            return Ok(new { message = "Письмо для сброса пароля отправлено" });
        }

        /// <summary>
        /// Завершает процесс восстановления пароля, используя токен и новую пару паролей.
        /// </summary>
        /// <param name="dto">DTO с токеном, userId, новым паролем и подтверждением.</param>
        /// <returns>HTTP 200 при успешном сбросе пароля; иначе ошибка.</returns>
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { error = "Пароли не совпадают" });

            if (!await _prService.ValidatePasswordResetTokenAsync(dto.Token, dto.UserId))
                return BadRequest(new { error = "Недействительный или просроченный токен" });

            var user = await _uow.Users.GetByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { error = "Пользователь не найден" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _uow.Users.UpdateAsync(user);
            await _uow.CommitAsync();

            return Ok(new { message = "Пароль успешно сброшен" });
        }
    }
}
