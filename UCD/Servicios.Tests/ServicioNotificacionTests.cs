using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioNotificacionTests
{
    private static readonly DateTime Hoy = new DateTime(2026, 11, 10, 9, 0, 0);
    private static readonly DateTime PedidoQueVencioAyer = new DateTime(2026, 11, 2, 10, 0, 0);
    private static readonly DateTime PedidoQueVenceHoy = new DateTime(2026, 11, 3, 10, 0, 0);

    private const string DniDocente = "12345672";

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
        GuardarUsuarios();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_UnPrestamoVencido_CreaUnaDeVencidoSinLeerConLaFechaDeHoy()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        ServicioNotificacion servicio = CrearServicio();

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        IReadOnlyList<Notificacion> creadas = LeerNoLeidasDe(DniDocente);
        Assert.AreEqual(1, creadas.Count);
        Assert.AreEqual(idPrestamo, creadas[0].Prestamo.Id);
        Assert.AreEqual(TipoNotificacion.PrestamoVencido, creadas[0].Tipo);
        Assert.AreEqual(Hoy, creadas[0].FechaHora);
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_PrestamoQueVenceHoy_NoCreaNinguna()
    {
        GuardarPrestamoActivo(PedidoQueVenceHoy);
        ServicioNotificacion servicio = CrearServicio();

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        Assert.AreEqual(0, LeerNoLeidasDe(DniDocente).Count);
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_PrestamoDevuelto_NoCreaNinguna()
    {
        GuardarPrestamoDevuelto(PedidoQueVencioAyer);
        ServicioNotificacion servicio = CrearServicio();

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        Assert.AreEqual(0, LeerNoLeidasDe(DniDocente).Count);
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_YaTieneUnaDeVencidoSinLeer_NoCreaOtra()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idAnterior = GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        ServicioNotificacion servicio = CrearServicio();

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        CollectionAssert.AreEqual(new List<int> { idAnterior },
            LeerNoLeidasDe(DniDocente).Select(notificacion => notificacion.Id).ToList());
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_LaAnteriorEstaLeida_CreaOtra()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idAnterior = GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        MarcarLeidaConOtroContexto(idAnterior);
        ServicioNotificacion servicio = CrearServicio();

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        IReadOnlyList<Notificacion> sinLeer = LeerNoLeidasDe(DniDocente);
        Assert.AreEqual(1, sinLeer.Count);
        Assert.AreNotEqual(idAnterior, sinLeer[0].Id);
        Assert.AreEqual(idPrestamo, sinLeer[0].Prestamo.Id);
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_LaBaseRechazaElGuardado_NoLanzaNoGuardaYRegistraElErrorConLaExcepcion()
    {
        GuardarPrestamoActivo(PedidoQueVencioAyer);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Notificacion>(_opciones);
        LoggerDePrueba<ServicioNotificacion> logger = new LoggerDePrueba<ServicioNotificacion>();
        ServicioNotificacion servicio = CrearServicioSobre(contextoQueFalla, logger);

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        Assert.AreEqual(0, LeerNoLeidasDe(DniDocente).Count);
        Assert.AreEqual(1, logger.Registros.Count);
        Assert.AreEqual(LogLevel.Error, logger.Registros[0].Nivel);
        Assert.IsInstanceOfType(logger.Registros[0].Excepcion, typeof(InvalidOperationException));
        Assert.IsInstanceOfType(logger.Registros[0].Excepcion!.InnerException, typeof(DbUpdateException));
    }

    [TestMethod]
    public void GenerarNotificacionesDeVencidos_LaBaseRechazaConDosVencidos_CortaEnLaPrimeraYRegistraUnaSolaVez()
    {
        GuardarPrestamoActivo(PedidoQueVencioAyer);
        GuardarPrestamoActivo(PedidoQueVencioAyer);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Notificacion>(_opciones);
        LoggerDePrueba<ServicioNotificacion> logger = new LoggerDePrueba<ServicioNotificacion>();
        ServicioNotificacion servicio = CrearServicioSobre(contextoQueFalla, logger);

        servicio.GenerarNotificacionesDeVencidos(DniDocente);

        Assert.AreEqual(1, logger.Registros.Count);
        Assert.AreEqual(0, LeerNoLeidasDe(DniDocente).Count);
    }

    private ServicioNotificacion CrearServicio()
    {
        return CrearServicioSobre(_contexto, NullLogger<ServicioNotificacion>.Instance);
    }

    private static ServicioNotificacion CrearServicioSobre(SqlContext contexto, ILogger<ServicioNotificacion> logger)
    {
        return new ServicioNotificacion(
            notificaciones: new RepositorioNotificacionSql(contexto),
            prestamos: new RepositorioPrestamoSql(contexto),
            reloj: new RelojFijo(Hoy),
            logger: logger);
    }

    private IReadOnlyList<Notificacion> LeerNoLeidasDe(string dni)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioNotificacionSql(contextoQueLee).ObtenerNoLeidasDeUsuario(dni);
    }

    private int GuardarNotificacion(Guid idPrestamo, TipoNotificacion tipo)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Prestamo prestamo = new RepositorioPrestamoSql(preparacion).ObtenerPrestamoPorId(idPrestamo)!;
        Notificacion notificacion = new Notificacion(prestamo, tipo, Hoy.AddDays(-1));
        new RepositorioNotificacionSql(preparacion).AgregarNotificacion(notificacion);

        return notificacion.Id;
    }

    private void MarcarLeidaConOtroContexto(int idNotificacion)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioNotificacionSql repositorio = new RepositorioNotificacionSql(preparacion);
        Notificacion notificacion = repositorio.ObtenerNotificacionPorId(idNotificacion)!;
        notificacion.MarcarComoLeida();
        repositorio.ActualizarNotificacion(notificacion);
    }

    // Un pedido de 5 dias habiles: el del 02/11 vence el 09/11 (vencido para hoy, 10/11) y el del 03/11 vence hoy.
    private Guid GuardarPrestamoActivo(DateTime fechaHoraPedido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Prestamo prestamo = CrearPrestamo(preparacion, fechaHoraPedido);
        prestamo.Activar(prestamo.TipoEquipamiento.Unidades.ToList());
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);

        return prestamo.Id;
    }

    private void GuardarPrestamoDevuelto(DateTime fechaHoraPedido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Prestamo prestamo = CrearPrestamo(preparacion, fechaHoraPedido);
        prestamo.Activar(prestamo.TipoEquipamiento.Unidades.ToList());
        prestamo.UnidadesAsignadas[0].Disponible();
        prestamo.MarcarDevuelto();
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
    }

    private static Prestamo CrearPrestamo(SqlContext preparacion, DateTime fechaHoraPedido)
    {
        Usuario solicitante = new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(DniDocente)!;
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        tipo.AgregarUnidad(new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null));

        return new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: fechaHoraPedido);
    }

    // Cada tipo de usuario se crea en el test: no se depende de datos sembrados en la base.
    private void GuardarUsuarios()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(CrearUsuario(DniDocente, TipoUsuario.Docente()));
    }

    private static Usuario CrearUsuario(string dni, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: "Renato", apellido: "Alves",
            correo: $"usuario{dni}@ejemplo.com", contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);

        return usuario;
    }
}
