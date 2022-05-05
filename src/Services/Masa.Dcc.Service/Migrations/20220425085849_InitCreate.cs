// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class InitCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ConfigObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name"),
                    FormatLabelId = table.Column<int>(type: "int", nullable: false, comment: "Format"),
                    TypeLabelId = table.Column<int>(type: "int", nullable: false, comment: "Type"),
                    RelationConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "Relation config object Id"),
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
                name: "IntegrationEventLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    TimesSent = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    EnvironmentClusterId = table.Column<int>(type: "int", nullable: false, comment: "EnvironmentClusterId"),
                    AppId = table.Column<int>(type: "int", nullable: false, comment: "AppId"),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false, comment: "ConfigObjectId"),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "ConfigObjectMains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "ntext", maxLength: 2147483647, nullable: false),
                    TempContent = table.Column<string>(type: "ntext", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigObjectMains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigObjectMains_ConfigObjects_ConfigObjectId",
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
                column: "ConfigObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigObjectMains_ConfigObjectId",
                table: "ConfigObjectMains",
                column: "ConfigObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "index_eventid_version",
                table: "IntegrationEventLog",
                columns: new[] { "EventId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "index_state_modificationtime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "index_state_timessent_modificationtime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "TimesSent", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TypeCode",
                table: "Labels",
                columns: new[] { "TypeCode", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_ConfigObjectId",
                table: "PublicConfigObjects",
                column: "ConfigObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicConfigObjects_PublicConfigId",
                table: "PublicConfigObjects",
                column: "PublicConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigObjects");

            migrationBuilder.DropTable(
                name: "AppSecrets");

            migrationBuilder.DropTable(
                name: "ConfigObjectMains");

            migrationBuilder.DropTable(
                name: "IntegrationEventLog");

            migrationBuilder.DropTable(
                name: "Labels");

            migrationBuilder.DropTable(
                name: "PublicConfigObjects");

            migrationBuilder.DropTable(
                name: "ConfigObjects");

            migrationBuilder.DropTable(
                name: "PublicConfigs");
        }
    }
}
