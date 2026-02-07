using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditLogIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Truncate table
            migrationBuilder.Sql("DELETE FROM AuditLogs");

            // Drop Index dependent on columns
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_AppId_UserId",
                table: "AuditLogs");

            // Drop Columns
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "AppId",
                table: "AuditLogs");

            // Add Columns as Guid
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            // Recreate Index
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AppId_UserId",
                table: "AuditLogs",
                columns: new[] { "AppId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AppId",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
