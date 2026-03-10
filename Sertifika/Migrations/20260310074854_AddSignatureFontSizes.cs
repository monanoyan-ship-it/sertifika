using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureFontSizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NameFontSize",
                table: "TrainingSignatures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TitleFontSize",
                table: "TrainingSignatures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NameFontSize",
                table: "TemplateSignatures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TitleFontSize",
                table: "TemplateSignatures",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameFontSize",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "TitleFontSize",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "NameFontSize",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "TitleFontSize",
                table: "TemplateSignatures");
        }
    }
}
