using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

public class DocumentFlowContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Document> Documents { get; set; }

    public DocumentFlowContext(DbContextOptions<DocumentFlowContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → Room (1-N)
        modelBuilder.Entity<User>()
            .HasMany(u => u.OwnedRooms)
            .WithOne(r => r.Owner)
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.GuestRooms)
            .WithMany(r => r.Guests)
            .UsingEntity(j => j.ToTable("RoomGuests"));

        // Room → Document (1-N)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Documents)
            .WithOne()                     // в Document нет навигации Room
            .HasForeignKey(d => d.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
