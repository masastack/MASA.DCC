using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddBizConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BizConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BizConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BizConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BizConfigId = table.Column<int>(type: "int", nullable: false),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentClusterId = table.Column<int>(type: "int", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BizConfigObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BizConfigObjects_BizConfigs_BizConfigId",
                        column: x => x.BizConfigId,
                        principalTable: "BizConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BizConfigObjects_ConfigObjects_ConfigObjectId",
                        column: x => x.ConfigObjectId,
                        principalTable: "ConfigObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BizConfigObjects_BizConfigId",
                table: "BizConfigObjects",
                column: "BizConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_BizConfigObjects_ConfigObjectId",
                table: "BizConfigObjects",
                column: "ConfigObjectId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BizConfigObjects");

            migrationBuilder.DropTable(
                name: "BizConfigs");
        }
    }
}
