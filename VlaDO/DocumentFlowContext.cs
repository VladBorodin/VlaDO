using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using VlaDO.Models;

namespace VlaDO;

public class DocumentFlowContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomUser> RoomUsers => Set<RoomUser>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentToken> DocumentTokens => Set<DocumentToken>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityRead> ActivityReads => Set<ActivityRead>();

    public DocumentFlowContext(DbContextOptions<DocumentFlowContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<UserContact>().HasKey(c => new { c.UserId, c.ContactId });
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

        mb.Entity<Room>()
            .HasIndex(r => new { r.OwnerId, r.Title })
            .IsUnique();

        // ───── DocumentToken
        mb.Entity<DocumentToken>()
            .HasIndex(t => t.Token)
            .IsUnique();

        mb.Entity<DocumentToken>()
            .HasOne(dt => dt.User)
            .WithMany()
            .HasForeignKey(dt => dt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Document>()
            .HasOne(d => d.Room)
            .WithMany(r => r.Documents)
            .HasForeignKey(d => d.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<PasswordResetToken>()
            .HasIndex(t => t.Token)
            .IsUnique();

        mb.Entity<PasswordResetToken>()
            .Property(t => t.ExpiresAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        mb.Entity<UserContact>()
            .HasOne(c => c.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<UserContact>()
            .HasOne(c => c.Contact)
            .WithMany()
            .HasForeignKey(c => c.ContactId)
            .OnDelete(DeleteBehavior.NoAction);

        mb.Entity<Activity>(e =>
        {
            e.Property(a => a.Type)
              .HasConversion<string>();

            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.CreatedAt);

            e.Property(a => a.PayloadJson)
             .HasColumnType("TEXT");
        });

        mb.Entity<Activity>()
          .HasIndex(a => a.CreatedAt);

        mb.Entity<Activity>()
          .Property(a => a.CreatedAt)
          .HasDefaultValueSql("CURRENT_TIMESTAMP");

        mb.Entity<ActivityRead>().HasKey(ar => new { ar.ActivityId, ar.UserId });

        mb.Entity<ActivityRead>()
            .HasOne(ar => ar.Activity)
            .WithMany()
            .HasForeignKey(ar => ar.ActivityId);

        mb.Entity<ActivityRead>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId);
    }
}