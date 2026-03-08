using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparshaERP.Migrations
{
    /// <inheritdoc />
    public partial class CreateAdminAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerformedByName",
                table: "AdminAuditLogs",
                newName: "AdminName");

            migrationBuilder.RenameColumn(
                name: "PerformedByAdminId",
                table: "AdminAuditLogs",
                newName: "AdminUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AdminAuditLogs",
                newName: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "AdminAuditLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "AdminUserId",
                table: "AdminAuditLogs",
                newName: "PerformedByAdminId");

            migrationBuilder.RenameColumn(
                name: "AdminName",
                table: "AdminAuditLogs",
                newName: "PerformedByName");
        }
    }
}
