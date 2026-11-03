using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioMovimientoSqlTests
{
    private static readonly DateTime FechaFija = new(2026, 11, 3, 10, 0, 0);

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void AgregarMovimiento_Devolucion_OtroContextoLeeResponsableYFechaHora()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        Usuario funcionario = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        Movimiento movimiento = Movimiento.DeDevolucion(prestamo, RegistroDeAccion.DeUsuario(funcionario, FechaFija),
            prestamo.UnidadesAsignadas[0]);

        GuardarConOtroContexto(movimiento);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Movimiento leido = contextoQueLee.Movimientos
            .Include("_registro.Responsable")
            .Single();

        Assert.AreEqual(TipoMovimiento.Devolucion, leido.Tipo);
        Assert.AreEqual(FechaFija, leido.FechaHora);
        Assert.IsNotNull(leido.Responsable);
        Assert.AreEqual("45678905", leido.Responsable.Dni);
    }

    [TestMethod]
    public void ObtenerPaginaDeMovimientos_SinFiltro_DelMasRecienteAlMasViejoYEnEmpateElUltimoRegistrado()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        UnidadFisica unidad = prestamo.UnidadesAsignadas[0];
        GuardarConOtroContexto(
            Movimiento.DeSolicitud(prestamo, RegistroDeAccion.DeUsuario(prestamo.Solicitante, FechaFija)),
            Movimiento.DePrestamo(prestamo, RegistroDeAccion.DelSistema(FechaFija), unidad),
            Movimiento.DeDevolucion(prestamo, RegistroDeAccion.DelSistema(FechaFija.AddDays(1)), unidad));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioMovimientoSql repositorio = new RepositorioMovimientoSql(contextoQueLee);
        IReadOnlyList<Movimiento> resultado =
            repositorio.ObtenerPaginaDeMovimientos(new FiltroMovimientos(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Devolucion, TipoMovimiento.Prestamo, TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    private void GuardarConOtroContexto(params Movimiento[] movimientos)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioMovimientoSql repositorio = new RepositorioMovimientoSql(contextoQueGuarda);

        foreach (Movimiento movimiento in movimientos)
        {
            repositorio.AgregarMovimiento(movimiento);
        }
    }

    private static Prestamo CrearPrestamoActivo()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Notebook Lenovo ThinkPad",
            descripcion: "Computadora para laboratorio", duracionMaxPrestamo: 5, requiereAprobacion: false,
            categoria: Categoria.Computacion);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Laboratorio Prueba",
            numSerie: null);
        tipo.AgregarUnidad(unidad);

        Prestamo prestamo = new Prestamo(solicitante: CrearUsuario(dni: "12345672", correo: "renato.alves@ejemplo.com"),
            tipoEquipamiento: tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        prestamo.Activar(new List<UnidadFisica> { unidad });

        return prestamo;
    }

    private static Usuario CrearUsuario(string dni, string correo)
    {
        return new Usuario(dni: dni, nombre: "Renato", apellido: "Alves", correo: correo,
            contrasena: "VivalaOrt2026");
    }
}
