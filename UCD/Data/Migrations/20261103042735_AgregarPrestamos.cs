using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPrestamos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prestamos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitanteId = table.Column<int>(type: "int", nullable: false),
                    TipoEquipamientoId = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    FechaHoraPrestamo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEsperadaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluadorId = table.Column<int>(type: "int", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prestamos_TiposEquipamiento_TipoEquipamientoId",
                        column: x => x.TipoEquipamientoId,
                        principalTable: "TiposEquipamiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestamos_Usuarios_EvaluadorId",
                        column: x => x.EvaluadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestamos_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesAsignadas",
                columns: table => new
                {
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnidadFisicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesAsignadas", x => new { x.PrestamoId, x.UnidadFisicaId });
                    table.ForeignKey(
                        name: "FK_UnidadesAsignadas_Prestamos_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesAsignadas_UnidadesFisicas_UnidadFisicaId",
                        column: x => x.UnidadFisicaId,
                        principalTable: "UnidadesFisicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_EvaluadorId",
                table: "Prestamos",
                column: "EvaluadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_SolicitanteId",
                table: "Prestamos",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_TipoEquipamientoId",
                table: "Prestamos",
                column: "TipoEquipamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesAsignadas_UnidadFisicaId",
                table: "UnidadesAsignadas",
                column: "UnidadFisicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnidadesAsignadas");

            migrationBuilder.DropTable(
                name: "Prestamos");
        }
    }
}
