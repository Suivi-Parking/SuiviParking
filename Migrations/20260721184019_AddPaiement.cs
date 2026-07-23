using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paiements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    abonnement_id = table.Column<int>(type: "integer", nullable: false),
                    montant = table.Column<decimal>(type: "numeric", nullable: false),
                    date_echeance = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_paiement = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mode_paiement = table.Column<int>(type: "integer", nullable: true),
                    reference_transaction = table.Column<string>(type: "text", nullable: true),
                    periode_couverte = table.Column<string>(type: "text", nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paiements", x => x.id);
                    table.ForeignKey(
                        name: "fk_paiements_abonnements_abonnement_id",
                        column: x => x.abonnement_id,
                        principalTable: "abonnements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_paiements_abonnement_id",
                table: "paiements",
                column: "abonnement_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paiements");
        }
    }
}
