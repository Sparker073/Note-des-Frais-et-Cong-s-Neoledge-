using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentaireManagerToNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "commentaireMananger",
                table: "NotesDeFrais",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commentaireMananger",
                table: "NotesDeFrais");
        }
    }
}
