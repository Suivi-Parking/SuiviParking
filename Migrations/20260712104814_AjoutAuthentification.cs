using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AjoutAuthentification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entreprises",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false),
                    adresse = table.Column<string>(type: "text", nullable: false),
                    telephone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    matricule_fiscal = table.Column<string>(type: "text", nullable: false),
                    secteur = table.Column<string>(type: "text", nullable: false),
                    date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    est_supprime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entreprises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false),
                    adresse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entreprise_id = table.Column<int>(type: "integer", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "abonnements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entreprise_id = table.Column<int>(type: "integer", nullable: false),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    date_debut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    statut = table.Column<string>(type: "text", nullable: false),
                    montant_mensuel = table.Column<decimal>(type: "numeric", nullable: false),
                    type_abonnement = table.Column<string>(type: "text", nullable: false),
                    raison = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_abonnements", x => x.id);
                    table.ForeignKey(
                        name: "fk_abonnements_entreprises_entreprise_id",
                        column: x => x.entreprise_id,
                        principalTable: "entreprises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_abonnements_parking_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parking",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "utilisateurs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false),
                    prenom = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    mot_de_passe_hash = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    est_actif = table.Column<bool>(type: "boolean", nullable: false),
                    date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilisateurs", x => x.id);
                    table.ForeignKey(
                        name: "fk_utilisateurs_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_abonnements_entreprise_id",
                table: "abonnements",
                column: "entreprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_abonnements_parking_id",
                table: "abonnements",
                column: "parking_id");

            migrationBuilder.CreateIndex(
                name: "ix_employe_entreprise_id",
                table: "employe",
                column: "entreprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilisateurs_email",
                table: "utilisateurs",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_utilisateurs_role_id",
                table: "utilisateurs",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abonnements");

            migrationBuilder.DropTable(
                name: "employe");

            migrationBuilder.DropTable(
                name: "utilisateurs");

            migrationBuilder.DropTable(
                name: "parking");

            migrationBuilder.DropTable(
                name: "entreprises");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
