using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Congés");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "Poste");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "Nom");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotDePasse",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoldeCongés",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JourFeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourFeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TarifsKm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategorieVehicule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarifParKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifsKm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeCongés",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeCongés", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotesDeFrais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateSoumission = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    ProjetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotesDeFrais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotesDeFrais_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotesDeFrais_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandesCongés",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeCongeId = table.Column<int>(type: "int", nullable: false),
                    TypeCongéId = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesCongés", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandesCongés_TypeCongés_TypeCongéId",
                        column: x => x.TypeCongéId,
                        principalTable: "TypeCongés",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandesCongés_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesNotesFrais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteDeFraisId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JustificatifPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarifKmId = table.Column<int>(type: "int", nullable: true),
                    DistanceKm = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesNotesFrais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesNotesFrais_NotesDeFrais_NoteDeFraisId",
                        column: x => x.NoteDeFraisId,
                        principalTable: "NotesDeFrais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesNotesFrais_TarifsKm_TarifKmId",
                        column: x => x.TarifKmId,
                        principalTable: "TarifsKm",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesCongés_TypeCongéId",
                table: "DemandesCongés",
                column: "TypeCongéId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesCongés_UserId",
                table: "DemandesCongés",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesNotesFrais_NoteDeFraisId",
                table: "LignesNotesFrais",
                column: "NoteDeFraisId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesNotesFrais_TarifKmId",
                table: "LignesNotesFrais",
                column: "TarifKmId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesDeFrais_ProjetId",
                table: "NotesDeFrais",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_NotesDeFrais_UserId",
                table: "NotesDeFrais",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "DemandesCongés");

            migrationBuilder.DropTable(
                name: "JourFeries");

            migrationBuilder.DropTable(
                name: "LignesNotesFrais");

            migrationBuilder.DropTable(
                name: "TypeCongés");

            migrationBuilder.DropTable(
                name: "NotesDeFrais");

            migrationBuilder.DropTable(
                name: "TarifsKm");

            migrationBuilder.DropTable(
                name: "Projets");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MotDePasse",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SoldeCongés",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Poste",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "Nom",
                table: "Users",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Congés",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Congés", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Congés_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Congés_UserId",
                table: "Congés",
                column: "UserId");
        }
    }
}
