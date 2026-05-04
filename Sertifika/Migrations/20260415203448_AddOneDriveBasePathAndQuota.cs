using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddOneDriveBasePathAndQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BasePath",
                table: "OneDriveAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "QuotaCheckedAt",
                table: "OneDriveAccounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QuotaTotalBytes",
                table: "OneDriveAccounts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QuotaUsedBytes",
                table: "OneDriveAccounts",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePath",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "QuotaCheckedAt",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "QuotaTotalBytes",
                table: "OneDriveAccounts");

            migrationBuilder.DropColumn(
                name: "QuotaUsedBytes",
                table: "OneDriveAccounts");
        }
    }
}
