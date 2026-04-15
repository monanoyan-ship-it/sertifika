using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddSmtpOAuthAndGraphFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "SmtpAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "SmtpAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SmtpAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseGraphApi",
                table: "SmtpAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseOAuth",
                table: "SmtpAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "UseGraphApi",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "UseOAuth",
                table: "SmtpAccounts");
        }
    }
}
