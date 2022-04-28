using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddConfigObjectReleaseField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInvalid",
                table: "ConfigObjectReleases",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "If it is rolled back, it will be true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvalid",
                table: "ConfigObjectReleases");
        }
    }
}
