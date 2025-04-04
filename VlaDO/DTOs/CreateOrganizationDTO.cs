namespace VlaDO.DTOs
{
    public class CreateOrganizationDTO
    {
        public Guid CreatedBy { get; set; }
        public string Name { get; set; }
        public Guid ClientTypeId { get; set; }
        public string? RegistrationCode { get; set; }
    }
}
