using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AjoutPaiementIdNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "paiement_id",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_paiement_id",
                table: "notifications",
                column: "paiement_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_paiements_paiement_id",
                table: "notifications",
                column: "paiement_id",
                principalTable: "paiements",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_paiements_paiement_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_paiement_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "paiement_id",
                table: "notifications");
        }
    }
}
