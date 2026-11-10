using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioNotificacionTests
{
    private static readonly DateTime Hoy = new DateTime(2026, 11, 10, 9, 0, 0);
    private static readonly DateTime Ayer = Hoy.AddDays(-1);
    private static readonly DateTime FechaDeEvaluacion = new DateTime(2026, 11, 4, 11, 30, 0);
    private static readonly DateTime PedidoQueVencioAyer = new DateTime(2026, 11, 2, 10, 0, 0);
    private static readonly DateTime PedidoQueVenceHoy = new DateTime(2026, 11, 3, 10, 0, 0);

    private const string DniDocente = "12345672";
    private const string DniEstudiante = "45678905";

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

    [TestMethod]
    public void ListarNoLeidas_SolicitudAprobada_DevuelveElDtoConIdFechaTipoYTexto()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idNotificacion = GuardarNotificacion(idPrestamo, TipoNotificacion.SolicitudAprobada);
        ServicioNotificacion servicio = CrearServicio();

        List<NotificacionDto> notificaciones = servicio.ListarNoLeidas(DniDocente);

        Assert.AreEqual(1, notificaciones.Count);
        Assert.AreEqual(idNotificacion, notificaciones[0].Id);
        Assert.AreEqual(Ayer, notificaciones[0].FechaHora);
        Assert.AreEqual("SolicitudAprobada", notificaciones[0].Tipo);
        Assert.AreEqual("Tu solicitud de Proyector Epson del 02/11/2026 fue aprobada.", notificaciones[0].Texto);
    }

    [TestMethod]
    public void ListarNoLeidas_SolicitudRechazada_ElTextoLlevaElMotivo()
    {
        Guid idPrestamo = GuardarPrestamoRechazadoPorFaltaDeUnidades(PedidoQueVencioAyer);
        GuardarNotificacion(idPrestamo, TipoNotificacion.SolicitudRechazada);
        ServicioNotificacion servicio = CrearServicio();

        NotificacionDto notificacion = servicio.ListarNoLeidas(DniDocente).Single();

        Assert.AreEqual("Tu solicitud de Proyector Epson del 02/11/2026 fue rechazada. " +
                        "Motivo: No hay unidades suficientes para el tipo de equipamiento", notificacion.Texto);
    }

    [TestMethod]
    public void ListarNoLeidas_PrestamoVencido_ElTextoDiceEnPasadoCuandoHabiaQueDevolverlo()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        ServicioNotificacion servicio = CrearServicio();

        NotificacionDto notificacion = servicio.ListarNoLeidas(DniDocente).Single();

        Assert.AreEqual("Tu préstamo de Proyector Epson venció: tenías que devolverlo el 09/11/2026.",
            notificacion.Texto);
    }

    [TestMethod]
    public void MarcarComoLeida_IdInexistente_LanzaServicioException()
    {
        ServicioNotificacion servicio = CrearServicio();

        ServicioException excepcion =
            Assert.ThrowsException<ServicioException>(() => servicio.MarcarComoLeida(999, DniDocente));

        Assert.AreEqual("La notificacion no es del usuario", excepcion.Message);
    }

    [TestMethod]
    public void MarcarComoLeida_NotificacionDelUsuario_QuedaLeidaYDejaDeListarse()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idNotificacion = GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        ServicioNotificacion servicio = CrearServicio();

        servicio.MarcarComoLeida(idNotificacion, DniDocente);

        Assert.IsTrue(LeerNotificacion(idNotificacion).EstaLeida);
        Assert.AreEqual(0, LeerNoLeidasDe(DniDocente).Count);
    }

    [TestMethod]
    public void MarcarComoLeida_NotificacionDeOtroUsuario_LanzaServicioExceptionYNoLaMarca()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idNotificacion = GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        ServicioNotificacion servicio = CrearServicio();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.MarcarComoLeida(idNotificacion, DniEstudiante));

        Assert.AreEqual("La notificacion no es del usuario", excepcion.Message);
        Assert.IsFalse(LeerNotificacion(idNotificacion).EstaLeida);
    }

    [TestMethod]
    public void MarcarComoLeida_YaLeida_NoLanzaYSigueLeida()
    {
        Guid idPrestamo = GuardarPrestamoActivo(PedidoQueVencioAyer);
        int idNotificacion = GuardarNotificacion(idPrestamo, TipoNotificacion.PrestamoVencido);
        ServicioNotificacion servicio = CrearServicio();
        servicio.MarcarComoLeida(idNotificacion, DniDocente);

        servicio.MarcarComoLeida(idNotificacion, DniDocente);

        Assert.IsTrue(LeerNotificacion(idNotificacion).EstaLeida);
    }

    [TestMethod]
    public void RegistrarResultadoDeEvaluacion_PrestamoActivo_GuardaSolicitudAprobadaConLaFechaRecibida()
    {
        Prestamo evaluado = CargarEnElContextoDelServicio(GuardarPrestamoActivo(PedidoQueVencioAyer));
        ServicioNotificacion servicio = CrearServicio();

        servicio.RegistrarResultadoDeEvaluacion(evaluado, FechaDeEvaluacion);

        IReadOnlyList<Notificacion> creadas = LeerNoLeidasDe(DniDocente);
        Assert.AreEqual(1, creadas.Count);
        Assert.AreEqual(evaluado.Id, creadas[0].Prestamo.Id);
        Assert.AreEqual(TipoNotificacion.SolicitudAprobada, creadas[0].Tipo);
        Assert.AreEqual(FechaDeEvaluacion, creadas[0].FechaHora);
    }

    // Como en ServicioPrestamo, el prestamo evaluado esta en el contexto compartido con el servicio.
    private Prestamo CargarEnElContextoDelServicio(Guid idPrestamo)
    {
        return new RepositorioPrestamoSql(_contexto).ObtenerPrestamoPorId(idPrestamo)!;
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

    private Notificacion LeerNotificacion(int idNotificacion)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioNotificacionSql(contextoQueLee).ObtenerNotificacionPorId(idNotificacion)!;
    }

    private int GuardarNotificacion(Guid idPrestamo, TipoNotificacion tipo)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Prestamo prestamo = new RepositorioPrestamoSql(preparacion).ObtenerPrestamoPorId(idPrestamo)!;
        Notificacion notificacion = new Notificacion(prestamo, tipo, Ayer);
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

    private Guid GuardarPrestamoRechazadoPorFaltaDeUnidades(DateTime fechaHoraPedido)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Prestamo prestamo = CrearPrestamo(preparacion, fechaHoraPedido);
        prestamo.RechazarPorFaltaDeUnidades();
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);

        return prestamo.Id;
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
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        usuarios.AgregarUsuario(CrearUsuario(DniDocente, TipoUsuario.Docente()));
        usuarios.AgregarUsuario(CrearUsuario(DniEstudiante, TipoUsuario.Estudiante()));
    }

    private static Usuario CrearUsuario(string dni, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: "Renato", apellido: "Alves",
            correo: $"usuario{dni}@ejemplo.com", contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);

        return usuario;
    }
}
