using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sertifika.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateSignatureWithPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ImageHeight",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageWidth",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageX",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageY",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "TrainingSignatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorTitle",
                table: "TrainingSignatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NameX",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NameY",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowName",
                table: "TrainingSignatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTitle",
                table: "TrainingSignatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TitleX",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TitleY",
                table: "TrainingSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageHeight",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageWidth",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageX",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ImageY",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "TemplateSignatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorTitle",
                table: "TemplateSignatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NameX",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NameY",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowName",
                table: "TemplateSignatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTitle",
                table: "TemplateSignatures",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TitleX",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TitleY",
                table: "TemplateSignatures",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageHeight",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ImageX",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ImageY",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "InstructorTitle",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "NameX",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "NameY",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ShowName",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ShowTitle",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "TitleX",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "TitleY",
                table: "TrainingSignatures");

            migrationBuilder.DropColumn(
                name: "ImageHeight",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "ImageX",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "ImageY",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "InstructorTitle",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "NameX",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "NameY",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "ShowName",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "ShowTitle",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "TitleX",
                table: "TemplateSignatures");

            migrationBuilder.DropColumn(
                name: "TitleY",
                table: "TemplateSignatures");
        }
    }
}
