using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class ExpandContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentClusterId",
                table: "PublicConfigObjects",
                type: "int",
                nullable: false,
                comment: "EnvironmentClusterId",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigObjectId",
                table: "PublicConfigObjects",
                type: "int",
                nullable: false,
                comment: "ConfigObjectId",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ExpandContent",
                table: "IntegrationEventLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version format is yyyyMMddHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version foramt is yyyyMMddHHmmss");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentClusterId",
                table: "BizConfigObjects",
                type: "int",
                nullable: false,
                comment: "EnvironmentClusterId",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigObjectId",
                table: "BizConfigObjects",
                type: "int",
                nullable: false,
                comment: "ConfigObjectId",
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpandContent",
                table: "IntegrationEventLog");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentClusterId",
                table: "PublicConfigObjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "EnvironmentClusterId");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigObjectId",
                table: "PublicConfigObjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "ConfigObjectId");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version foramt is yyyyMMddHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version format is yyyyMMddHHmmss");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentClusterId",
                table: "BizConfigObjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "EnvironmentClusterId");

            migrationBuilder.AlterColumn<int>(
                name: "ConfigObjectId",
                table: "BizConfigObjects",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "ConfigObjectId");
        }
    }
}
