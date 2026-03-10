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
    public DbSet<User> Users => Set<User>();
    public DbSet<CertificateTemplate> CertificateTemplates => Set<CertificateTemplate>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<TrainingSignature> TrainingSignatures => Set<TrainingSignature>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<OneDriveAccount> OneDriveAccounts => Set<OneDriveAccount>();
    public DbSet<SmtpAccount> SmtpAccounts => Set<SmtpAccount>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<TemplateSignature> TemplateSignatures => Set<TemplateSignature>();

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<TemplateSignature>(entity =>
        {
            entity.HasOne(e => e.Template)
                .WithMany(t => t.TemplateSignatures)
                .HasForeignKey(e => e.TemplateId);

            entity.HasOne(e => e.Signature)
                .WithMany()
                .HasForeignKey(e => e.SignatureId);
        });

        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId);
        });

        modelBuilder.Entity<TrainingSignature>(entity =>
        {
            entity.HasOne(e => e.Training)
                .WithMany(t => t.TrainingSignatures)
                .HasForeignKey(e => e.TrainingId);

            entity.HasOne(e => e.Signature)
                .WithMany()
                .HasForeignKey(e => e.SignatureId);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasOne(e => e.Training)
                .WithMany(t => t.Participants)
                .HasForeignKey(e => e.TrainingId);
        });

        // Seed Data - CreatedAt sabit olmali (DateTime.UtcNow kullanilmamali)
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Yazilim", Description = "Yazilim gelistirme sertifikalari", CreatedAt = seedDate },
            new Category { Id = 2, Name = "Ag ve Guvenlik", Description = "Ag ve siber guvenlik sertifikalari", CreatedAt = seedDate },
            new Category { Id = 3, Name = "Veritabani", Description = "Veritabani yonetimi sertifikalari", CreatedAt = seedDate },
            new Category { Id = 4, Name = "Bulut Teknolojileri", Description = "Bulut bilisim sertifikalari", CreatedAt = seedDate },
            new Category { Id = 5, Name = "Proje Yonetimi", Description = "Proje yonetimi sertifikalari", CreatedAt = seedDate }
        );

        modelBuilder.Entity<Holder>().HasData(
            new Holder { Id = 1, FirstName = "Ahmet", LastName = "Yilmaz", Email = "ahmet@example.com", Phone = "5551234567", CreatedAt = seedDate },
            new Holder { Id = 2, FirstName = "Ayse", LastName = "Demir", Email = "ayse@example.com", Phone = "5559876543", CreatedAt = seedDate },
            new Holder { Id = 3, FirstName = "Mehmet", LastName = "Kaya", Email = "mehmet@example.com", Phone = "5554567890", CreatedAt = seedDate }
        );

        // Admin kullanici - sifre: "admin123" (BCrypt hash)
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FirstName = "Admin", LastName = "User", Email = "admin@sertifika.com", PasswordHash = "$2b$11$WN/yviAPXEYvPVmfayU28e4cv1s58IAy7XfMQDpfyUDvLjDe6jQeG", Role = UserRole.Admin, CreatedAt = seedDate }
        );

        modelBuilder.Entity<Certificate>().HasData(
            new Certificate { Id = 1, Title = "Microsoft Azure Fundamentals", Description = "AZ-900 sertifikasi", CertificateNumber = "CERT-2025-001", IssueDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Utc), HolderId = 1, CategoryId = 4, CreatedAt = seedDate },
            new Certificate { Id = 2, Title = "AWS Solutions Architect", Description = "AWS cozum mimari sertifikasi", CertificateNumber = "CERT-2025-002", IssueDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 3, 10, 0, 0, 0, DateTimeKind.Utc), HolderId = 1, CategoryId = 4, CreatedAt = seedDate },
            new Certificate { Id = 3, Title = "CompTIA Security+", Description = "Siber guvenlik temel sertifikasi", CertificateNumber = "CERT-2025-003", IssueDate = new DateTime(2025, 5, 20, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 5, 20, 0, 0, 0, DateTimeKind.Utc), HolderId = 2, CategoryId = 2, CreatedAt = seedDate },
            new Certificate { Id = 4, Title = "Oracle Database Administrator", Description = "Oracle DBA sertifikasi", CertificateNumber = "CERT-2025-004", IssueDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), HolderId = 3, CategoryId = 3, CreatedAt = seedDate },
            new Certificate { Id = 5, Title = "PMP", Description = "Project Management Professional", CertificateNumber = "CERT-2025-005", IssueDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), ExpiryDate = new DateTime(2028, 6, 1, 0, 0, 0, DateTimeKind.Utc), HolderId = 2, CategoryId = 5, CreatedAt = seedDate }
        );
    }
}
