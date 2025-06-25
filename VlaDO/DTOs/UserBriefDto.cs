namespace VlaDO.DTOs
{
    /// <summary>
    /// Краткая информация о пользователе.
    /// </summary>
    /// <param name="Id">Идентификатор пользователя.</param>
    /// <param name="Name">Отображаемое имя пользователя.</param>
    public record UserBriefDto(Guid Id, string Name);
}
