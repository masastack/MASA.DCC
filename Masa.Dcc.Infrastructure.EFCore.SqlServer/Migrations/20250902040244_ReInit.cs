using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Infrastructure.EFCore.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ReInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "AppSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EncryptionSecret = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Secret = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSecrets", x => x.Id);
                });

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
                name: "ConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name"),
                    FormatLabelCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Format"),
                    Type = table.Column<int>(type: "int", nullable: false, comment: "Type"),
                    Encryption = table.Column<bool>(type: "bit", nullable: false),
                    Content = table.Column<string>(type: "ntext", nullable: false),
                    TempContent = table.Column<string>(type: "ntext", nullable: false),
                    RelationConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "Relation config object Id"),
                    FromRelation = table.Column<bool>(type: "bit", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigObjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Code"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name"),
                    TypeCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "TypeCode"),
                    TypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "TypeName"),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Description"),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppId = table.Column<int>(type: "int", nullable: false, comment: "AppId"),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "ConfigObjectId"),
                    EnvironmentClusterId = table.Column<int>(type: "int", nullable: false, comment: "EnvironmentClusterId")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConfigObjects_ConfigObjects_ConfigObjectId",
                        column: x => x.ConfigObjectId,
                        principalTable: "ConfigObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BizConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BizConfigId = table.Column<int>(type: "int", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "ConfigObjectId"),
                    EnvironmentClusterId = table.Column<int>(type: "int", nullable: false, comment: "EnvironmentClusterId")
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

            migrationBuilder.CreateTable(
                name: "ConfigObjectReleases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<byte>(type: "tinyint", nullable: false, comment: "Release type"),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "Config object Id"),
                    FromReleaseId = table.Column<int>(type: "int", nullable: false, comment: "Rollback From Release Id"),
                    IsInvalid = table.Column<bool>(type: "bit", nullable: false, comment: "If it is rolled back, it will be true"),
                    Version = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Version format is yyyyMMddHHmmss"),
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
                    table.ForeignKey(
                        name: "FK_ConfigObjectReleases_ConfigObjects_ConfigObjectId",
                        column: x => x.ConfigObjectId,
                        principalTable: "ConfigObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicConfigId = table.Column<int>(type: "int", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "ConfigObjectId"),
                    EnvironmentClusterId = table.Column<int>(type: "int", nullable: false, comment: "EnvironmentClusterId")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicConfigObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicConfigObjects_ConfigObjects_ConfigObjectId",
                        column: x => x.ConfigObjectId,
                        principalTable: "ConfigObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicConfigObjects_PublicConfigs_PublicConfigId",
                        column: x => x.PublicConfigId,
                        principalTable: "PublicConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigObjects_ConfigObjectId",
                table: "AppConfigObjects",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BizConfigObjects_BizConfigId",
                table: "BizConfigObjects",
                column: "BizConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_BizConfigObjects_ConfigObjectId",
                table: "BizConfigObjects",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigObjectReleases_ConfigObjectId",
                table: "ConfigObjectReleases",
                column: "ConfigObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeCode",
                table: "Labels",
                columns: new[] { "TypeCode", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_PublicConfigId",
                table: "PublicConfigObjects",
                column: "PublicConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigObjects");

            migrationBuilder.DropTable(
                name: "AppPin");

            migrationBuilder.DropTable(
                name: "AppSecrets");

            migrationBuilder.DropTable(
                name: "BizConfigObjects");

            migrationBuilder.DropTable(
                name: "ConfigObjectReleases");

            migrationBuilder.DropTable(
                name: "Labels");

            migrationBuilder.DropTable(
                name: "PublicConfigObjects");

            migrationBuilder.DropTable(
                name: "BizConfigs");

            migrationBuilder.DropTable(
                name: "ConfigObjects");

            migrationBuilder.DropTable(
                name: "PublicConfigs");
        }
    }
}
