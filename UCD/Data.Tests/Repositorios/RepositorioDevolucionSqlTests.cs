using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioDevolucionSqlTests
{
    private static readonly DateTime InicioPrestamo = new(2026, 11, 3, 10, 0, 0);
    private const string DniFuncionario = "45678905";

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void ObtenerDevoluciones_DevolucionAgregada_OtroContextoLeeQuienLaRegistroYCuando()
    {
        GuardarConOtroContexto(CrearDevolucion(diasDespuesDelPrestamo: 1));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Devolucion> leidas =
            new RepositorioDevolucionSql(contextoQueLee).ObtenerDevoluciones(new FiltroPorCatalogoYFechas());

        Assert.AreEqual(1, leidas.Count);
        Assert.AreEqual(InicioPrestamo.AddDays(1), leidas[0].FechaHoraDevolucion);
        Assert.AreEqual(DniFuncionario, leidas[0].RegistradaPor.Dni);
    }

    [TestMethod]
    public void ObtenerDevoluciones_DevolucionAgregada_OtroContextoLeeSusUnidadesConCondicion()
    {
        GuardarConOtroContexto(CrearDevolucion(diasDespuesDelPrestamo: 1, CondicionDevolucion.ConFallas, "No enciende"));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Devolucion leida =
            new RepositorioDevolucionSql(contextoQueLee).ObtenerDevoluciones(new FiltroPorCatalogoYFechas()).Single();

        UnidadDevuelta unidad = leida.Unidades.Single();
        Assert.AreEqual("EQP-0001", unidad.Unidad.IdInstitucional);
        Assert.AreEqual(CondicionDevolucion.ConFallas, unidad.Condicion);
        Assert.AreEqual("No enciende", unidad.Observaciones);
    }

    private void GuardarConOtroContexto(params Devolucion[] devoluciones)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioDevolucionSql repositorio = new RepositorioDevolucionSql(contextoQueGuarda);

        foreach (Devolucion devolucion in devoluciones)
        {
            repositorio.AgregarDevolucion(devolucion);
        }
    }

    private static Devolucion CrearDevolucion(int diasDespuesDelPrestamo,
        CondicionDevolucion condicion = CondicionDevolucion.SinObservaciones, string? observaciones = null)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(CrearUsuario("12345672", "ana.perez@ejemplo.com"), tipo,
            cantidadSolicitada: 1, fechaHoraPrestamo: InicioPrestamo);
        prestamo.Activar(new List<UnidadFisica> { unidad });
        RegistroDeAccion registro = RegistroDeAccion.DeUsuario(CrearUsuario(DniFuncionario, "laura.diaz@ejemplo.com"),
            InicioPrestamo.AddDays(diasDespuesDelPrestamo));

        return new Devolucion(prestamo, registro,
            new List<UnidadDevuelta> { new UnidadDevuelta(unidad, condicion, observaciones) });
    }

    private static Usuario CrearUsuario(string dni, string correo)
    {
        return new Usuario(dni: dni, nombre: "Ana", apellido: "Perez", correo: correo, contrasena: "VivaLaOrt2026");
    }
}
