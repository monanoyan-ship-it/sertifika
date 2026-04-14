using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Certificates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Certificates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Certificates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Certificates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Certificates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Holders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Holders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Holders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$55oOjUVSt4TpCtUe5yj0fuYMdIfKHoImj8nFthky1Qa/ocqdqIDe6");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "creator@sertifika.com", "Creator", true, "User", "$2a$11$BkQ2uMbLcaih7xBKAgTN5ugeBJZ0sycLUzMPm4w2UuDaUTL89WQse", 1, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "viewer@sertifika.com", "Viewer", true, "User", "$2a$11$Dgmd4mzdUuNZtRYdv6SMVewnMcKrtDwxR1SLr048u43oh7khLw0pi", 2, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yazilim gelistirme sertifikalari", true, "Yazilim", null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ag ve siber guvenlik sertifikalari", true, "Ag ve Guvenlik", null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Veritabani yonetimi sertifikalari", true, "Veritabani", null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bulut bilisim sertifikalari", true, "Bulut Teknolojileri", null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Proje yonetimi sertifikalari", true, "Proje Yonetimi", null }
                });

            migrationBuilder.InsertData(
                table: "Holders",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IdentityNumber", "IsActive", "LastName", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ahmet@example.com", "Ahmet", null, true, "Yilmaz", "5551234567", null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ayse@example.com", "Ayse", null, true, "Demir", "5559876543", null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mehmet@example.com", "Mehmet", null, true, "Kaya", "5554567890", null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2b$11$WN/yviAPXEYvPVmfayU28e4cv1s58IAy7XfMQDpfyUDvLjDe6jQeG");

            migrationBuilder.InsertData(
                table: "Certificates",
                columns: new[] { "Id", "CategoryId", "CertificateNumber", "CreatedAt", "Description", "ExpiryDate", "HolderId", "ImageUrl", "IsActive", "IssueDate", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 4, "CERT-2025-001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AZ-900 sertifikasi", new DateTime(2027, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Microsoft Azure Fundamentals", null },
                    { 2, 4, "CERT-2025-002", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AWS cozum mimari sertifikasi", new DateTime(2028, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, new DateTime(2025, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), "AWS Solutions Architect", null },
                    { 3, 2, "CERT-2025-003", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Siber guvenlik temel sertifikasi", new DateTime(2028, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "CompTIA Security+", null },
                    { 4, 3, "CERT-2025-004", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oracle DBA sertifikasi", null, 3, null, true, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oracle Database Administrator", null },
                    { 5, 5, "CERT-2025-005", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Project Management Professional", new DateTime(2028, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PMP", null }
                });
        }
    }
}
