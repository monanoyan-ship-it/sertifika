using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddContactAndTrainingInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Trainings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "Trainings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorTitle",
                table: "Trainings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Participants",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_CompanyId",
                table: "Trainings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ContactId",
                table: "Participants",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CompanyId_Email",
                table: "Contacts",
                columns: new[] { "CompanyId", "Email" });

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Contacts_ContactId",
                table: "Participants",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainings_Companies_CompanyId",
                table: "Trainings",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Contacts_ContactId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Trainings_Companies_CompanyId",
                table: "Trainings");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Trainings_CompanyId",
                table: "Trainings");

            migrationBuilder.DropIndex(
                name: "IX_Participants_ContactId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "InstructorTitle",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Participants");
        }
    }
}
