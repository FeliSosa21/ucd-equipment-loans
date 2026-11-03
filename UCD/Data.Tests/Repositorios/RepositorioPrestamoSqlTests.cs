using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioPrestamoSqlTests
{
    private const string DniSolicitante = "52083412";
    private const string NombreTipo = "Notebook Lenovo ThinkPad";
    private const int CantidadSolicitada = 2;
    private static readonly DateTime FechaFija = new(2026, 11, 3, 10, 0, 0);

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    }

    [TestMethod]
    public void AgregarPrestamo_PrestamoValido_OtroContextoLoEncuentra()
    {
        Prestamo prestamo = CrearPrestamo();

        using (SqlContext contextoQueGuarda = new SqlContext(_opciones))
        {
            new RepositorioPrestamoSql(contextoQueGuarda).AgregarPrestamo(prestamo);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo? leido = contextoQueLee.Prestamos.Find(prestamo.Id);

        Assert.IsNotNull(leido);
        Assert.AreEqual(prestamo.CantidadSolicitada, leido.CantidadSolicitada);
        Assert.AreEqual(prestamo.FechaHoraPrestamo, leido.FechaHoraPrestamo);
        Assert.AreEqual(prestamo.FechaEsperadaDevolucion, leido.FechaEsperadaDevolucion);
    }

    private static Prestamo CrearPrestamo()
    {
        return new Prestamo(
            solicitante: CrearSolicitante(),
            tipoEquipamiento: CrearTipoEquipamiento(),
            cantidadSolicitada: CantidadSolicitada,
            fechaHoraPrestamo: FechaFija);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: DniSolicitante,
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(
            nombre: NombreTipo,
            descripcion: "Computadora para laboratorio",
            duracionMaxPrestamo: 5,
            requiereAprobacion: false,
            categoria: Categoria.Computacion);

        return tipo;
    }
}