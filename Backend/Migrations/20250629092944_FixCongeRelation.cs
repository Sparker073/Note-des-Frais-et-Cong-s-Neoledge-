using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixCongeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Congés_Users_UserId1",
                table: "Congés");

            migrationBuilder.DropIndex(
                name: "IX_Congés_UserId1",
                table: "Congés");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Congés");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Congés",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Congés_UserId",
                table: "Congés",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Congés_Users_UserId",
                table: "Congés",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Congés_Users_UserId",
                table: "Congés");

            migrationBuilder.DropIndex(
                name: "IX_Congés_UserId",
                table: "Congés");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Congés",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Congés",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Congés_UserId1",
                table: "Congés",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Congés_Users_UserId1",
                table: "Congés",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
