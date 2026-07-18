using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    date_creation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    est_supprime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entreprises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parkings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    adresse = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    ouvert24h24 = table.Column<bool>(type: "boolean", nullable: false),
                    heure_ouverture = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    heure_fermeture = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    statut = table.Column<string>(type: "text", nullable: false),
                    est_supprime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parkings", x => x.id);
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
                    date_rattachement = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                name: "abonnements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entreprise_id = table.Column<int>(type: "integer", nullable: false),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    date_debut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date_fin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                        name: "fk_abonnements_parkings_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parkings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historique_parkings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    date_action = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historique_parkings", x => x.id);
                    table.ForeignKey(
                        name: "fk_historique_parkings_parkings_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parkings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    nom = table.Column<string>(type: "text", nullable: false),
                    nombre_places = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    est_active = table.Column<bool>(type: "boolean", nullable: false),
                    places_occupees = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_zones_parkings_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parkings",
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
                    date_creation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "acces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date_heure_entree = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date_heure_sortie = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    est_autorise = table.Column<bool>(type: "boolean", nullable: false),
                    motif_refus = table.Column<string>(type: "text", nullable: true),
                    statut = table.Column<string>(type: "text", nullable: false),
                    vehicule_id = table.Column<int>(type: "integer", nullable: false),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    abonnement_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_acces", x => x.id);
                    table.ForeignKey(
                        name: "fk_acces_abonnements_abonnement_id",
                        column: x => x.abonnement_id,
                        principalTable: "abonnements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_acces_parkings_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parkings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_acces_vehicules_vehicule_id",
                        column: x => x.vehicule_id,
                        principalTable: "vehicules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "ix_acces_abonnement_id",
                table: "acces",
                column: "abonnement_id");

            migrationBuilder.CreateIndex(
                name: "ix_acces_parking_id",
                table: "acces",
                column: "parking_id");

            migrationBuilder.CreateIndex(
                name: "ix_acces_vehicule_id",
                table: "acces",
                column: "vehicule_id");

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
                name: "ix_historique_parkings_parking_id",
                table: "historique_parkings",
                column: "parking_id");

            migrationBuilder.CreateIndex(
                name: "ix_utilisateurs_email",
                table: "utilisateurs",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_utilisateurs_role_id",
                table: "utilisateurs",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicules_employe_id",
                table: "vehicules",
                column: "employe_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicules_immatriculation",
                table: "vehicules",
                column: "immatriculation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_zones_parking_id",
                table: "zones",
                column: "parking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acces");

            migrationBuilder.DropTable(
                name: "historique_parkings");

            migrationBuilder.DropTable(
                name: "utilisateurs");

            migrationBuilder.DropTable(
                name: "zones");

            migrationBuilder.DropTable(
                name: "abonnements");

            migrationBuilder.DropTable(
                name: "vehicules");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "parkings");

            migrationBuilder.DropTable(
                name: "employes");

            migrationBuilder.DropTable(
                name: "entreprises");
        }
    }
}
