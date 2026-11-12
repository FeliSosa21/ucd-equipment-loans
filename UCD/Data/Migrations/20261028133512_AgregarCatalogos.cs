using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEquipamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DuracionMaxPrestamo = table.Column<int>(type: "int", nullable: false),
                    RequiereAprobacion = table.Column<bool>(type: "bit", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CatalogoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEquipamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposEquipamiento_Catalogos_CatalogoId",
                        column: x => x.CatalogoId,
                        principalTable: "Catalogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesFisicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInstitucional = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    UbiFisica = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NumSerie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoEquipamientoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesFisicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesFisicas_TiposEquipamiento_TipoEquipamientoId",
                        column: x => x.TipoEquipamientoId,
                        principalTable: "TiposEquipamiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposEquipamiento_CatalogoId_Nombre",
                table: "TiposEquipamiento",
                columns: new[] { "CatalogoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesFisicas_IdInstitucional",
                table: "UnidadesFisicas",
                column: "IdInstitucional",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesFisicas_TipoEquipamientoId",
                table: "UnidadesFisicas",
                column: "TipoEquipamientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnidadesFisicas");

            migrationBuilder.DropTable(
                name: "TiposEquipamiento");

            migrationBuilder.DropTable(
                name: "Catalogos");
        }
    }
}
