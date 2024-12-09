// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class UpdateConfigObjectReleaseField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RollbackReleaseId",
                table: "ConfigObjectReleases");

            migrationBuilder.AddColumn<int>(
                name: "RollbackFromReleaseId",
                table: "ConfigObjectReleases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Rollback From Release Id");

            migrationBuilder.AddColumn<int>(
                name: "RollbackToReleaseId",
                table: "ConfigObjectReleases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Rollback To Release Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RollbackFromReleaseId",
                table: "ConfigObjectReleases");

            migrationBuilder.DropColumn(
                name: "RollbackToReleaseId",
                table: "ConfigObjectReleases");

            migrationBuilder.AddColumn<int>(
                name: "RollbackReleaseId",
                table: "ConfigObjectReleases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Rollback Release Id");
        }
    }
}
