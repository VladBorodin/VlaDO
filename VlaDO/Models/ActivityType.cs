namespace VlaDO.Models
{
    /// <summary>
    /// Тип действия, которое было зафиксировано в журнале активности.
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// Был создан новый документ.
        /// </summary>
        CreatedDocument,

        /// <summary>
        /// Документ был обновлён (например, загружен новый файл).
        /// </summary>
        UpdatedDocument,

        /// <summary>
        /// Документ был удалён.
        /// </summary>
        DeletedDocument,

        /// <summary>
        /// Документ был архивирован.
        /// </summary>
        ArchivedDocument,

        /// <summary>
        /// Документ был переименован.
        /// </summary>
        RenamedDocument,

        /// <summary>
        /// Создан новый токен доступа.
        /// </summary>
        IssuedToken,

        /// <summary>
        /// Обновлены параметры токена доступа.
        /// </summary>
        UpdatedToken,

        /// <summary>
        /// Токен был отозван.
        /// </summary>
        RevokedToken,

        /// <summary>
        /// Создана новая комната.
        /// </summary>
        CreatedRoom,

        /// <summary>
        /// Пользователь был приглашён в комнату.
        /// </summary>
        InvitedToRoom,

        /// <summary>
        /// Пользователь принял приглашение в комнату.
        /// </summary>
        AcceptedRoom,

        /// <summary>
        /// Пользователь отклонил приглашение в комнату.
        /// </summary>
        DeclinedRoom,

        /// <summary>
        /// Изменён уровень доступа пользователя в комнате.
        /// </summary>
        UpdatedRoomAccess,

        /// <summary>
        /// Комната была удалена.
        /// </summary>
        DeletedRoom,

        /// <summary>
        /// Доступ пользователя к комнате был отозван.
        /// </summary>
        RevokedRoom,

        /// <summary>
        /// Пользователь был приглашён в контакты.
        /// </summary>
        InvitedToContacts,

        /// <summary>
        /// Пользователь принял приглашение в контакты.
        /// </summary>
        AcceptedContact,

        /// <summary>
        /// Пользователь отклонил приглашение в контакты.
        /// </summary>
        DeclinedContact
    }
}
