using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioHistoricoTests
{
    private const string TipoConAprobacion = "Proyector Epson";
    private const string TipoSinAprobacion = "Notebook Lenovo ThinkPad";
    private const string DniFuncionario = "45678905";
    private const string DniSupervisor = "89012349";
    private static readonly DateTime FechaFija = new DateTime(2026, 9, 22, 10, 30, 0);

    private MovimientosSqlDePrueba _datos = null!;
    private int _idCatalogo;

    [TestInitialize]
    public void Inicializar()
    {
        _datos = new MovimientosSqlDePrueba();
        _idCatalogo = _datos.GuardarCatalogo("Catalogo de la facultad",
            CrearTipo(TipoConAprobacion, requiereAprobacion: true, idInstitucional: "EQP-0001"),
            CrearTipo(TipoSinAprobacion, requiereAprobacion: false, idInstitucional: "EQP-0002"));
    }

    [TestCleanup]
    public void Limpiar()
    {
        _datos.Dispose();
    }

    [TestMethod]
    public void ListarMovimientos_SinMovimientos_DevuelveListaVacia()
    {
        ServicioHistorico servicio = CrearServicio();

        List<MovimientoDto> movimientos = servicio.ListarMovimientos();

        Assert.AreEqual(0, movimientos.Count);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoDePrestamo_DevuelveSusDatos()
    {
        _datos.GuardarMovimiento(_idCatalogo, TipoConAprobacion, (prestamo, evaluador) =>
        {
            UnidadFisica unidad = prestamo.TipoEquipamiento.Unidades.Single();
            prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });
            return Movimiento.DePrestamo(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija), unidad);
        });
        ServicioHistorico servicio = CrearServicio();

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.AreEqual("Prestamo", movimiento.Tipo);
        Assert.AreEqual("Ana Perez", movimiento.Solicitante);
        Assert.AreEqual("Felipe Sosa", movimiento.Evaluador);
        Assert.AreEqual("EQP-0001", movimiento.Unidad);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoSinEvaluadorNiUnidad_DevuelveNulos()
    {
        GuardarSolicitud(FechaFija);
        ServicioHistorico servicio = CrearServicio();

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.IsNull(movimiento.Evaluador);
        Assert.IsNull(movimiento.Unidad);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoDeUnUsuario_DevuelveSuNombreComoResponsable()
    {
        GuardarSolicitud(FechaFija);
        ServicioHistorico servicio = CrearServicio();

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.AreEqual("Ana Perez", movimiento.Responsable);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoDelSistema_DevuelveSistemaComoResponsable()
    {
        _datos.GuardarMovimiento(_idCatalogo, TipoSinAprobacion, (prestamo, _) =>
        {
            UnidadFisica unidad = prestamo.TipoEquipamiento.Unidades.Single();
            prestamo.Activar(new List<UnidadFisica> { unidad });
            return Movimiento.DePrestamo(prestamo, RegistroDeAccion.DelSistema(FechaFija), unidad);
        });
        ServicioHistorico servicio = CrearServicio();

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.AreEqual("Sistema", movimiento.Responsable);
    }

    [TestMethod]
    public void ListarMovimientos_UsuarioSinPermisoConsultarHistorico_LanzaServicioException()
    {
        _datos.GuardarUsuario(DniFuncionario, TipoUsuario.FuncionarioCE());
        ServicioHistorico servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarMovimientos(_idCatalogo, new FiltroMovimientosDto(), DniFuncionario));
    }

    [TestMethod]
    public void ListarMovimientos_UsuarioInexistente_LanzaServicioException()
    {
        ServicioHistorico servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarMovimientos(_idCatalogo, new FiltroMovimientosDto(), DniSupervisor));
    }

    [TestMethod]
    public void ListarMovimientos_UsuarioDesactivado_LanzaServicioException()
    {
        _datos.GuardarUsuarioDesactivado(DniSupervisor, TipoUsuario.Supervisor());
        ServicioHistorico servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarMovimientos(_idCatalogo, new FiltroMovimientosDto(), DniSupervisor));
    }

    private ServicioHistorico CrearServicio()
    {
        return new ServicioHistorico(new RepositorioUsuarioSql(_datos.Contexto),
            new RepositorioMovimientoSql(_datos.Contexto));
    }

    private void GuardarSolicitud(DateTime fechaHora)
    {
        _datos.GuardarMovimiento(_idCatalogo, TipoConAprobacion, (prestamo, _) =>
            Movimiento.DeSolicitud(prestamo, RegistroDeAccion.DeUsuario(prestamo.Solicitante, fechaHora)));
    }

    private static TipoEquipamiento CrearTipo(string nombre, bool requiereAprobacion, string idInstitucional)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: nombre, descripcion: "Equipo para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: requiereAprobacion, categoria: Categoria.Audiovisual);
        tipo.AgregarUnidad(new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Deposito A", numSerie: null));

        return tipo;
    }
}
