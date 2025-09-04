using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRoleToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesCongés_TypeCongés_TypeCongéId",
                table: "DemandesCongés");

            migrationBuilder.DropTable(
                name: "TypeCongés");

            migrationBuilder.DropIndex(
                name: "IX_DemandesCongés_TypeCongéId",
                table: "DemandesCongés");

            migrationBuilder.DropColumn(
                name: "TypeCongeId",
                table: "DemandesCongés");

            migrationBuilder.RenameColumn(
                name: "TypeCongéId",
                table: "DemandesCongés",
                newName: "type");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "type",
                table: "DemandesCongés",
                newName: "TypeCongéId");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "TypeCongeId",
                table: "DemandesCongés",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TypeCongés",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeCongés", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemandesCongés_TypeCongéId",
                table: "DemandesCongés",
                column: "TypeCongéId");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesCongés_TypeCongés_TypeCongéId",
                table: "DemandesCongés",
                column: "TypeCongéId",
                principalTable: "TypeCongés",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
