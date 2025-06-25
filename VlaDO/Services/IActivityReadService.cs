namespace VlaDO.Services
{
    /// <summary>
    /// Сервис для работы с информацией о прочитанных активностях пользователя.
    /// </summary>
    public interface IActivityReadService
    {
        /// <summary>
        /// Проверяет, была ли указанная активность прочитана пользователем.
        /// </summary>
        /// <param name="activityId">Идентификатор активности.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>True, если активность отмечена как прочитанная, иначе false.</returns>
        Task<bool> IsReadAsync(Guid activityId, Guid userId);

        /// <summary>
        /// Помечает указанную активность как прочитанную пользователем.
        /// </summary>
        /// <param name="activityId">Идентификатор активности.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        Task MarkAsReadAsync(Guid activityId, Guid userId);

        /// <summary>
        /// Получает список идентификаторов активностей, прочитанных пользователем.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Коллекция идентификаторов прочитанных активностей.</returns>
        Task<IEnumerable<Guid>> GetReadActivityIdsAsync(Guid userId);
    }
}
