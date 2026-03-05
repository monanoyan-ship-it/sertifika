using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Orientation = table.Column<int>(type: "integer", nullable: false),
                    BackgroundImageUrl = table.Column<string>(type: "text", nullable: true),
                    LayoutJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IdentityNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TrainingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trainings_CertificateTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CertificateNumber = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    HolderId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificates_Holders_HolderId",
                        column: x => x.HolderId,
                        principalTable: "Holders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    CertificateNumber = table.Column<string>(type: "text", nullable: true),
                    CertificatePdfUrl = table.Column<string>(type: "text", nullable: true),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSignatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    SignatureId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingSignatures_Signatures_SignatureId",
                        column: x => x.SignatureId,
                        principalTable: "Signatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingSignatures_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 5, 9, 13, 50, 914, DateTimeKind.Utc).AddTicks(9311), "Yazilim gelistirme sertifikalari", true, "Yazilim", null },
                    { 2, new DateTime(2026, 3, 5, 9, 13, 50, 914, DateTimeKind.Utc).AddTicks(9814), "Ag ve siber guvenlik sertifikalari", true, "Ag ve Guvenlik", null },
                    { 3, new DateTime(2026, 3, 5, 9, 13, 50, 914, DateTimeKind.Utc).AddTicks(9815), "Veritabani yonetimi sertifikalari", true, "Veritabani", null },
                    { 4, new DateTime(2026, 3, 5, 9, 13, 50, 914, DateTimeKind.Utc).AddTicks(9816), "Bulut bilisim sertifikalari", true, "Bulut Teknolojileri", null },
                    { 5, new DateTime(2026, 3, 5, 9, 13, 50, 914, DateTimeKind.Utc).AddTicks(9817), "Proje yonetimi sertifikalari", true, "Proje Yonetimi", null }
                });

            migrationBuilder.InsertData(
                table: "Holders",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IdentityNumber", "IsActive", "LastName", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(4038), "ahmet@example.com", "Ahmet", null, true, "Yilmaz", "5551234567", null },
                    { 2, new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(4583), "ayse@example.com", "Ayse", null, true, "Demir", "5559876543", null },
                    { 3, new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(4584), "mehmet@example.com", "Mehmet", null, true, "Kaya", "5554567890", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(4943), "admin@sertifika.com", "Admin", true, "User", "$2b$11$WN/yviAPXEYvPVmfayU28e4cv1s58IAy7XfMQDpfyUDvLjDe6jQeG", 0, null });

            migrationBuilder.InsertData(
                table: "Certificates",
                columns: new[] { "Id", "CategoryId", "CertificateNumber", "CreatedAt", "Description", "ExpiryDate", "HolderId", "ImageUrl", "IsActive", "IssueDate", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 4, "CERT-2025-001", new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(5764), "AZ-900 sertifikasi", new DateTime(2027, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Microsoft Azure Fundamentals", null },
                    { 2, 4, "CERT-2025-002", new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(6685), "AWS cozum mimari sertifikasi", new DateTime(2028, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), "AWS Solutions Architect", null },
                    { 3, 2, "CERT-2025-003", new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(6689), "Siber guvenlik temel sertifikasi", new DateTime(2028, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "CompTIA Security+", null },
                    { 4, 3, "CERT-2025-004", new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(6691), "Oracle DBA sertifikasi", null, 3, null, true, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oracle Database Administrator", null },
                    { 5, 5, "CERT-2025-005", new DateTime(2026, 3, 5, 9, 13, 50, 915, DateTimeKind.Utc).AddTicks(6692), "Project Management Professional", new DateTime(2028, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PMP", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CategoryId",
                table: "Certificates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_HolderId",
                table: "Certificates",
                column: "HolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Holders_Email",
                table: "Holders",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_TrainingId",
                table: "Participants",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_TemplateId",
                table: "Trainings",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSignatures_SignatureId",
                table: "TrainingSignatures",
                column: "SignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSignatures_TrainingId",
                table: "TrainingSignatures",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "TrainingSignatures");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Holders");

            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropTable(
                name: "Trainings");

            migrationBuilder.DropTable(
                name: "CertificateTemplates");
        }
    }
}
