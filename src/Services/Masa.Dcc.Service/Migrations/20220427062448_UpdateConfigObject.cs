// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class UpdateConfigObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigObjectMains");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ConfigObjects",
                type: "ntext",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TempContent",
                table: "ConfigObjects",
                type: "ntext",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ConfigObjects");

            migrationBuilder.DropColumn(
                name: "TempContent",
                table: "ConfigObjects");

            migrationBuilder.CreateTable(
                name: "ConfigObjectMains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigObjectId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "ntext", maxLength: 2147483647, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempContent = table.Column<string>(type: "ntext", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ConfigObjectMains_ConfigObjectId",
                table: "ConfigObjectMains",
                column: "ConfigObjectId",
                unique: true);
        }
    }
}
