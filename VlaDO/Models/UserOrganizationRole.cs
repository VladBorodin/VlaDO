namespace VlaDO.Models
{
    public class UserOrganizationRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; } = null!;
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }
}
