using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MonBackend.Migrations
{
    /// <inheritdoc />
    public partial class SomeChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_DemandesCongés_UserId",
                table: "DemandesCongés");

            migrationBuilder.DropColumn(
                name: "MotDePasse",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Poste",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SoldeCongés",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "DemandesCongés",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SoldeCongesAnnuel",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JourFeries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "DemandesCongés",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "DemandesCongés",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "EnAttente",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDemande",
                table: "DemandesCongés",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Commentaire",
                table: "DemandesCongés",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CommentaireManager",
                table: "DemandesCongés",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.InsertData(
                table: "JourFeries",
                columns: new[] { "Id", "Date", "Description" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jour de l'An" },
                    { 2, new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête de l'Indépendance" },
                    { 3, new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête des Martyrs" },
                    { 4, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête du Travail" },
                    { 5, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête de la République" },
                    { 6, new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête de la Femme" },
                    { 7, new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fête de l'Évacuation" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourFerie_Date_Unique",
                table: "JourFeries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandeCongé_UserDateRange",
                table: "DemandesCongés",
                columns: new[] { "UserId", "DateDebut", "DateFin" });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_JourFerie_Date_Unique",
                table: "JourFeries");

            migrationBuilder.DropIndex(
                name: "IX_DemandeCongé_UserDateRange",
                table: "DemandesCongés");

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JourFeries",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SoldeCongesAnnuel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CommentaireManager",
                table: "DemandesCongés");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "DemandesCongés",
                newName: "type");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "MotDePasse",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Poste",
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

            migrationBuilder.AddColumn<int>(
                name: "SoldeCongés",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JourFeries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "DemandesCongés",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Statut",
                table: "DemandesCongés",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "EnAttente");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDemande",
                table: "DemandesCongés",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Commentaire",
                table: "DemandesCongés",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandesCongés_UserId",
                table: "DemandesCongés",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
