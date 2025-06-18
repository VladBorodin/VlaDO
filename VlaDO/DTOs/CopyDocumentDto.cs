namespace VlaDO.DTOs
{
    public class CopyDocumentDto
    {
        public Guid? TargetRoomId { get; set; }  // null = копия вне комнаты
    }

}
