using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RetirarSemillaTiposDeUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TiposDeUsuario",
                keyColumn: "Id",
                keyValue: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TiposDeUsuario",
                columns: new[] { "Id", "Permisos", "Descripcion", "Imagen", "Nombre", "MaxPrestamosConcurrentes", "MaxUnidadesPorCatalogo" },
                values: new object[,]
                {
                    { 1, "", "Cuenta registrada que espera que un administrador le asigne un tipo de usuario.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABi0lEQVR42u2b0U0EMQxEk9H2BkVABVAOVABFcNXBFxI60LGb2OPxxv48ndZ+Y2eVeJ3WysrKVrbOdPby9vG597/Pj/c9vQBHgKME6erg3kL0LOBeQvRs4NZCIDO8hX9khreIo2cGt1gSOBP8SHw4E/xInFtUkE8Pd79+e32/6G6FrbL/F7iXEHveB1CDP/I/i7ihBs8WAVHrnSnCsABZ3vozVQDV7LOqAGfP/n88aIsbVsj+La6qAG8Hs7s67+1xVQBj/Y9m0SP713y0CjgKwzoZUpfAXijmsZjeD/iGU+kHhDVEImAlBGA2RGQEmOkFeIuxKZ8Efz7DSwgowzOOxcgA7/lcZIH3ej4sv7Yyenizfq75kCXzXv6QEd7SL7LCW/lH5IRWE/h0jszZt4gD0XN60YMT1RIb/bysUv574rnFURWgMrMbNTQFpcHliIkxqE1vs8fltuw9PepWOEsVHIkTKnP7UZOi/QyzwjOJgeItDmYcUL3KwvJfN0Za3RlqdWusLXxvsKxscfsCzJLHh9CjvCgAAAAASUVORK5CYII=", "Sin permisos", 0, 0 },
                    { 2, "ConsultarCatalogo,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes", "Estudiantes de la facultad: consultan el catálogo y solicitan préstamos de equipamiento.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bQU7EMAxFk69u4EpwNVjA1eBKzBJWSGhAQ5vY39+NvRyNar9vp0pcp7WysrKVrTOdPbx8fO797/vzfU8vwBHgKEG6Ori3ED0LuJcQPRu4tRDIDG/hH5nhLeLomcEtlgTOBD8SH84EPxLnFhXk29Pdr98eXy+6W2Gr7P8F7iXEnvcB1OCP/M8ibqjBs0VA1HpnijAsQJa3/kwVQDX7rCrA2bP/Hw/a4oYVsn+LqyrA28Hsrs57e1wVwFj/o1n0yP41H60CjsKwTobUJbAXinkspvcDvuFU+gFhDZEIWAkBmA0RGQFmegHeYmzKJ8Gfz/ASAsrwjGMxMsB7PhdZ4L2eD8uvrYwe3qyfaz5kybyXP2SEt/SLrPBW/hE5odUEPp0jc/Yt4kD0nF704ES1xEY/L6uU/554bnFUBajM7EYNTUFpcDliYgxq09vscbkte0+PuhXOUgVH4oTK3H7UpGg/w6zwTGKgeIuDGQdUr7Kw/NeNkVZ3hlrdGmsL3xssK1vcvgDTb8v7Luk3MwAAAABJRU5ErkJggg==", "Estudiante", 2, 2 },
                    { 3, "ConsultarCatalogo,ConsultarHistorico,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes", "Docentes de la facultad: solicitan préstamos con topes más altos y consultan el histórico.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bUU7EMAxEk1EPAJeCW4HgVnAp9gbwhYQWtLSJPR439udqVfuNnSpxndbKyspWts50dvf28rn3v5fH555egCPAUYJ0dXBvIXoWcC8hejZwayGQGd7CPzLDW8TRM4NbLAmcCX4kPpwJfiTOLSrIj4enX7/dv7/qboWtsv8XuJcQe94HUIM/8j+LuKEGzxYBUeudKcKwAFne+jNVANXss6oAZ8/+fzxoixtWyP4trqoAbwezuzrv7XFVAGP9j2bRI/vXfLQKOArDOhlSl8BeKOaxmN4P+IZT6QeENUQiYCUEYDZEZASY6QV4i7EpnwR/PsNLCCjDM47FyADv+Vxkgfd6Piy/tjJ6eLN+rvmQJfNe/pAR3tIvssJb+UfkhFYT+HSOzNm3iAPRc3rRgxPVEhv9vKxS/nviucVRFaAysxs1NAWlweWIiTGoTW+zx+W27D096lY4SxUciRMqc/tRk6L9DLPCM4mB4i0OZhxQvcrC8l83RlrdGWp1a6wtfG+wrGxx+wLGV9PGmW25PwAAAABJRU5ErkJggg==", "Docente", 3, 40 },
                    { 4, "ConsultarCatalogo,GestionarUnidades,RealizarPrestamo,RegistrarDevolucion", "Personal del centro: gestiona unidades, entrega préstamos y registra devoluciones.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABh0lEQVR42u2bwU7EMAxEk1FPLD8JHwY/yXJlT0hoQUub2ONxYx9Xq9pv7FSJ67RWVla2snWms+vb5Wvvfy+v155egCPAUYJ0dXBvIXoWcC8hejZwayGQGd7CPzLDW8TRM4NbLAmcCX4kPpwJfiTOLSrIp5ePX799vj/rboWtsv8XuJcQe94HUIM/8j+LuKEGzxYBUeudKcKwAFne+jNVANXss6oAZ8/+fzxoixtWyP4jrqoAbwezuzrv7XFVAGP9j2bRI/v3fLQKOArDOhlSl8BeKOaxmN4P+IZT6QeENUQiYCUEYDZEZASY6QV4i7EpnwR/PsNLCCjDM47FyADv+Vxkgfd6Piy/tjJ6eLN+7vmQJfNe/pAR3tIvssJb+UfkhFYT+HSOzNm3iAPRc3rRgxPVEhv9vKxS/nviecRRFaAysxs1NAWlweWIiTGoTW+zx+W27D096lY4SxUciRMqc/tRk6L9DLPCM4mB4i0OZhxQvcrC8l83RlrdGWp1a6wtfG+wrGxxuwHl282KHT/3UQAAAABJRU5ErkJggg==", "Funcionario del centro de equipamiento", 0, 0 },
                    { 5, "ConsultarCatalogo,ConsultarHistorico,GestionarUnidades,RegistrarDevolucion,GestionarCatalogo,EvaluarSolicitud,ConsultarPrestamos", "Gestiona el catálogo y las unidades, evalúa solicitudes y consulta préstamos e histórico.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bQU7EMAxFk69u4IxwCzgF3ALOyCxhhYQGNLSJ/f3d2MvRqPb7dqrEdVorKytb2TrT2cvDx+fe/z6/3/f0AhwBjhKkq4N7C9GzgHsJ0bOBWwuBzPAW/pEZ3iKOnhncYkngTPAj8eFM8CNxblFBPr3d/frt9fGiuxW2yv5f4F5C7HkfQA3+yP8s4oYaPFsERK13pgjDAmR5689UAVSzz6oCnD37//GgLW5YIfu3uKoCvB3M7uq8t8dVAYz1P5pFj+xf89Eq4CgM62RIXQJ7oZjHYno/4BtOpR8Q1hCJgJUQgNkQkRFgphfgLcamfBL8+QwvIaAMzzgWIwO853ORBd7r+bD82sro4c36ueZDlsx7+UNGeEu/yApv5R+RE1pN4NM5MmffIg5Ez+lFD05US2z087JK+e+J5xZHVYDKzG7U0BSUBpcjJsagNr3NHpfbsvf0qFvhLFVwJE6ozO1HTYr2M8wKzyQGirc4mHFA9SoLy3/dGGl1Z6jVrbG28L3BsrLF7QtqKcjdsAOVfgAAAABJRU5ErkJggg==", "Supervisor", 0, 0 }
                });

            migrationBuilder.InsertData(
                table: "TiposDeUsuario",
                columns: new[] { "Id", "Permisos", "Descripcion", "Imagen", "Nombre" },
                values: new object[] { 6, "ConsultarCatalogo,ConsultarHistorico,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes,GestionarUnidades,RealizarPrestamo,RegistrarDevolucion,GestionarCatalogo,EvaluarSolicitud,ConsultarPrestamos,GestionarUsuarios", "Gestiona usuarios y tipos de usuario; tiene todos los permisos y no tiene límites.", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABh0lEQVR42u2b0W3DMAxEpYMHaHdKpk52ajdovwoEaZHaEnk8WuRnEJh8R8qQaKq1srKyla0znX1cLl97//t+v/f0AhwBjhKkq4N7C9GzgHsJ0bOBWwuBzPAW/pEZ3iKOnhncYkngTPAj8eFM8CNxblFBvt1uv377vF51t8JW2f8L3EuIPe8DqMEf+Z9F3FCDZ4uAqPXOFGFYgCxv/ZkqgGr2WVWAs2f/Px60xQ0rZP8VV1WAt4PZXZ339rgqgLH+R7Pokf1nPloFHIVhnQypS2AvFPNYTO8H/MCp9APCGiIRsBICMBsiMgLM9AK8xdiUT4KPz/ASAsrwjGMxMsB7PhdZ4L2eD8uvrYwe3qyfZz5kybyXP2SEt/SLrPBW/hE5odUEPp0jc/Yt4kD0nF704ES1xEY/L6uU/554XnFUBajM7EYNTUFpcDliYgxq09vscbkte0+PuhXOUgVH4oTK3H7UpGg/w6zwTGKgeIuDGQdUr7Kw/NeNkVZ3hlrdGmsL3xssK1vcvgG0hdBvKvPwGAAAAABJRU5ErkJggg==", "Administrador" });
        }
    }
}
