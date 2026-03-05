using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Holder> Holders => Set<Holder>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(e => e.CertificateNumber).IsUnique();

            entity.HasOne(e => e.Holder)
                .WithMany(h => h.Certificates)
                .HasForeignKey(e => e.HolderId);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Certificates)
                .HasForeignKey(e => e.CategoryId);
        });

        modelBuilder.Entity<Holder>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Seed Data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Yazilim", Description = "Yazilim gelistirme sertifikalari" },
            new Category { Id = 2, Name = "Ag ve Guvenlik", Description = "Ag ve siber guvenlik sertifikalari" },
            new Category { Id = 3, Name = "Veritabani", Description = "Veritabani yonetimi sertifikalari" },
            new Category { Id = 4, Name = "Bulut Teknolojileri", Description = "Bulut bilisim sertifikalari" },
            new Category { Id = 5, Name = "Proje Yonetimi", Description = "Proje yonetimi sertifikalari" }
        );

        modelBuilder.Entity<Holder>().HasData(
            new Holder { Id = 1, FirstName = "Ahmet", LastName = "Yilmaz", Email = "ahmet@example.com", Phone = "5551234567" },
            new Holder { Id = 2, FirstName = "Ayse", LastName = "Demir", Email = "ayse@example.com", Phone = "5559876543" },
            new Holder { Id = 3, FirstName = "Mehmet", LastName = "Kaya", Email = "mehmet@example.com", Phone = "5554567890" }
        );

        modelBuilder.Entity<Certificate>().HasData(
            new Certificate { Id = 1, Title = "Microsoft Azure Fundamentals", Description = "AZ-900 sertifikasi", CertificateNumber = "CERT-2025-001", IssueDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Utc), HolderId = 1, CategoryId = 4 },
            new Certificate { Id = 2, Title = "AWS Solutions Architect", Description = "AWS cozum mimari sertifikasi", CertificateNumber = "CERT-2025-002", IssueDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 3, 10, 0, 0, 0, DateTimeKind.Utc), HolderId = 1, CategoryId = 4 },
            new Certificate { Id = 3, Title = "CompTIA Security+", Description = "Siber guvenlik temel sertifikasi", CertificateNumber = "CERT-2025-003", IssueDate = new DateTime(2025, 5, 20, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 5, 20, 0, 0, 0, DateTimeKind.Utc), HolderId = 2, CategoryId = 2 },
            new Certificate { Id = 4, Title = "Oracle Database Administrator", Description = "Oracle DBA sertifikasi", CertificateNumber = "CERT-2025-004", IssueDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), HolderId = 3, CategoryId = 3 },
            new Certificate { Id = 5, Title = "PMP", Description = "Project Management Professional", CertificateNumber = "CERT-2025-005", IssueDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 6, 1, 0, 0, 0, DateTimeKind.Utc), HolderId = 2, CategoryId = 5 }
        );
    }
}
