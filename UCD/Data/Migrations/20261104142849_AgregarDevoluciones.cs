using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDevoluciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devoluciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistradaPorId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devoluciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devoluciones_Prestamos_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devoluciones_Usuarios_RegistradaPorId",
                        column: x => x.RegistradaPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesDevueltas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnidadFisicaId = table.Column<int>(type: "int", nullable: false),
                    Condicion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DevolucionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesDevueltas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesDevueltas_Devoluciones_DevolucionId",
                        column: x => x.DevolucionId,
                        principalTable: "Devoluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnidadesDevueltas_UnidadesFisicas_UnidadFisicaId",
                        column: x => x.UnidadFisicaId,
                        principalTable: "UnidadesFisicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_PrestamoId",
                table: "Devoluciones",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_RegistradaPorId",
                table: "Devoluciones",
                column: "RegistradaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesDevueltas_DevolucionId",
                table: "UnidadesDevueltas",
                column: "DevolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesDevueltas_UnidadFisicaId",
                table: "UnidadesDevueltas",
                column: "UnidadFisicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnidadesDevueltas");

            migrationBuilder.DropTable(
                name: "Devoluciones");
        }
    }
}
