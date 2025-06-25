using VlaDO.DTOs;
using VlaDO.Models;

namespace VlaDO.Services
{
    /// <summary>
    /// Сервис для управления документами: загрузка, обновление, скачивание и удаление.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Загружает новый документ в указанную комнату.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты, в которую загружается документ.</param>
        /// <param name="userId">Идентификатор пользователя, загружающего документ.</param>
        /// <param name="name">Имя файла.</param>
        /// <param name="data">Двоичные данные файла.</param>
        /// <returns>Идентификатор загруженного документа.</returns>
        Task<Guid> UploadAsync(Guid roomId, Guid userId, string name, byte[] data);

        /// <summary>
        /// Загружает несколько файлов в комнату.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="files">Список файлов.</param>
        Task UploadManyAsync(Guid roomId, Guid userId, IEnumerable<IFormFile> files);

        /// <summary>
        /// Обновляет содержимое существующего документа.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="docId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя, выполняющего обновление.</param>
        /// <param name="newFile">Новый файл для замены содержимого.</param>
        /// <param name="note">Примечание к новой версии (необязательно).</param>
        /// <returns>Идентификатор новой версии документа.</returns>
        Task<Guid> UpdateAsync(Guid roomId, Guid docId, Guid userId, IFormFile newFile, string? note = null);

        /// <summary>
        /// Загружает документ в виде байтового массива.
        /// </summary>
        /// <param name="docId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Кортеж с байтами файла, именем и MIME-типом.</returns>
        Task<(byte[] bytes, string fileName, string ctype)> DownloadAsync(Guid docId, Guid userId);

        /// <summary>
        /// Загружает несколько документов в архиве zip.
        /// </summary>
        /// <param name="ids">Список идентификаторов документов.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Кортеж с zip-архивом и именем файла.</returns>
        Task<(byte[] zip, string fileName)> DownloadManyAsync(IEnumerable<Guid> ids, Guid userId);

        /// <summary>
        /// Получает список документов, доступных пользователю в комнате.
        /// </summary>
        /// <param name="roomId">Идентификатор комнаты.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список описаний документов.</returns>
        Task<IEnumerable<DocumentInfoDto>> ListAsync(Guid roomId, Guid userId);

        /// <summary>
        /// Удаляет указанный документ.
        /// </summary>
        /// <param name="docId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя, инициирующего удаление.</param>
        Task DeleteAsync(Guid docId, Guid userId);

        /// <summary>
        /// Возвращает документы, к которым у пользователя есть доступ, за исключением тех, что он сам создал.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список документов.</returns>
        Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId);

        /// <summary>
        /// Переименовывает документ.
        /// </summary>
        /// <param name="docId">Идентификатор документа.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="newName">Новое имя для документа.</param>
        /// <returns>Идентификатор переименованного документа.</returns>
        Task<Guid> RenameAsync(Guid docId, Guid userId, string newName);
    }
}
