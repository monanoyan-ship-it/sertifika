using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddOneDriveOAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriveId",
                table: "OneDriveAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "OneDriveAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriveId",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "OneDriveAccounts");
        }
    }
}
