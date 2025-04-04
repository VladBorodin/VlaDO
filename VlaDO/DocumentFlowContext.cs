using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

public class DocumentFlowContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserOrganizationRole> UserOrganizationRoles { get; set; }

    public DocumentFlowContext(DbContextOptions<DocumentFlowContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserOrganizationRole>()
            .HasOne(uor => uor.User)
            .WithMany()
            .HasForeignKey(uor => uor.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserOrganizationRole>()
            .HasOne(uor => uor.Organization)
            .WithMany()
            .HasForeignKey(uor => uor.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserOrganizationRole>()
            .HasOne(uor => uor.Role)
            .WithMany()
            .HasForeignKey(uor => uor.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
