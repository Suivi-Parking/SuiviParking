using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace parkini.Migrations
{
    /// <inheritdoc />
    public partial class AjoutMiseAJourInfosNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_derniere_modification",
                table: "vehicules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_derniere_modification",
                table: "parkings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "employe_id",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "vehicule_id",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_derniere_modification",
                table: "entreprises",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_derniere_modification",
                table: "employes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "ix_notifications_employe_id",
                table: "notifications",
                column: "employe_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_vehicule_id",
                table: "notifications",
                column: "vehicule_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_employes_employe_id",
                table: "notifications",
                column: "employe_id",
                principalTable: "employes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_vehicules_vehicule_id",
                table: "notifications",
                column: "vehicule_id",
                principalTable: "vehicules",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_employes_employe_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_vehicules_vehicule_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_employe_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_vehicule_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "date_derniere_modification",
                table: "vehicules");

            migrationBuilder.DropColumn(
                name: "date_derniere_modification",
                table: "parkings");

            migrationBuilder.DropColumn(
                name: "employe_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "vehicule_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "date_derniere_modification",
                table: "entreprises");

            migrationBuilder.DropColumn(
                name: "date_derniere_modification",
                table: "employes");
        }
    }
}
