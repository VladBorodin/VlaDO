using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO
{
    public class DocumentFlowContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ClientType> ClientTypes { get; set; }
        public DbSet<CompanyHash> CompanyHashes { get; set; }

        public DocumentFlowContext(DbContextOptions<DocumentFlowContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasIndex(d => d.ParentDoc);

            modelBuilder.Entity<User>()
                .HasOne<ClientType>()
                .WithMany()
                .HasForeignKey(u => u.ClientTypeId)
                .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить тип клиента, если он используется

            modelBuilder.Entity<User>()
                .HasOne<CompanyHash>()
                .WithOne()
                .HasForeignKey<User>(u => u.CompanyHashId)
                .OnDelete(DeleteBehavior.SetNull); // Если хеш удаляется, CompanyHashId в User становится NULL
        }
    }
}
