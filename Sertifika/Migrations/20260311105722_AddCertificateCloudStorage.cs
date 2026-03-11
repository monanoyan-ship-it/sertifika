using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateCloudStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudFileId",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Participants");
        }
    }
}
