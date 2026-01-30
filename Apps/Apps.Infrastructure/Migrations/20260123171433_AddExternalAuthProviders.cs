using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalAuthProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalAuthProvidersJson",
                table: "Apps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalAuthProvidersJson",
                table: "Apps");
        }
    }
}
