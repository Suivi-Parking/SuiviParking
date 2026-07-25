using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    canal = table.Column<string>(type: "text", nullable: false),
                    statut = table.Column<string>(type: "text", nullable: false),
                    destinataire = table.Column<string>(type: "text", nullable: false),
                    sujet = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    entreprise_id = table.Column<int>(type: "integer", nullable: true),
                    abonnement_id = table.Column<int>(type: "integer", nullable: true),
                    parking_id = table.Column<int>(type: "integer", nullable: true),
                    acces_id = table.Column<int>(type: "integer", nullable: true),
                    date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_envoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    erreur_detail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_abonnements_abonnement_id",
                        column: x => x.abonnement_id,
                        principalTable: "abonnements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_acces_acces_id",
                        column: x => x.acces_id,
                        principalTable: "acces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_entreprises_entreprise_id",
                        column: x => x.entreprise_id,
                        principalTable: "entreprises",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_parkings_parking_id",
                        column: x => x.parking_id,
                        principalTable: "parkings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_abonnement_id",
                table: "notifications",
                column: "abonnement_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_acces_id",
                table: "notifications",
                column: "acces_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_entreprise_id",
                table: "notifications",
                column: "entreprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_parking_id",
                table: "notifications",
                column: "parking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
