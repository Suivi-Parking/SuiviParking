using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployesEtVehicules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_abonnements_parking_parking_id",
                table: "abonnements");

            migrationBuilder.DropTable(
                name: "employe");

            migrationBuilder.DropPrimaryKey(
                name: "pk_parking",
                table: "parking");

            migrationBuilder.RenameTable(
                name: "parking",
                newName: "parkings");

            migrationBuilder.AddPrimaryKey(
                name: "pk_parkings",
                table: "parkings",
                column: "id");

            migrationBuilder.CreateTable(
                name: "employes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false),
                    prenom = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telephone = table.Column<string>(type: "text", nullable: false),
                    fonction = table.Column<string>(type: "text", nullable: false),
                    statut = table.Column<string>(type: "text", nullable: false),
                    date_rattachement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entreprise_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employes", x => x.id);
                    table.ForeignKey(
                        name: "fk_employes_entreprises_entreprise_id",
                        column: x => x.entreprise_id,
                        principalTable: "entreprises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    immatriculation = table.Column<string>(type: "text", nullable: false),
                    marque = table.Column<string>(type: "text", nullable: false),
                    modele = table.Column<string>(type: "text", nullable: false),
                    couleur = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    statut = table.Column<string>(type: "text", nullable: false),
                    employe_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicules", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicules_employes_employe_id",
                        column: x => x.employe_id,
                        principalTable: "employes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_employes_email",
                table: "employes",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employes_entreprise_id",
                table: "employes",
                column: "entreprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicules_employe_id",
                table: "vehicules",
                column: "employe_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicules_immatriculation",
                table: "vehicules",
                column: "immatriculation",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_abonnements_parkings_parking_id",
                table: "abonnements",
                column: "parking_id",
                principalTable: "parkings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_abonnements_parkings_parking_id",
                table: "abonnements");

            migrationBuilder.DropTable(
                name: "vehicules");

            migrationBuilder.DropTable(
                name: "employes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_parkings",
                table: "parkings");

            migrationBuilder.RenameTable(
                name: "parkings",
                newName: "parking");

            migrationBuilder.AddPrimaryKey(
                name: "pk_parking",
                table: "parking",
                column: "id");

            migrationBuilder.CreateTable(
                name: "employe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entreprise_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    telephone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employe", x => x.id);
                    table.ForeignKey(
                        name: "fk_employe_entreprises_entreprise_id",
                        column: x => x.entreprise_id,
                        principalTable: "entreprises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_employe_entreprise_id",
                table: "employe",
                column: "entreprise_id");

            migrationBuilder.AddForeignKey(
                name: "fk_abonnements_parking_parking_id",
                table: "abonnements",
                column: "parking_id",
                principalTable: "parking",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
