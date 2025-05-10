using System.Security.Claims;

namespace VlaDO.Extensions
{
    public static class UserExtensions
    {
        /// <summary>Возвращает Guid текущего пользователя из claim NameIdentifier.</summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("User ID claim missing or invalid");
        }
    }
}
