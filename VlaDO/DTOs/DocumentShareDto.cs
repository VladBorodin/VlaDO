using VlaDO.Models;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для представления информации о правах доступа пользователя к документу по токену.
    /// </summary>
    /// <param name="TokenId">Идентификатор токена, сгенерированного для доступа к документу.</param>
    /// <param name="UserId">Идентификатор пользователя, которому предоставлен доступ.</param>
    /// <param name="UserName">Имя пользователя, которому предоставлен доступ.</param>
    /// <param name="AccessLevel">Уровень доступа, предоставленный пользователю.</param>
    public record DocumentShareDto(
        Guid TokenId,
        Guid UserId,
        string UserName,
        AccessLevel AccessLevel
    );
}