// VlaDO/Controllers/PasswordController.cs
using Microsoft.AspNetCore.Mvc;
using VlaDO.DTOs;
using VlaDO.Services;
using VlaDO.Repositories;
using System.Net.Mail;
using System.Net;

namespace VlaDO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordResetService _prService;
        private readonly IConfiguration _config;

        public PasswordController(
            IUnitOfWork uow,
            IPasswordResetService prService,
            IConfiguration config)
        {
            _uow = uow;
            _prService = prService;
            _config = config;
        }

        // POST api/password/forgot
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

        // POST api/password/reset
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
{
    if (dto.NewPassword != dto.ConfirmPassword)
        return BadRequest(new { error = "Пароли не совпадают" });

    // Проверяем валидность токена
    if (!await _prService.ValidatePasswordResetTokenAsync(dto.Token, dto.UserId))
        return BadRequest(new { error = "Недействительный или просроченный токен" });

    // Ищем пользователя
    var user = await _uow.Users.GetByIdAsync(dto.UserId);
    if (user == null)
        return NotFound(new { error = "Пользователь не найден" });

    // Обновляем пароль — используем UpdateAsync, не Update
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
    await _uow.Users.UpdateAsync(user);
    await _uow.CommitAsync();

    return Ok(new { message = "Пароль успешно сброшен" });
}
    }
}
