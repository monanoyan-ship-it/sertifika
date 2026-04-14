using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Trainings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Trainings");
        }
    }
}
