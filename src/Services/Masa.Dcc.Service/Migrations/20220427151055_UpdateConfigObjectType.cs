using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class UpdateConfigObjectType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeLabelId",
                table: "ConfigObjects",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version foramt is YYYYMMDDHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version, foramt is YYYYMMDDHHmmss");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ConfigObjects",
                newName: "TypeLabelId");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version, foramt is YYYYMMDDHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version foramt is YYYYMMDDHHmmss");
        }
    }
}
