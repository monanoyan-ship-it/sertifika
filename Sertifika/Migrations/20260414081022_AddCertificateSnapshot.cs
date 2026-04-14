using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificatePdfUrl",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "CloudFileId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Participants");

            migrationBuilder.CreateTable(
                name: "CertificateSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    CertificateNumber = table.Column<string>(type: "text", nullable: false),
                    Orientation = table.Column<string>(type: "text", nullable: false),
                    BackgroundImagePath = table.Column<string>(type: "text", nullable: true),
                    LayoutJson = table.Column<string>(type: "text", nullable: false),
                    SignaturesJson = table.Column<string>(type: "text", nullable: false),
                    DynamicValuesJson = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateSnapshots_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateSnapshots_CertificateNumber",
                table: "CertificateSnapshots",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificateSnapshots_ParticipantId",
                table: "CertificateSnapshots",
                column: "ParticipantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateSnapshots");

            migrationBuilder.AddColumn<string>(
                name: "CertificatePdfUrl",
                table: "Participants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloudFileId",
                table: "Participants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageType",
                table: "Participants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
