using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AddParking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "parkings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "est_supprime",
                table: "parkings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "heure_fermeture",
                table: "parkings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "heure_ouverture",
                table: "parkings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "parkings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "parkings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "parkings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ouvert24h24",
                table: "parkings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "statut",
                table: "parkings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "historique_parkings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parking_id = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    date_action = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "ix_historique_parkings_parking_id",
                table: "historique_parkings",
                column: "parking_id");

            migrationBuilder.CreateIndex(
                name: "ix_zones_parking_id",
                table: "zones",
                column: "parking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historique_parkings");

            migrationBuilder.DropTable(
                name: "zones");

            migrationBuilder.DropColumn(
                name: "code",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "est_supprime",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "heure_fermeture",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "heure_ouverture",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "ouvert24h24",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "statut",
                table: "parkings");
        }
    }
}
