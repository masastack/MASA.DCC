using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddLabelCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormatLabelId",
                table: "ConfigObjects");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Labels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Code");

            migrationBuilder.AddColumn<string>(
                name: "FormatLabelCode",
                table: "ConfigObjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Format");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Labels");

            migrationBuilder.DropColumn(
                name: "FormatLabelCode",
                table: "ConfigObjects");

            migrationBuilder.AddColumn<int>(
                name: "FormatLabelId",
                table: "ConfigObjects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Format");
        }
    }
}
