using System.Security.Claims;

namespace VlaDO.Extensions
{
    /// <summary>
    /// Расширения для получения информации о пользователе из ClaimsPrincipal.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Возвращает идентификатор пользователя из клейма NameIdentifier.
        /// </summary>
        /// <param name="user">Объект пользователя (ClaimsPrincipal).</param>
        /// <returns>Guid пользователя.</returns>
        /// <exception cref="InvalidOperationException">Если идентификатор отсутствует или некорректен.</exception>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id
                : throw new InvalidOperationException("User ID claim missing or invalid");
        }
    }
}
