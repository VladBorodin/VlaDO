namespace VlaDO.DTOs
{
    /// <summary>
    /// DTO для восстановления документа с возможным указанием новой комнаты.
    /// </summary>
    public class UnarchiveDto
    {
        /// <summary>
        /// Идентификатор комнаты, куда следует восстановить объект. 
        /// Если null — восстановить в исходную или текущую комнату.
        /// </summary>
        public Guid? TargetRoomId { get; set; }
    }
}
