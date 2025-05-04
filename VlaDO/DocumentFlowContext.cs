using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO;

public class DocumentFlowContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomUser> RoomUsers => Set<RoomUser>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentToken> DocumentTokens => Set<DocumentToken>();

    public DocumentFlowContext(DbContextOptions<DocumentFlowContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ───── RoomUser many‑to‑many
        mb.Entity<RoomUser>()
          .HasKey(ru => new { ru.RoomId, ru.UserId });

        mb.Entity<RoomUser>()
          .HasOne(ru => ru.Room)
          .WithMany(r => r.Users)
          .HasForeignKey(ru => ru.RoomId)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<RoomUser>()
          .HasOne(ru => ru.User)
          .WithMany(u => u.Rooms)
          .HasForeignKey(ru => ru.UserId)
          .OnDelete(DeleteBehavior.Cascade);

        // ───── Room → Document (1‑N)
        mb.Entity<Room>()
          .HasMany(r => r.Documents)
          .WithOne(d => d.Room)
          .HasForeignKey(d => d.RoomId)
          .OnDelete(DeleteBehavior.SetNull);

        // ───── DocumentToken
        mb.Entity<DocumentToken>()
          .HasIndex(t => t.Token)
          .IsUnique();

        mb.Entity<DocumentToken>()
          .HasOne(t => t.Document)
          .WithMany(d => d.Tokens)
          .HasForeignKey(t => t.DocumentId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}