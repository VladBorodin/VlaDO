using Microsoft.AspNetCore.Http;
using System;

namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для обновления документа.
    /// </summary>
    public class UpdateDocumentDto
    {
        /// <summary>
        /// Новое имя документа (опционально).
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Обновлённая заметка к документу (опционально).
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Новая комната, в которую переместить документ (опционально).
        /// </summary>
        public Guid? RoomId { get; set; }

        /// <summary>
        /// Указание родительского документа (опционально).
        /// </summary>
        public Guid? ParentDocId { get; set; }

        /// <summary>
        /// Новый файл документа.
        /// </summary>
        public IFormFile File { get; set; } = null!;
    }
}
