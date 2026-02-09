using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy",
                table: "Apps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyPolicy",
                table: "Apps");
        }
    }
}
