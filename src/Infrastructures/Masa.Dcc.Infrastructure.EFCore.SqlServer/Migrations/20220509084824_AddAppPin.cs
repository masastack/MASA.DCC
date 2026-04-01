using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddAppPin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigObjects_ConfigObjectId",
                table: "AppConfigObjects");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version foramt is yyyyMMddHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version foramt is YYYYMMDDHHmmss");

            migrationBuilder.CreateTable(
                name: "AppPin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppId = table.Column<int>(type: "int", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigObjectReleases_ConfigObjectId",
                table: "ConfigObjectReleases",
                column: "ConfigObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigObjects_ConfigObjectId",
                table: "AppConfigObjects",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigObjectReleases_ConfigObjects_ConfigObjectId",
                table: "ConfigObjectReleases",
                column: "ConfigObjectId",
                principalTable: "ConfigObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigObjectReleases_ConfigObjects_ConfigObjectId",
                table: "ConfigObjectReleases");

            migrationBuilder.DropTable(
                name: "AppPin");

            migrationBuilder.DropIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects");

            migrationBuilder.DropIndex(
                name: "IX_ConfigObjectReleases_ConfigObjectId",
                table: "ConfigObjectReleases");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigObjects_ConfigObjectId",
                table: "AppConfigObjects");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ConfigObjectReleases",
                type: "varchar(20)",
                nullable: false,
                comment: "Version foramt is YYYYMMDDHHmmss",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldComment: "Version foramt is yyyyMMddHHmmss");

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects",
                column: "ConfigObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigObjects_ConfigObjectId",
                table: "AppConfigObjects",
                column: "ConfigObjectId");
        }
    }
}
