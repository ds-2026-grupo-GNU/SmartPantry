using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class Agrega_Cantidad_Unidad_A_DespensaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cantidad",
                table: "AppDespensaItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unidad",
                table: "AppDespensaItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDespensaItems_PropietarioId",
                table: "AppDespensaItems",
                column: "PropietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppDespensaItems_PropietarioId",
                table: "AppDespensaItems");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "AppDespensaItems");

            migrationBuilder.DropColumn(
                name: "Unidad",
                table: "AppDespensaItems");
        }
    }
}
