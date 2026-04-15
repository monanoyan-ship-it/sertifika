using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddEnabledAndLastTestFieldsToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "SmtpAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastTestError",
                table: "SmtpAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastTestStatus",
                table: "SmtpAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTestedAt",
                table: "SmtpAccounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "OneDriveAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastTestError",
                table: "OneDriveAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastTestStatus",
                table: "OneDriveAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTestedAt",
                table: "OneDriveAccounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestError",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestStatus",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestedAt",
                table: "SmtpAccounts");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestError",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestStatus",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "LastTestedAt",
                table: "OneDriveAccounts");
        }
    }
}
