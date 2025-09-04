using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class Fixsmthng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotDePasse",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotDePasse",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
