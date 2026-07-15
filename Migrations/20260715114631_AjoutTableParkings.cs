using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AjoutTableParkings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_abonnements_parking_parking_id",
                table: "abonnements");

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
