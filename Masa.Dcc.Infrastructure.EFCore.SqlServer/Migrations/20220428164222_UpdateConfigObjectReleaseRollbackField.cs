// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class UpdateConfigObjectReleaseRollbackField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RollbackToReleaseId",
                table: "ConfigObjectReleases",
                newName: "ToReleaseId");

            migrationBuilder.RenameColumn(
                name: "RollbackFromReleaseId",
                table: "ConfigObjectReleases",
                newName: "FromReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToReleaseId",
                table: "ConfigObjectReleases",
                newName: "RollbackToReleaseId");

            migrationBuilder.RenameColumn(
                name: "FromReleaseId",
                table: "ConfigObjectReleases",
                newName: "RollbackFromReleaseId");
        }
    }
}
