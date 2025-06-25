using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Services;

/// <summary>
/// Сервис для управления доступом к документам по токену или явному разрешению.
/// </summary>
public interface IShareService
{
    /// <summary>
    /// Делится документом с использованием временного токена.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="level">Уровень доступа.</param>
    /// <param name="ttl">Время жизни токена.</param>
    /// <returns>Строковое значение токена доступа.</returns>
    Task<string> ShareDocumentAsync(Guid docId, AccessLevel level, TimeSpan ttl);

    /// <summary>
    /// Отзывает ранее выданный токен.
    /// </summary>
    /// <param name="token">Строковое значение токена для отзыва.</param>
    Task RevokeTokenAsync(string token);

    /// <summary>
    /// Загружает документ по токену доступа.
    /// </summary>
    /// <param name="token">Токен доступа.</param>
    /// <returns>Кортеж: байты файла, имя, MIME-тип и идентификатор комнаты.</returns>
    Task<(byte[] bytes, string name, string ctype, Guid roomId)> DownloadByTokenAsync(string token);

    /// <summary>
    /// Получает список прямых расшариваний документа с пользователями.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Массив объектов с информацией о расшаривании.</returns>
    Task<DocumentShareDto[]> GetSharesAsync(Guid docId);

    /// <summary>
    /// Создаёт или обновляет запись о расшаривании документа конкретному пользователю.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <param name="userId">Идентификатор пользователя, с которым делятся.</param>
    /// <param name="level">Уровень доступа.</param>
    /// <returns>Объект, описывающий расшаривание.</returns>
    Task<DocumentShareDto> UpsertShareAsync(Guid docId, Guid userId, AccessLevel level);

    /// <summary>
    /// Отзывает прямой доступ пользователя к документу.
    /// </summary>
    /// <param name="tokenId">Идентификатор расшаривания (внутренний ID).</param>
    Task RevokeShareAsync(Guid tokenId);
}
