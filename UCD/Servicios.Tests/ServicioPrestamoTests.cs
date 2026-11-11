using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioPrestamoTests
{
    private static readonly DateTime FechaFija = new DateTime(2026, 10, 5, 10, 0, 0);
    private static readonly DateTime FechaDeEvaluacion = new DateTime(2026, 10, 6, 9, 30, 0);

    private const string DniDocente = "12345672";
    private const string DniEstudiante = "45678905";
    private const string DniFuncionario = "23456783";
    private const string DniDocenteDesactivado = "34567894";
    private const string DniInexistente = "56789016";
    private const string DniSupervisor = "67890127";
    private const string DniSupervisorDesactivado = "78901238";
    private const string DniAdministrador = "89012349";
    private const string DniFuncionarioDesactivado = "90123450";

    private const string NombreTipoSinAprobacion = "Proyector Epson PowerLite";
    private const string NombreTipoSinUnidades = "Parlante JBL Flip";
    private const string NombreTipoConAprobacion = "Camara Canon EOS R50";
    private const string NombreTipoConAprobacionSinUnidades = "Tablet Samsung Galaxy Tab S9";
    private const string NombreTipoDiscontinuado = "Retroproyector 3M";
    private const string NombreTipoInexistente = "Microscopio Zeiss Primo";
    private const string NombreTipoDeOtroCatalogo = "Osciloscopio Rigol DS1054Z";

    private const int CantidadSolicitadaPorDefecto = 2;

    private const string MotivoDeRechazo = "El equipo esta reservado para un examen";

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;
    private int _idCatalogo;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void Solicitar_DatosNulos_LanzaArgumentNullException()
    {
        ServicioPrestamo servicio = CrearServicio();

        Assert.ThrowsException<ArgumentNullException>(
            () => servicio.Solicitar(null!, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteInexistente_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniInexistente));

        VerificarQueNoSeGuardoNiSePrestoNada(DniInexistente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteDesactivado_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocenteDesactivado));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocenteDesactivado);
    }

    [TestMethod]
    public void Solicitar_SolicitanteSinPermiso_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniFuncionario));
        Assert.AreEqual("El solicitante no tiene permiso para solicitar prestamos", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(DniFuncionario);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_QuedaActivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_PrestaLaCantidadSolicitada()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoEquipamiento tipo = ObtenerTipoGuardado(contextoQueLee, NombreTipoSinAprobacion);

        Assert.AreEqual(CantidadSolicitadaPorDefecto,
            tipo.Unidades.Count(unidad => unidad.Estado == EstadoUnidadFisica.Prestada));
        Assert.AreEqual(1, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_DatosValidos_LoGuarda()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Prestamo> guardados =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Activo, guardados[0].Estado);
    }

    [TestMethod]
    public void Solicitar_DatosValidos_UsaElMomentoDelReloj()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente).Single();

        Assert.AreEqual(FechaFija, guardado.FechaHoraPrestamo);
    }

    [TestMethod]
    public void Solicitar_DatosValidos_RegistraUnMovimientoDeSolicitud()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1);

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario docente = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniDocente)!;
        List<Movimiento> solicitudes = LeerMovimientos(contextoQueLee)
            .Where(movimiento => movimiento.Tipo == TipoMovimiento.Solicitud).ToList();

        Assert.AreEqual(1, solicitudes.Count);
        Assert.AreEqual(FechaFija, solicitudes[0].FechaHora);
        Assert.AreSame(docente, solicitudes[0].Solicitante);
        Assert.IsNull(solicitudes[0].Evaluador);
        Assert.IsNull(solicitudes[0].Unidad);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_RegistraUnMovimientoDePrestamoPorUnidad()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente).Single();
        List<Movimiento> movimientos = LeerMovimientos(contextoQueLee)
            .Where(movimiento => movimiento.Tipo == TipoMovimiento.Prestamo).ToList();

        Assert.AreEqual(CantidadSolicitadaPorDefecto, movimientos.Count);
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(FechaFija, movimiento.FechaHora);
            Assert.AreSame(guardado.Solicitante, movimiento.Solicitante);
            Assert.IsNull(movimiento.Evaluador);
        }
        CollectionAssert.AreEquivalent(
            guardado.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_RegistraUnaSolicitudYUnPrestamoPorUnidad()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Movimiento> movimientos = LeerMovimientos(contextoQueLee);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Solicitud, TipoMovimiento.Prestamo, TipoMovimiento.Prestamo },
            movimientos.Select(movimiento => movimiento.Tipo).ToList(),
            "La solicitud deberia registrarse antes que los prestamos");
        Assert.AreEqual(FechaFija, movimientos[0].FechaHora);
        Assert.IsNull(movimientos[0].Evaluador);
        Assert.IsNull(movimientos[0].Unidad);
        CollectionAssert.AreEqual(
            new List<string> { "EQP-0001", "EQP-0002" },
            movimientos.Skip(1).Select(movimiento => movimiento.Unidad!.IdInstitucional).ToList());
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_QuedaRechazadoConMotivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.MotivoRechazo),
            "La solicitud rechazada deberia tener un motivo");
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_LoGuardaSinPrestarNiRegistrarMovimientosDePrestamo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 4);

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Prestamo> guardados =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Rechazado, guardados[0].Estado);

        Assert.AreEqual(0, guardados[0].UnidadesAsignadas.Count);
        TipoEquipamiento tipo = ObtenerTipoGuardado(contextoQueLee, NombreTipoSinAprobacion);
        Assert.AreEqual(3, tipo.CantidadDisponibles());

        Assert.AreEqual(0, LeerMovimientos(contextoQueLee)
            .Count(movimiento => movimiento.Tipo == TipoMovimiento.Prestamo));
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_RegistraUnMovimientoDeRechazo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades, cantidadSolicitada: 1);

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario docente = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniDocente)!;
        IReadOnlyList<Movimiento> movimientos = LeerMovimientos(contextoQueLee);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Solicitud, TipoMovimiento.Rechazo },
            movimientos.Select(movimiento => movimiento.Tipo).ToList(),
            "Deberia registrarse la solicitud y despues el rechazo");
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(FechaFija, movimiento.FechaHora);
            Assert.AreSame(docente, movimiento.Solicitante);
            Assert.IsNull(movimiento.Evaluador);
            Assert.IsNull(movimiento.Unidad);
        }
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_QuedaSolicitadoSinUnidades()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Solicitado", resultado.Estado);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoEquipamiento tipo = ObtenerTipoGuardado(contextoQueLee, NombreTipoConAprobacion);
        Assert.AreEqual(3, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_LoGuardaSinRegistrarMovimientosDePrestamo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Prestamo> guardados =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente);
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Solicitado, guardados[0].Estado);

        Assert.AreEqual(0, LeerMovimientos(contextoQueLee)
            .Count(movimiento => movimiento.Tipo == TipoMovimiento.Prestamo));
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacion_NoNotifica()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoSinAprobacion);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);

        Assert.AreEqual("Activo", resultado.Estado);
        Assert.AreEqual(0, LeerNoLeidas(contextoQueLee, DniDocente).Count,
            "Solo los prestamos que requerian aprobacion llevan notificacion");
    }

    [TestMethod]
    public void Solicitar_TipoInexistente_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoInexistente);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoSinNombre_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: null!);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoDiscontinuado_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoDiscontinuado);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoDeOtroCatalogo_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        CargarOtroCatalogo();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoDeOtroCatalogo);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.AreEqual("El tipo de equipamiento no existe", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoDeOtroCatalogoConElIdDeEseCatalogo_QuedaActivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        int idOtroCatalogo = CargarOtroCatalogo();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoDeOtroCatalogo);
        datos.IdCatalogo = idOtroCatalogo;

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_CatalogoDadoDeBaja_LanzaExcepcionConMensaje()
    {
        ServicioPrestamo servicio = CrearServicio();
        RepositorioCatalogoSql catalogos = new RepositorioCatalogoSql(_contexto);
        Catalogo catalogo = catalogos.ObtenerCatalogoActivoPorId(_idCatalogo)!;
        catalogo.DarDeBaja();
        catalogos.ActualizarCatalogo(catalogo);
        SolicitudPrestamoDto datos = CrearSolicitud();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));

        Assert.AreEqual("El tipo de equipamiento no existe", excepcion.Message);
    }

    [TestMethod]
    public void Solicitar_SuperaUnidadesPorPrestamo_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 3);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniEstudiante));

        VerificarQueNoSeGuardoNiSePrestoNada(DniEstudiante);
    }

    [TestMethod]
    public void Solicitar_UnidadesActivasMasSolicitadasSuperanElMaximo_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(), DniEstudiante);
        SolicitudPrestamoDto segunda = CrearSolicitud(cantidadSolicitada: 1);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(segunda, DniEstudiante));
        Assert.AreEqual("La cantidad solicitada supera el maximo de unidades del solicitante", excepcion.Message);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Assert.AreEqual(1,
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniEstudiante).Count);
        TipoEquipamiento tipo = ObtenerTipoGuardado(contextoQueLee, NombreTipoSinAprobacion);
        Assert.IsTrue(tipo.Unidades.Single(unidad => unidad.IdInstitucional == "EQP-0003").EstaDisponible(),
            "La unidad EQP-0003 deberia seguir disponible");
    }

    [TestMethod]
    public void Solicitar_SolicitudesPendientesNoCuentanParaElMaximoDeUnidades()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion), DniEstudiante);
        SolicitudPrestamoDto segunda = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(segunda, DniEstudiante);

        Assert.AreEqual("Activo", resultado.Estado);
        Assert.AreEqual(CantidadSolicitadaPorDefecto, resultado.Unidades.Count);
    }

    [TestMethod]
    public void Solicitar_SuperaPrestamosConcurrentes_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();

        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1),
            DniEstudiante);
        SolicitudPrestamoDto tercera = CrearSolicitud(cantidadSolicitada: 1);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(tercera, DniEstudiante));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Assert.AreEqual(2,
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniEstudiante).Count);
        Assert.AreEqual(1, LeerMovimientos(contextoQueLee)
            .Count(movimiento => movimiento.Tipo == TipoMovimiento.Prestamo));
        TipoEquipamiento tipo = ObtenerTipoGuardado(contextoQueLee, NombreTipoSinAprobacion);
        Assert.AreEqual(2, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_SolicitudesRechazadasPrevias_NoCuentanParaElLimite()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);

        PrestamoDto resultado = servicio.Solicitar(CrearSolicitud(), DniEstudiante);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_CantidadCero_LanzaExcepcionConCausaDeDominio()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 0);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_DevuelveSusDatos()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado =
            new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(DniDocente).Single();
        Assert.AreEqual(guardado.Id, resultado.Id);
        Assert.AreEqual(NombreTipoSinAprobacion, resultado.TipoEquipamiento);
        Assert.AreEqual(CantidadSolicitadaPorDefecto, resultado.CantidadSolicitada);
        Assert.AreEqual(FechaFija, resultado.FechaHoraPrestamo);

        CollectionAssert.AreEquivalent(guardado.UnidadesAsignadas.Select(
            unidad => unidad.IdInstitucional).ToList(), resultado.Unidades);
    }

    [TestMethod]
    public void Solicitar_DatosValidos_DevuelveLaFechaEsperadaDeDevolucion()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 1);
        DateTime fechaEsperada = new DateTime(2026, 10, 12);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual(fechaEsperada, resultado.FechaEsperadaDevolucion);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void Solicitar_TipoVacio_LanzaExcepcionConMensaje(string nombreTipo)
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: nombreTipo);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.AreEqual("Debe indicar un tipo de equipamiento", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Solicitar_CatalogoNoIndicado_LanzaExcepcionConMensaje(int idCatalogo)
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos = CrearSolicitud();
        datos.IdCatalogo = idCatalogo;

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.AreEqual("Debe indicar un catalogo", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_QuedaActivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniSupervisor);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_AsignaLasUnidadesYGuardaElEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        servicio.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);
        TipoEquipamiento camara = ObtenerTipoGuardado(contextoQueLee, NombreTipoConAprobacion);
        Usuario supervisor = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniSupervisor)!;

        Assert.AreEqual(CantidadSolicitadaPorDefecto, guardado.UnidadesAsignadas.Count);
        Assert.AreEqual(1, camara.CantidadDisponibles());
        Assert.AreSame(supervisor, guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_RegistraUnMovimientoDePrestamoPorUnidadConElEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        ServicioPrestamo servicioDeLaEvaluacion =
            CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);
        Usuario supervisor = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniSupervisor)!;
        List<Movimiento> movimientos = LeerMovimientos(contextoQueLee)
            .Where(movimiento => movimiento.Tipo == TipoMovimiento.Prestamo).ToList();

        Assert.AreEqual(CantidadSolicitadaPorDefecto, movimientos.Count);
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(FechaDeEvaluacion, movimiento.FechaHora);
            Assert.AreSame(guardado.Solicitante, movimiento.Solicitante);
            Assert.AreSame(supervisor, movimiento.Evaluador);
        }
        CollectionAssert.AreEquivalent(
            guardado.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_RegistraUnMovimientoDeAprobacionPorUnidadConElEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        ServicioPrestamo servicioDeLaEvaluacion =
            CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario docente = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniDocente)!;
        Usuario supervisor = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniSupervisor)!;
        IReadOnlyList<Movimiento> movimientos = LeerMovimientos(contextoQueLee);
        List<Movimiento> aprobaciones = movimientos
            .Where(movimiento => movimiento.Tipo == TipoMovimiento.Aprobacion).ToList();

        CollectionAssert.AreEqual(
            new List<TipoMovimiento>
            {
                TipoMovimiento.Solicitud, TipoMovimiento.Aprobacion, TipoMovimiento.Aprobacion,
                TipoMovimiento.Prestamo, TipoMovimiento.Prestamo
            },
            movimientos.Select(movimiento => movimiento.Tipo).ToList(),
            "Las aprobaciones deberian registrarse despues de la solicitud y antes de los prestamos");
        foreach (Movimiento aprobacion in aprobaciones)
        {
            Assert.AreEqual(FechaDeEvaluacion, aprobacion.FechaHora);
            Assert.AreSame(docente, aprobacion.Solicitante);
            Assert.AreSame(supervisor, aprobacion.Evaluador);
        }
        CollectionAssert.AreEqual(
            new List<string> { "EQP-0101", "EQP-0102" },
            aprobaciones.Select(aprobacion => aprobacion.Unidad!.IdInstitucional).ToList());
    }

    [TestMethod]
    public void Aprobar_SinUnidadesSuficientes_QuedaRechazadoSinEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio, tipoEquipamiento: NombreTipoConAprobacionSinUnidades);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsNull(guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_SinUnidadesSuficientes_RegistraUnMovimientoDeRechazo()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos =
            CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacionSinUnidades, cantidadSolicitada: 1);
        Guid idPrestamo = servicio.Solicitar(datos, DniDocente).Id;
        ServicioPrestamo servicioDeLaEvaluacion =
            CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario docente = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniDocente)!;
        IReadOnlyList<Movimiento> movimientos = LeerMovimientos(contextoQueLee);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Solicitud, TipoMovimiento.Rechazo },
            movimientos.Select(movimiento => movimiento.Tipo).ToList(),
            "Deberia registrarse la solicitud y despues el rechazo del sistema");
        Movimiento rechazo =
            movimientos.Single(movimiento => movimiento.Tipo == TipoMovimiento.Rechazo);
        Assert.AreEqual(FechaDeEvaluacion, rechazo.FechaHora);
        Assert.AreSame(docente, rechazo.Solicitante);
        Assert.IsNull(rechazo.Evaluador, "El rechazo por falta de unidades lo decide el sistema, sin evaluador");
        Assert.IsNull(rechazo.Unidad);
    }

    [TestMethod]
    public void Aprobar_UnidadesInsuficientesPeroNoCero_QuedaRechazadoSinEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idDelAdministrador = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);
        servicio.Aprobar(idDelAdministrador, DniSupervisor);
        Guid idDelDocente = CrearSolicitudPendiente(servicio);

        PrestamoDto resultado = servicio.Aprobar(idDelDocente, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idDelDocente);
        TipoEquipamiento camara = ObtenerTipoGuardado(contextoQueLee, NombreTipoConAprobacion);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsNull(guardado.Evaluador);
        Assert.AreEqual(0, guardado.UnidadesAsignadas.Count);
        Assert.AreEqual(1, camara.CantidadDisponibles());
    }

    [TestMethod]
    public void Aprobar_PrestamoInexistente_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idInexistente = Guid.NewGuid();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idInexistente, DniSupervisor));

        Assert.AreEqual("El prestamo no existe", excepcion.Message);
        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    [DataRow(DniInexistente)]
    public void Aprobar_EvaluadorNoHabilitado_LanzaExcepcion(string dniEvaluador)
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, dniEvaluador));

        Assert.AreEqual("El usuario no tiene permiso para evaluar solicitudes", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(idPrestamo);
    }

    [TestMethod]
    public void Aprobar_SolicitanteDesactivadoDespuesDeSolicitar_LanzaExcepcionYSigueSolicitado()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        // Con el contexto del servicio: con otro, el servicio seguiria viendo al docente que ya tiene cargado.
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(_contexto);
        Usuario docente = usuarios.ObtenerUsuarioPorDni(DniDocente)!;
        docente.Desactivar();
        usuarios.ActualizarUsuario(docente);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, DniSupervisor));
        Assert.AreEqual("El solicitante esta desactivado", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(idPrestamo);
    }

    [TestMethod]
    public void Aprobar_AdministradorApruebaSuPropiaSolicitud_QuedaActivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniAdministrador);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual("Activo", resultado.Estado);
        Assert.AreSame(guardado.Solicitante, guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_PrestamoYaActivo_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudAprobada(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, DniSupervisor));

        Assert.AreEqual("El prestamo debe estar solicitado", excepcion.Message);
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Activo, guardado.Estado,
            "El prestamo deberia seguir activo");
        Assert.AreEqual(CantidadSolicitadaPorDefecto, guardado.UnidadesAsignadas.Count,
            "Deberia conservar sus unidades");
    }

    [TestMethod]
    public void Aprobar_LaBaseRechazaLosMovimientos_LanzaDbUpdateExceptionYElPrestamoQuedaActivo()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Movimiento>(_opciones);
        ServicioPrestamo servicioQueFalla = CrearServicioSobre(contextoQueFalla, new RelojFijo(FechaDeEvaluacion));

        Assert.ThrowsException<DbUpdateException>(() => servicioQueFalla.Aprobar(idPrestamo, DniSupervisor));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Activo, guardado.Estado,
            "El prestamo deberia guardarse antes que sus movimientos");
    }

    [TestMethod]
    public void Aprobar_SinUnidadesSuficientes_LaBaseRechazaLosMovimientos_LanzaDbUpdateExceptionYElPrestamoQuedaRechazado()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio, tipoEquipamiento: NombreTipoConAprobacionSinUnidades);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Movimiento>(_opciones);
        ServicioPrestamo servicioQueFalla = CrearServicioSobre(contextoQueFalla, new RelojFijo(FechaDeEvaluacion));

        Assert.ThrowsException<DbUpdateException>(() => servicioQueFalla.Aprobar(idPrestamo, DniSupervisor));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Rechazado, guardado.Estado,
            "El rechazo del sistema deberia guardarse antes que su movimiento");
        Assert.IsFalse(string.IsNullOrWhiteSpace(guardado.MotivoRechazo),
            "El rechazo del sistema deberia guardarse con su motivo");
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_NotificaLaAprobacionAlSolicitanteConLaFechaDeEvaluacion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        ServicioPrestamo servicioDeLaEvaluacion = CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Notificacion> notificaciones = LeerNoLeidas(contextoQueLee, DniDocente);

        Assert.AreEqual(1, notificaciones.Count);
        Assert.AreEqual(TipoNotificacion.SolicitudAprobada, notificaciones[0].Tipo);
        Assert.AreEqual(idPrestamo, notificaciones[0].Prestamo.Id);
        Assert.AreEqual(FechaDeEvaluacion, notificaciones[0].FechaHora);
    }

    [TestMethod]
    public void Aprobar_SinUnidadesSuficientes_NotificaElRechazoAlSolicitanteConLaFechaDeEvaluacion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio, tipoEquipamiento: NombreTipoConAprobacionSinUnidades);
        ServicioPrestamo servicioDeLaEvaluacion = CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Aprobar(idPrestamo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Notificacion> notificaciones = LeerNoLeidas(contextoQueLee, DniDocente);

        Assert.AreEqual(1, notificaciones.Count);
        Assert.AreEqual(TipoNotificacion.SolicitudRechazada, notificaciones[0].Tipo);
        Assert.AreEqual(idPrestamo, notificaciones[0].Prestamo.Id);
        Assert.AreEqual(FechaDeEvaluacion, notificaciones[0].FechaHora);
    }

    [TestMethod]
    public void Aprobar_LaBaseRechazaLaNotificacion_LanzaInvalidOperationExceptionYQuedanElPrestamoActivoYSusMovimientos()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Notificacion>(_opciones);
        ServicioPrestamo servicioQueFalla = CrearServicioSobre(contextoQueFalla, new RelojFijo(FechaDeEvaluacion));

        Assert.ThrowsException<InvalidOperationException>(() => servicioQueFalla.Aprobar(idPrestamo, DniSupervisor));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Activo, guardado.Estado);
        CollectionAssert.AreEqual(
            new List<TipoMovimiento>
            {
                TipoMovimiento.Solicitud, TipoMovimiento.Aprobacion, TipoMovimiento.Aprobacion,
                TipoMovimiento.Prestamo, TipoMovimiento.Prestamo
            },
            LeerMovimientos(contextoQueLee).Select(movimiento => movimiento.Tipo).ToList(),
            "Los movimientos deberian guardarse antes que la notificacion");
        Assert.AreEqual(0, LeerNoLeidas(contextoQueLee, DniDocente).Count);
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_NotificaElRechazoAlSolicitanteConLaFechaDeEvaluacion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        ServicioPrestamo servicioDeLaEvaluacion = CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Notificacion> notificaciones = LeerNoLeidas(contextoQueLee, DniDocente);

        Assert.AreEqual(1, notificaciones.Count);
        Assert.AreEqual(TipoNotificacion.SolicitudRechazada, notificaciones[0].Tipo);
        Assert.AreEqual(idPrestamo, notificaciones[0].Prestamo.Id);
        Assert.AreEqual(FechaDeEvaluacion, notificaciones[0].FechaHora);
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_ElMovimientoYLaNotificacionLlevanLaMismaFecha()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        ServicioPrestamo servicioDeLaEvaluacion = CrearServicioConReloj(new RelojQueAvanza(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Movimiento rechazo = LeerMovimientos(contextoQueLee)
            .Single(movimiento => movimiento.Tipo == TipoMovimiento.Rechazo);
        Notificacion notificacion = LeerNoLeidas(contextoQueLee, DniDocente).Single();

        Assert.AreEqual(rechazo.FechaHora, notificacion.FechaHora,
            "La fecha del rechazo deberia tomarse una sola vez");
    }

    [TestMethod]
    public void Rechazar_LaBaseRechazaLaNotificacion_LanzaInvalidOperationExceptionYQuedanElRechazoYSuMovimiento()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        using SqlContext contextoQueFalla = new ContextoQueFallaSiHayPendiente<Notificacion>(_opciones);
        ServicioPrestamo servicioQueFalla = CrearServicioSobre(contextoQueFalla, new RelojFijo(FechaDeEvaluacion));

        Assert.ThrowsException<InvalidOperationException>(
            () => servicioQueFalla.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Rechazado, guardado.Estado);
        Assert.AreEqual(MotivoDeRechazo, guardado.MotivoRechazo);
        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Solicitud, TipoMovimiento.Rechazo },
            LeerMovimientos(contextoQueLee).Select(movimiento => movimiento.Tipo).ToList(),
            "El movimiento deberia guardarse antes que la notificacion");
        Assert.AreEqual(0, LeerNoLeidas(contextoQueLee, DniDocente).Count);
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_QuedaRechazadoConMotivoYEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        PrestamoDto resultado = servicio.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);
        Usuario supervisor = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniSupervisor)!;

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.AreEqual(MotivoDeRechazo, resultado.MotivoRechazo);
        Assert.AreSame(supervisor, guardado.Evaluador);
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_RegistraUnMovimientoDeRechazoConElEvaluador()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitudPrestamoDto datos =
            CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1);
        Guid idPrestamo = servicio.Solicitar(datos, DniDocente).Id;
        ServicioPrestamo servicioDeLaEvaluacion =
            CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion));

        servicioDeLaEvaluacion.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario docente = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniDocente)!;
        Usuario supervisor = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(DniSupervisor)!;
        IReadOnlyList<Movimiento> movimientos = LeerMovimientos(contextoQueLee);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Solicitud, TipoMovimiento.Rechazo },
            movimientos.Select(movimiento => movimiento.Tipo).ToList(),
            "Deberia registrarse la solicitud y despues el rechazo del supervisor");
        Movimiento rechazo =
            movimientos.Single(movimiento => movimiento.Tipo == TipoMovimiento.Rechazo);
        Assert.AreEqual(FechaDeEvaluacion, rechazo.FechaHora);
        Assert.AreSame(docente, rechazo.Solicitante);
        Assert.AreSame(supervisor, rechazo.Evaluador, "El rechazo del supervisor deberia registrarlo como evaluador");
        Assert.IsNull(rechazo.Unidad);
    }

    [TestMethod]
    public void Rechazar_MotivoVacio_LanzaExcepcionConCausaDeDominio()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Rechazar(idPrestamo, "", DniSupervisor));
        Assert.AreEqual("El motivo de rechazo no puede ser vacio", excepcion.Message);
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        VerificarQueSigueSolicitadaSinPrestarNada(idPrestamo);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    [DataRow(DniInexistente)]
    public void Rechazar_EvaluadorNoHabilitado_LanzaExcepcion(string dniEvaluador)
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Rechazar(idPrestamo, MotivoDeRechazo, dniEvaluador));

        Assert.AreEqual("El usuario no tiene permiso para evaluar solicitudes", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(idPrestamo);
    }

    [TestMethod]
    public void Rechazar_PrestamoInexistente_LanzaExcepcion()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idInexistente = Guid.NewGuid();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Rechazar(idInexistente, MotivoDeRechazo, DniSupervisor));

        Assert.AreEqual("El prestamo no existe", excepcion.Message);
        VerificarQueNoSeGuardoNiSePrestoNada(DniDocente);
    }

    [TestMethod]
    public void ListarPrestamosActivos_VariosEstados_DevuelveLosActivos()
    {
        ServicioPrestamo servicio = CrearServicio();
        (_, _, Guid idPrimerActivo, Guid idSegundoActivo) = CrearPrestamosEnVariosEstados(servicio);

        List<PrestamoDto> resultado = servicio.ListarPrestamosActivos(DniFuncionario);

        CollectionAssert.AreEquivalent(
            new List<Guid> { idPrimerActivo, idSegundoActivo },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    [DataRow(DniSupervisor)]
    [DataRow(DniFuncionarioDesactivado)]
    public void ListarPrestamosActivos_ConsultanteNoHabilitado_LanzaExcepcionConMensaje(string dniConsultante)
    {
        ServicioPrestamo servicio = CrearServicio();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarPrestamosActivos(dniConsultante));

        Assert.AreEqual("El usuario no tiene permiso para consultar los prestamos activos", excepcion.Message);
    }

    [TestMethod]
    public void ListarPrestamosActivos_FechasEsperadasEnOtroOrdenQueLasSolicitudes_LosOrdenaPorFechaEsperada()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idQueVenceDespues = servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniAdministrador).Id;
        CambiarDuracionDelTipo(nombreTipo: NombreTipoSinAprobacion, diasHabiles: 1);
        Guid idQueVenceAntes = CrearServicioConReloj(new RelojFijo(FechaDeEvaluacion))
            .Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniAdministrador).Id;

        List<PrestamoDto> resultado = servicio.ListarPrestamosActivos(DniFuncionario);

        CollectionAssert.AreEqual(
            new List<Guid> { idQueVenceAntes, idQueVenceDespues },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    public void ListarPrestamosActivos_DespuesDeLaFechaEsperada_LoMarcaVencido()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniDocente);
        ServicioPrestamo servicioDelDiaSiguienteALaFechaEsperada =
            CrearServicioConReloj(new RelojFijo(new DateTime(2026, 10, 13, 9, 0, 0)));

        List<PrestamoDto> resultado =
            servicioDelDiaSiguienteALaFechaEsperada.ListarPrestamosActivos(DniFuncionario);

        Assert.IsTrue(resultado.Single().EstaVencido);
    }

    [TestMethod]
    public void ListarPrestamosActivos_AntesDeLaFechaEsperada_NoLoMarcaVencido()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniDocente);

        List<PrestamoDto> resultado = servicio.ListarPrestamosActivos(DniFuncionario);

        Assert.IsFalse(resultado.Single().EstaVencido);
    }

    [TestMethod]
    public void ListarPrestamos_VariosEstados_DevuelveTodos()
    {
        ServicioPrestamo servicio = CrearServicio();
        (Guid idSolicitado, Guid idRechazado, Guid idPrimerActivo, Guid idSegundoActivo) =
            CrearPrestamosEnVariosEstados(servicio);

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, servicio.RangoPorDefecto());

        CollectionAssert.AreEquivalent(
            new List<Guid> { idSolicitado, idRechazado, idPrimerActivo, idSegundoActivo },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    public void ListarPrestamos_ConsultanteNoHabilitado_LanzaExcepcionConMensaje(string dniConsultante)
    {
        ServicioPrestamo servicio = CrearServicio();
        FiltroDePrestamosDto filtro = servicio.RangoPorDefecto();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarPrestamos(dniConsultante, filtro));

        Assert.AreEqual("El usuario no tiene permiso para consultar prestamos", excepcion.Message);
    }

    [TestMethod]
    public void ListarPrestamos_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioPrestamo servicio = CrearServicio();

        Assert.ThrowsException<ArgumentNullException>(
            () => servicio.ListarPrestamos(DniSupervisor, null!));
    }

    [TestMethod]
    public void ListarPrestamos_RangoConHoras_DevuelveLosDeLosDiasCompletos()
    {
        ServicioPrestamo servicio = CrearServicio();
        SolicitarEnLaFecha(new DateTime(2026, 10, 1, 10, 0, 0));
        Guid idDelPrimerDia = SolicitarEnLaFecha(new DateTime(2026, 10, 2, 10, 0, 0));
        Guid idDelUltimoDia = SolicitarEnLaFecha(new DateTime(2026, 10, 5, 18, 0, 0));
        SolicitarEnLaFecha(new DateTime(2026, 10, 6, 9, 30, 0));
        FiltroDePrestamosDto filtro = new FiltroDePrestamosDto
        {
            Desde = new DateTime(2026, 10, 2, 12, 0, 0),
            Hasta = new DateTime(2026, 10, 5, 12, 0, 0)
        };

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, filtro);

        CollectionAssert.AreEquivalent(
            new List<Guid> { idDelPrimerDia, idDelUltimoDia },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    public void ListarPrestamos_HastaEnLaUltimaFechaPosible_DevuelveLosDelRangoSinFallar()
    {
        ServicioPrestamo servicio = CrearServicio();
        (Guid idSolicitado, Guid idRechazado, Guid idPrimerActivo, Guid idSegundoActivo) =
            CrearPrestamosEnVariosEstados(servicio);
        FiltroDePrestamosDto filtro = servicio.RangoPorDefecto();
        filtro.Hasta = DateTime.MaxValue.Date;

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, filtro);

        CollectionAssert.AreEquivalent(
            new List<Guid> { idSolicitado, idRechazado, idPrimerActivo, idSegundoActivo },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    public void ListarPrestamos_DesdePosteriorAHasta_LanzaExcepcionConMensaje()
    {
        ServicioPrestamo servicio = CrearServicio();
        FiltroDePrestamosDto filtro = new FiltroDePrestamosDto
        {
            Desde = new DateTime(2026, 10, 6),
            Hasta = new DateTime(2026, 10, 5)
        };

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarPrestamos(DniSupervisor, filtro));

        Assert.AreEqual("La fecha desde no puede ser posterior a la fecha hasta", excepcion.Message);
        Assert.AreEqual(nameof(FiltroDePrestamosDto.Desde), excepcion.Campo);
    }

    [TestMethod]
    public void ListarPrestamos_ConEstado_DevuelveSoloLosDeEseEstado()
    {
        ServicioPrestamo servicio = CrearServicio();
        (_, _, Guid idPrimerActivo, Guid idSegundoActivo) = CrearPrestamosEnVariosEstados(servicio);
        FiltroDePrestamosDto filtro = servicio.RangoPorDefecto();
        filtro.Estado = "Activo";

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, filtro);

        CollectionAssert.AreEquivalent(
            new List<Guid> { idPrimerActivo, idSegundoActivo },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void ListarPrestamos_EstadoVacio_DevuelveTodosLosEstados(string estado)
    {
        ServicioPrestamo servicio = CrearServicio();
        (Guid idSolicitado, Guid idRechazado, Guid idPrimerActivo, Guid idSegundoActivo) =
            CrearPrestamosEnVariosEstados(servicio);
        FiltroDePrestamosDto filtro = servicio.RangoPorDefecto();
        filtro.Estado = estado;

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, filtro);

        CollectionAssert.AreEquivalent(
            new List<Guid> { idSolicitado, idRechazado, idPrimerActivo, idSegundoActivo },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    [DataRow("Prestado")]
    [DataRow("2")]
    public void ListarPrestamos_EstadoDesconocido_LanzaExcepcionConMensaje(string estado)
    {
        ServicioPrestamo servicio = CrearServicio();
        FiltroDePrestamosDto filtro = servicio.RangoPorDefecto();
        filtro.Estado = estado;

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarPrestamos(DniSupervisor, filtro));

        Assert.AreEqual("El estado del prestamo no es valido", excepcion.Message);
    }

    [TestMethod]
    public void ListarPrestamos_VariasFechas_LosDevuelveDelMasNuevoAlMasViejo()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idDelPrimero = SolicitarEnLaFecha(new DateTime(2026, 10, 1, 10, 0, 0));
        Guid idDelSegundo = SolicitarEnLaFecha(new DateTime(2026, 10, 2, 10, 0, 0));
        Guid idDelTercero = SolicitarEnLaFecha(new DateTime(2026, 10, 5, 9, 0, 0));

        List<PrestamoDto> resultado = servicio.ListarPrestamos(DniSupervisor, servicio.RangoPorDefecto());

        CollectionAssert.AreEqual(
            new List<Guid> { idDelTercero, idDelSegundo, idDelPrimero },
            resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    public void RangoPorDefecto_RelojFijo_DevuelveLosUltimos30DiasConHoyYSinEstado()
    {
        ServicioPrestamo servicio = CrearServicio();

        FiltroDePrestamosDto rango = servicio.RangoPorDefecto();

        Assert.AreEqual(new DateTime(2026, 9, 6), rango.Desde);
        Assert.AreEqual(new DateTime(2026, 10, 5), rango.Hasta);
        Assert.IsNull(rango.Estado);
    }

    [TestMethod]
    public void ListarSolicitudesPendientes_VariosEstados_DevuelveSoloLasPendientesConSuSolicitante()
    {
        ServicioPrestamo servicio = CrearServicio();
        Guid idPendienteDocente = CrearSolicitudPendiente(servicio);
        Guid idPendienteAdministrador = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);

        CrearSolicitudAprobada(servicio);
        Guid idRechazada = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);
        servicio.Rechazar(idRechazada, MotivoDeRechazo, DniSupervisor);
        servicio.Solicitar(CrearSolicitud(), DniAdministrador);

        List<PrestamoDto> resultado = servicio.ListarSolicitudesPendientes(DniSupervisor);

        Assert.AreEqual(2, resultado.Count);
        CollectionAssert.AreEquivalent(
            new List<Guid> { idPendienteDocente, idPendienteAdministrador },
            resultado.Select(prestamo => prestamo.Id).ToList());

        Assert.AreEqual("Renato Alves", resultado.Single(
            prestamo => prestamo.Id == idPendienteDocente).Solicitante);
        Assert.AreEqual("Lucia Fernandez", resultado.Single(
            prestamo => prestamo.Id == idPendienteAdministrador).Solicitante);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    public void ListarSolicitudesPendientes_ConsultanteNoHabilitado_LanzaExcepcionConMensaje(string dniConsultante)
    {
        ServicioPrestamo servicio = CrearServicio();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarSolicitudesPendientes(dniConsultante));

        Assert.AreEqual("El usuario no tiene permiso para evaluar solicitudes", excepcion.Message);
    }

    [TestMethod]
    public void ListarMisSolicitudes_ConPrestamosDeVariosUsuarios_DevuelveSoloLosSuyos()
    {
        ServicioPrestamo servicio = CrearServicio();
        PrestamoDto activoDelDocente =
            servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniDocente);
        PrestamoDto pendienteDelDocente =
            servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1), DniDocente);
        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniEstudiante);

        List<PrestamoDto> resultado = servicio.ListarMisSolicitudes(DniDocente);

        Assert.AreEqual(2, resultado.Count);
        CollectionAssert.AreEquivalent(new List<Guid>
            { activoDelDocente.Id, pendienteDelDocente.Id }, resultado.Select(prestamo => prestamo.Id).ToList());
    }

    [TestMethod]
    public void ListarMisSolicitudes_SinPrestamos_DevuelveListaVacia()
    {
        ServicioPrestamo servicio = CrearServicio();
        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniDocente);

        List<PrestamoDto> resultado = servicio.ListarMisSolicitudes(DniAdministrador);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniDocenteDesactivado)]
    public void ListarMisSolicitudes_ConsultanteNoHabilitado_LanzaExcepcionConMensaje(string dniConsultante)
    {
        ServicioPrestamo servicio = CrearServicio();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.ListarMisSolicitudes(dniConsultante));

        Assert.AreEqual("El usuario no tiene permiso para consultar sus solicitudes", excepcion.Message);
    }

    private ServicioPrestamo CrearServicio()
    {
        CargarCatalogo();
        CargarUsuarios();

        return CrearServicioConReloj(new RelojFijo(FechaFija));
    }

    private ServicioPrestamo CrearServicioConReloj(TimeProvider reloj)
    {
        return CrearServicioSobre(_contexto, reloj);
    }

    // Todos los repositorios sobre el mismo contexto: si se mezclan, EF intenta insertar lo que ya existe.
    private static ServicioPrestamo CrearServicioSobre(SqlContext contexto, TimeProvider reloj)
    {
        ServicioNotificacion servicioNotificacion = new ServicioNotificacion(
            notificaciones: new RepositorioNotificacionSql(contexto),
            prestamos: new RepositorioPrestamoSql(contexto),
            reloj: reloj,
            logger: NullLogger<ServicioNotificacion>.Instance);

        return new ServicioPrestamo(
            prestamos: new RepositorioPrestamoSql(contexto),
            usuarios: new RepositorioUsuarioSql(contexto),
            catalogo: new RepositorioCatalogoSql(contexto),
            movimientos: new RepositorioMovimientoSql(contexto),
            servicioNotificacion: servicioNotificacion,
            reloj: reloj);
    }

    private SolicitudPrestamoDto CrearSolicitud(
        string tipoEquipamiento = NombreTipoSinAprobacion,
        int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        return new SolicitudPrestamoDto
        {
            IdCatalogo = _idCatalogo,
            TipoEquipamiento = tipoEquipamiento,
            CantidadSolicitada = cantidadSolicitada
        };
    }

    private Guid CrearSolicitudPendiente(
        ServicioPrestamo servicio,
        string tipoEquipamiento = NombreTipoConAprobacion,
        string dniSolicitante = DniDocente)
    {
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: tipoEquipamiento);
        PrestamoDto pendiente = servicio.Solicitar(datos, dniSolicitante);

        return pendiente.Id;
    }

    private (Guid idSolicitado, Guid idRechazado, Guid idPrimerActivo, Guid idSegundoActivo)
        CrearPrestamosEnVariosEstados(ServicioPrestamo servicio)
    {
        Guid idSolicitado =
            servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion), DniAdministrador).Id;
        Guid idRechazado =
            servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniAdministrador).Id;
        Guid idPrimerActivo =
            servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniAdministrador).Id;
        Guid idSegundoActivo =
            servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniAdministrador).Id;

        return (idSolicitado, idRechazado, idPrimerActivo, idSegundoActivo);
    }

    private Guid SolicitarEnLaFecha(DateTime fechaHora)
    {
        ServicioPrestamo servicioDeEsaFecha = CrearServicioConReloj(new RelojFijo(fechaHora));
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        return servicioDeEsaFecha.Solicitar(datos, DniAdministrador).Id;
    }

    private Guid CrearSolicitudAprobada(ServicioPrestamo servicio)
    {
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        servicio.Aprobar(idPrestamo, DniSupervisor);

        return idPrestamo;
    }

    private static Prestamo ObtenerPrestamoGuardado(SqlContext contextoQueLee, Guid idPrestamo)
    {
        return new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamoPorId(idPrestamo)!;
    }

    private TipoEquipamiento ObtenerTipoGuardado(SqlContext contextoQueLee, string nombreTipo)
    {
        return new RepositorioCatalogoSql(contextoQueLee).ObtenerTipoPorNombre(_idCatalogo, nombreTipo)!;
    }

    private static IReadOnlyList<Notificacion> LeerNoLeidas(SqlContext contextoQueLee, string dni)
    {
        return new RepositorioNotificacionSql(contextoQueLee).ObtenerNoLeidasDeUsuario(dni);
    }

    // RepositorioMovimientoSql no tiene una consulta de todos los movimientos: se leen de la base,
    // en el orden en que se registraron.
    private static IReadOnlyList<Movimiento> LeerMovimientos(SqlContext contextoQueLee)
    {
        return contextoQueLee.Movimientos
            .Include(movimiento => movimiento.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(movimiento => movimiento.Evaluador)
            .Include(movimiento => movimiento.Unidad)
            .OrderBy(movimiento => movimiento.Id)
            .ToList();
    }

    private void CargarCatalogo()
    {
        Catalogo catalogo = new Catalogo("Catalogo general");

        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoSinAprobacion,
            requiereAprobacion: false,
            cantidadUnidades: 3));
        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoSinUnidades,
            requiereAprobacion: false,
            cantidadUnidades: 0));
        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoConAprobacion,
            requiereAprobacion: true,
            cantidadUnidades: 3,
            primerNumero: 101));
        catalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoConAprobacionSinUnidades,
            requiereAprobacion: true,
            cantidadUnidades: 0));

        TipoEquipamiento tipoEquipamientoDiscontinuado = CrearTipoEquipamiento(
            nombre: NombreTipoDiscontinuado,
            requiereAprobacion: false,
            cantidadUnidades: 3,
            primerNumero: 201);

        tipoEquipamientoDiscontinuado.Discontinuar();
        catalogo.AgregarTipo(tipoEquipamientoDiscontinuado);

        new RepositorioCatalogoSql(_contexto).AgregarCatalogo(catalogo);
        _idCatalogo = catalogo.Id;
    }

    private int CargarOtroCatalogo()
    {
        Catalogo otroCatalogo = new Catalogo("Catalogo de laboratorio");

        otroCatalogo.AgregarTipo(CrearTipoEquipamiento(
            nombre: NombreTipoDeOtroCatalogo,
            requiereAprobacion: false,
            cantidadUnidades: 3,
            primerNumero: 301));

        new RepositorioCatalogoSql(_contexto).AgregarCatalogo(otroCatalogo);

        return otroCatalogo.Id;
    }

    // Con el contexto del servicio: la proxima solicitud usa el tipo que ese contexto ya tiene cargado.
    private void CambiarDuracionDelTipo(string nombreTipo, int diasHabiles)
    {
        RepositorioCatalogoSql catalogos = new RepositorioCatalogoSql(_contexto);
        Catalogo catalogo = catalogos.ObtenerCatalogoActivoPorId(_idCatalogo)!;
        TipoEquipamiento tipo = catalogo.BuscarTipoPorNombre(nombreTipo)!;

        tipo.Modificar(tipo.Nombre, tipo.Descripcion, diasHabiles, tipo.RequiereAprobacion, tipo.Categoria);
        catalogos.ActualizarCatalogo(catalogo);
    }

    private static TipoEquipamiento CrearTipoEquipamiento(string nombre, bool requiereAprobacion, int cantidadUnidades,
        int primerNumero = 1)
    {
        TipoEquipamiento tipoEquipamiento = new TipoEquipamiento(
            nombre: nombre,
            descripcion: "Equipo audiovisual para pruebas de solicitud",
            duracionMaxPrestamo: 5,
            requiereAprobacion: requiereAprobacion,
            categoria: Categoria.Audiovisual);

        for (int i = 0; i < cantidadUnidades; i++)
        {
            int numero = primerNumero + i;

            tipoEquipamiento.AgregarUnidad(new UnidadFisica(
                idInstitucional: $"EQP-{numero:D4}",
                ubiFisica: "Deposito A",
                numSerie: null));
        }

        return tipoEquipamiento;
    }

    // Un objeto por tipo de usuario, como en la base, donde el nombre del tipo es unico.
    private void CargarUsuarios()
    {
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(_contexto);
        TipoUsuario docente = TipoUsuario.Docente();
        TipoUsuario funcionario = TipoUsuario.FuncionarioCE();
        TipoUsuario supervisor = TipoUsuario.Supervisor();

        usuarios.AgregarUsuario(CrearUsuario(dni: DniDocente, tipoUsuario: docente));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniEstudiante, tipoUsuario: TipoUsuario.Estudiante()));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniFuncionario, tipoUsuario: funcionario));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniSupervisor, tipoUsuario: supervisor,
            nombre: "Sofia", apellido: "Martinez"));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniAdministrador, tipoUsuario: TipoUsuario.Administrador(),
            nombre: "Lucia", apellido: "Fernandez"));

        Usuario desactivado = CrearUsuario(dni: DniDocenteDesactivado, tipoUsuario: docente);
        desactivado.Desactivar();
        usuarios.AgregarUsuario(desactivado);

        Usuario supervisorDesactivado = CrearUsuario(dni: DniSupervisorDesactivado, tipoUsuario: supervisor,
            nombre: "Diego", apellido: "Rodriguez");
        supervisorDesactivado.Desactivar();
        usuarios.AgregarUsuario(supervisorDesactivado);

        Usuario funcionarioDesactivado = CrearUsuario(dni: DniFuncionarioDesactivado, tipoUsuario: funcionario);
        funcionarioDesactivado.Desactivar();
        usuarios.AgregarUsuario(funcionarioDesactivado);
    }

    private static Usuario CrearUsuario(
        string dni,
        TipoUsuario tipoUsuario,
        string nombre = "Renato",
        string apellido = "Alves")
    {
        Usuario usuario = new Usuario(
            dni: dni,
            nombre: nombre,
            apellido: apellido,
            correo: $"usuario{dni}@ejemplo.com",
            contrasena: "Vivalaort2026");

        usuario.AsignarTipo(tipoUsuario);

        return usuario;
    }

    private void VerificarQueNoSeGuardoNiSePrestoNada(string dni)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        Assert.AreEqual(0, new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamosDeSolicitante(dni: dni).Count,
            "No deberia haberse guardado ningun prestamo");
        Assert.AreEqual(0, LeerMovimientos(contextoQueLee).Count,
            "No deberia haberse registrado ningun movimiento");

        Catalogo catalogo = new RepositorioCatalogoSql(contextoQueLee).ObtenerCatalogoActivoPorId(_idCatalogo)!;
        Assert.IsTrue(catalogo.Tipos.SelectMany(tipo => tipo.Unidades).All(unidad => unidad.EstaDisponible()),
            "Ninguna unidad deberia estar prestada");
    }

    private void VerificarQueSigueSolicitadaSinPrestarNada(Guid idPrestamo)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo guardado = ObtenerPrestamoGuardado(contextoQueLee, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Solicitado, guardado.Estado,
            "El prestamo deberia seguir solicitado");
        Assert.AreEqual(0, guardado.UnidadesAsignadas.Count,
            "No deberia haberse asignado ninguna unidad");
        Assert.AreEqual(0, LeerMovimientos(contextoQueLee)
                .Count(movimiento => movimiento.Tipo != TipoMovimiento.Solicitud),
            "No deberia haberse registrado ningun movimiento ademas de la solicitud");
    }
}