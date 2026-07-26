using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AddAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date_heure_entree = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_heure_sortie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acces");
        }
    }
}
