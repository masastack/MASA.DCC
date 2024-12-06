using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Dcc.Service.Admin.Migrations
{
    public partial class AddEncryption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToReleaseId",
                table: "ConfigObjectReleases");

            migrationBuilder.RenameIndex(
                name: "index_state_timessent_modificationtime",
                table: "IntegrationEventLog",
                newName: "IX_State_TimesSent_MTime");

            migrationBuilder.RenameIndex(
                name: "index_state_modificationtime",
                table: "IntegrationEventLog",
                newName: "IX_State_MTime");

            migrationBuilder.RenameIndex(
                name: "index_eventid_version",
                table: "IntegrationEventLog",
                newName: "IX_EventId_Version");

            migrationBuilder.AddColumn<bool>(
                name: "Encryption",
                table: "ConfigObjects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Encryption",
                table: "ConfigObjects");

            migrationBuilder.RenameIndex(
                name: "IX_State_TimesSent_MTime",
                table: "IntegrationEventLog",
                newName: "index_state_timessent_modificationtime");

            migrationBuilder.RenameIndex(
                name: "IX_State_MTime",
                table: "IntegrationEventLog",
                newName: "index_state_modificationtime");

            migrationBuilder.RenameIndex(
                name: "IX_EventId_Version",
                table: "IntegrationEventLog",
                newName: "index_eventid_version");

            migrationBuilder.AddColumn<int>(
                name: "ToReleaseId",
                table: "ConfigObjectReleases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Rollback To Release Id");
        }
    }
}
