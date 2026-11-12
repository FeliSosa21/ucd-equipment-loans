using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceNumeroSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UnidadesFisicas_NumSerie",
                table: "UnidadesFisicas",
                column: "NumSerie",
                unique: true,
                filter: "[NumSerie] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnidadesFisicas_NumSerie",
                table: "UnidadesFisicas");
        }
    }
}
