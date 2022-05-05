using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddConfigObjectRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigObjectReleases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<byte>(type: "tinyint", nullable: false, comment: "Release type"),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "Config object Id"),
                    RollbackReleaseId = table.Column<int>(type: "int", nullable: false, comment: "Rollback Release Id"),
                    Version = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Version, foramt is YYYYMMDDHHmmss"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name"),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Comment"),
                    Content = table.Column<string>(type: "ntext", maxLength: 2147483647, nullable: false, comment: "Content"),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigObjectReleases", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigObjectReleases");
        }
    }
}
