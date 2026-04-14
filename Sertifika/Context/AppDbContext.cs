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

        // Test kullanicilari (sifreler: admin123 / creator123 / viewer123)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1, FirstName = "Admin", LastName = "User",
                Email = "admin@sertifika.com",
                PasswordHash = "$2a$11$55oOjUVSt4TpCtUe5yj0fuYMdIfKHoImj8nFthky1Qa/ocqdqIDe6",
                Role = UserRole.Admin, CreatedAt = seedDate
            },
            new User
            {
                Id = 2, FirstName = "Creator", LastName = "User",
                Email = "creator@sertifika.com",
                PasswordHash = "$2a$11$BkQ2uMbLcaih7xBKAgTN5ugeBJZ0sycLUzMPm4w2UuDaUTL89WQse",
                Role = UserRole.CertificateCreator, CreatedAt = seedDate
            },
            new User
            {
                Id = 3, FirstName = "Viewer", LastName = "User",
                Email = "viewer@sertifika.com",
                PasswordHash = "$2a$11$Dgmd4mzdUuNZtRYdv6SMVewnMcKrtDwxR1SLr048u43oh7khLw0pi",
                Role = UserRole.Viewer, CreatedAt = seedDate
            }
        );
    }
}
