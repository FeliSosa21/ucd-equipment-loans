using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioPrestamoTests
{
    private static readonly DateTime FechaFija = new DateTime(2026, 10, 5, 10, 0, 0);
    private static readonly DateTime FechaDeRealizacion = new DateTime(2026, 10, 6, 9, 30, 0);

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

    private const int CantidadSolicitadaPorDefecto = 2;

    private const string MotivoDeRechazo = "El equipo esta reservado para un examen";

    [TestMethod]
    public void Solicitar_DatosNulos_LanzaArgumentNullException()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);

        Assert.ThrowsException<ArgumentNullException>(
            () => servicio.Solicitar(null!, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniInexistente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniInexistente);
    }

    [TestMethod]
    public void Solicitar_SolicitanteDesactivado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocenteDesactivado));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocenteDesactivado);
    }

    [TestMethod]
    public void Solicitar_SolicitanteSinPermiso_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniFuncionario));
        Assert.AreEqual("El solicitante no tiene permiso para solicitar prestamos", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniFuncionario);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_QuedaActivo()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_PrestaLaCantidadSolicitada()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;

        Assert.AreEqual(CantidadSolicitadaPorDefecto,
            tipo.Unidades.Count(unidad => unidad.Estado == EstadoUnidadFisica.Prestada));
        Assert.AreEqual(1, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_DatosValidos_LoGuarda()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados = new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Activo, guardados[0].Estado);
    }

    [TestMethod]
    public void Solicitar_DatosValidos_UsaElMomentoDelReloj()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        Prestamo guardado =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente).Single();

        Assert.AreEqual(FechaFija, guardado.FechaHoraPrestamo);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_RegistraUnMovimientoDePrestamoPorUnidad()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        servicio.Solicitar(datos, DniDocente);

        Prestamo guardado =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente).Single();
        IReadOnlyList<Movimiento> movimientos = new RepositorioMovimiento(memoria).ObtenerTodos();

        Assert.AreEqual(CantidadSolicitadaPorDefecto, movimientos.Count);
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(TipoMovimiento.Prestamo, movimiento.Tipo);
            Assert.AreEqual(FechaFija, movimiento.FechaHora);
            Assert.AreSame(guardado.Solicitante, movimiento.Solicitante);
            Assert.IsNull(movimiento.Evaluador);
        }
        CollectionAssert.AreEquivalent(
            guardado.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_QuedaRechazadoConMotivo()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.MotivoRechazo),
            "La solicitud rechazada deberia tener un motivo");
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionSinUnidadesSuficientes_LoGuardaSinPrestarNiRegistrarMovimientos()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 4);

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados =
            new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);

        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Rechazado, guardados[0].Estado);

        Assert.AreEqual(0, guardados[0].UnidadesAsignadas.Count);
        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;
        Assert.AreEqual(3, tipo.CantidadDisponibles());

        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_QuedaSolicitadoSinUnidades()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Assert.AreEqual("Solicitado", resultado.Estado);

        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoConAprobacion)!;
        Assert.AreEqual(3, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_TipoConAprobacion_LoGuardaSinRegistrarMovimientos()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion);

        servicio.Solicitar(datos, DniDocente);

        IReadOnlyList<Prestamo> guardados = new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente);
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(EstadoPrestamo.Solicitado, guardados[0].Estado);

        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void Solicitar_TipoInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoInexistente);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoSinNombre_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: null!);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoDiscontinuado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: NombreTipoDiscontinuado);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniDocente));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_SuperaUnidadesPorPrestamo_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 3);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(datos, DniEstudiante));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniEstudiante);
    }

    [TestMethod]
    public void Solicitar_SuperaPrestamosConcurrentes_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);

        servicio.Solicitar(CrearSolicitud(cantidadSolicitada: 1), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1),
            DniEstudiante);
        SolicitudPrestamoDto tercera = CrearSolicitud(cantidadSolicitada: 1);

        Assert.ThrowsException<ServicioException>(() => servicio.Solicitar(tercera, DniEstudiante));

        Assert.AreEqual(2, new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniEstudiante).Count);
        Assert.AreEqual(1, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
        TipoEquipamiento tipo = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoSinAprobacion)!;
        Assert.AreEqual(2, tipo.CantidadDisponibles());
    }

    [TestMethod]
    public void Solicitar_SolicitudesRechazadasPrevias_NoCuentanParaElLimite()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);
        servicio.Solicitar(CrearSolicitud(tipoEquipamiento: NombreTipoSinUnidades), DniEstudiante);

        PrestamoDto resultado = servicio.Solicitar(CrearSolicitud(), DniEstudiante);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Solicitar_CantidadCero_LanzaExcepcionConCausaDeDominio()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(cantidadSolicitada: 0);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Solicitar_TipoSinAprobacionConUnidades_DevuelveSusDatos()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud();

        PrestamoDto resultado = servicio.Solicitar(datos, DniDocente);

        Prestamo guardado = new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(DniDocente).Single();
        Assert.AreEqual(guardado.Id, resultado.Id);
        Assert.AreEqual(NombreTipoSinAprobacion, resultado.TipoEquipamiento);
        Assert.AreEqual(CantidadSolicitadaPorDefecto, resultado.CantidadSolicitada);
        Assert.AreEqual(FechaFija, resultado.FechaHoraPrestamo);

        CollectionAssert.AreEquivalent(guardado.UnidadesAsignadas.Select(
            unidad => unidad.IdInstitucional).ToList(), resultado.Unidades);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void Solicitar_TipoVacio_LanzaExcepcionConMensaje(string nombreTipo)
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: nombreTipo);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Solicitar(datos, DniDocente));
        Assert.AreEqual("Debe indicar un tipo de equipamiento", excepcion.Message);

        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_QuedaAprobado()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniSupervisor);

        Assert.AreEqual("Aprobado", resultado.Estado);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_AsignaLasUnidadesYGuardaElEvaluador()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        servicio.Aprobar(idPrestamo, DniSupervisor);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);
        TipoEquipamiento camara = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoConAprobacion)!;
        Usuario supervisor = new RepositorioUsuario(memoria).ObtenerUsuarioPorDni(DniSupervisor)!;

        Assert.AreEqual(CantidadSolicitadaPorDefecto, guardado.UnidadesAsignadas.Count);
        Assert.AreEqual(1, camara.CantidadDisponibles());
        Assert.AreSame(supervisor, guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_SolicitudPendienteConUnidades_NoRegistraMovimientos()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        servicio.Aprobar(idPrestamo, DniSupervisor);

        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count);
    }

    [TestMethod]
    public void Aprobar_SinUnidadesSuficientes_QuedaRechazadoSinEvaluador()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio, tipoEquipamiento: NombreTipoConAprobacionSinUnidades);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniSupervisor);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.IsNull(guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_PrestamoInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idInexistente = Guid.NewGuid();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idInexistente, DniSupervisor));

        Assert.AreEqual("El prestamo no existe", excepcion.Message);
        VerificarQueNoSeGuardoNiSePrestoNada(memoria, DniDocente);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    [DataRow(DniInexistente)]
    public void Aprobar_EvaluadorNoHabilitado_LanzaExcepcion(string dniEvaluador)
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, dniEvaluador));

        Assert.AreEqual("El usuario no tiene permiso para evaluar solicitudes", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(memoria, idPrestamo);
    }

    [TestMethod]
    public void Aprobar_SolicitanteDesactivadoDespuesDeSolicitar_LanzaExcepcionYSigueSolicitado()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        Usuario docente = new RepositorioUsuario(memoria).ObtenerUsuarioPorDni(DniDocente)!;
        docente.Desactivar();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, DniSupervisor));
        Assert.AreEqual("El solicitante esta desactivado", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(memoria, idPrestamo);
    }

    [TestMethod]
    public void Aprobar_AdministradorApruebaSuPropiaSolicitud_QuedaAprobado()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);

        PrestamoDto resultado = servicio.Aprobar(idPrestamo, DniAdministrador);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);

        Assert.AreEqual("Aprobado", resultado.Estado);
        Assert.AreSame(guardado.Solicitante, guardado.Evaluador);
    }

    [TestMethod]
    public void Aprobar_PrestamoYaAprobado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudAprobada(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Aprobar(idPrestamo, DniSupervisor));

        Assert.AreEqual("El prestamo debe estar solicitado", excepcion.Message);
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Aprobado, guardado.Estado,
            "El prestamo deberia seguir aprobado");
        Assert.AreEqual(CantidadSolicitadaPorDefecto, guardado.UnidadesAsignadas.Count,
            "Deberia conservar sus unidades");
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_QuedaRechazadoConMotivoYEvaluador()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        PrestamoDto resultado = servicio.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);
        Usuario supervisor = new RepositorioUsuario(memoria).ObtenerUsuarioPorDni(DniSupervisor)!;

        Assert.AreEqual("Rechazado", resultado.Estado);
        Assert.AreEqual(MotivoDeRechazo, resultado.MotivoRechazo);
        Assert.AreSame(supervisor, guardado.Evaluador);
    }

    [TestMethod]
    public void Rechazar_MotivoVacio_LanzaExcepcionConCausaDeDominio()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Rechazar(idPrestamo, "", DniSupervisor));
        Assert.AreEqual("El motivo de rechazo no puede ser vacio", excepcion.Message);
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        VerificarQueSigueSolicitadaSinPrestarNada(memoria, idPrestamo);
    }

    [TestMethod]
    [DataRow(DniFuncionario)]
    [DataRow(DniSupervisorDesactivado)]
    [DataRow(DniInexistente)]
    public void Rechazar_EvaluadorNoHabilitado_LanzaExcepcion(string dniEvaluador)
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Rechazar(idPrestamo, MotivoDeRechazo, dniEvaluador));

        Assert.AreEqual("El usuario no tiene permiso para evaluar solicitudes", excepcion.Message);

        VerificarQueSigueSolicitadaSinPrestarNada(memoria, idPrestamo);
    }

    [TestMethod]
    public void Rechazar_PrestamoAprobado_LiberaLasUnidades()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudAprobada(servicio);

        List<UnidadFisica> unidades = ObtenerPrestamoGuardado(memoria, idPrestamo).UnidadesAsignadas.ToList();

        servicio.Rechazar(idPrestamo, MotivoDeRechazo, DniSupervisor);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);
        TipoEquipamiento camara = new RepositorioCatalogo(memoria).ObtenerTipoPorNombre(NombreTipoConAprobacion)!;

        Assert.AreEqual(CantidadSolicitadaPorDefecto, unidades.Count,
            "Deberia haber copiado las unidades asignadas");
        Assert.IsTrue(unidades.All(unidad => unidad.EstaDisponible()),
            "Las unidades deberian estar disponibles");
        Assert.AreEqual(3, camara.CantidadDisponibles());
        Assert.AreEqual(0, guardado.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Realizar_PrestamoAprobado_QuedaActivo()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudAprobada(servicio);

        PrestamoDto resultado = servicio.Realizar(idPrestamo, DniFuncionario);

        Assert.AreEqual("Activo", resultado.Estado);
    }

    [TestMethod]
    public void Realizar_PrestamoAprobado_RegistraUnMovimientoDePrestamoPorUnidadConElEvaluador()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudAprobada(servicio);
        ServicioPrestamo servicioDeLaEntrega =
            CrearServicioConReloj(memoria, new RelojFijo(FechaDeRealizacion));

        servicioDeLaEntrega.Realizar(idPrestamo, DniFuncionario);

        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);
        Usuario supervisor = new RepositorioUsuario(memoria).ObtenerUsuarioPorDni(DniSupervisor)!;
        IReadOnlyList<Movimiento> movimientos = new RepositorioMovimiento(memoria).ObtenerTodos();

        Assert.AreEqual(CantidadSolicitadaPorDefecto, movimientos.Count);
        foreach (Movimiento movimiento in movimientos)
        {
            Assert.AreEqual(TipoMovimiento.Prestamo, movimiento.Tipo);
            Assert.AreEqual(FechaDeRealizacion, movimiento.FechaHora);
            Assert.AreSame(guardado.Solicitante, movimiento.Solicitante);
            Assert.AreSame(supervisor, movimiento.Evaluador);
        }
        CollectionAssert.AreEquivalent(
            guardado.UnidadesAsignadas.ToList(),
            movimientos.Select(movimiento => movimiento.Unidad).ToList());
    }

    [TestMethod]
    [DataRow(DniSupervisor)]
    [DataRow(DniFuncionarioDesactivado)]
    [DataRow(DniInexistente)]
    public void Realizar_ResponsableNoHabilitado_LanzaExcepcion(string dniResponsable)
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudAprobada(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Realizar(idPrestamo, dniResponsable));
        Assert.AreEqual("El usuario no tiene permiso para realizar prestamos", excepcion.Message);

        VerificarQueSigueAprobadoSinMovimientos(memoria, idPrestamo);
    }

    [TestMethod]
    public void Realizar_PrestamoSolicitado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPrestamo = CrearSolicitudPendiente(servicio);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(
            () => servicio.Realizar(idPrestamo, DniFuncionario));
        Assert.AreEqual("El prestamo debe estar aprobado", excepcion.Message);
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));

        VerificarQueSigueSolicitadaSinPrestarNada(memoria, idPrestamo);
    }

    [TestMethod]
    public void ListarSolicitudesPendientes_VariosEstados_DevuelveSoloLasPendientesConSuSolicitante()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);
        Guid idPendienteDocente = CrearSolicitudPendiente(servicio);
        Guid idPendienteAdministrador = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);

        CrearSolicitudAprobada(servicio);
        Guid idRechazada = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);
        servicio.Rechazar(idRechazada, MotivoDeRechazo, DniSupervisor);
        servicio.Solicitar(CrearSolicitud(), DniAdministrador);

        List<PrestamoDto> resultado = servicio.ListarSolicitudesPendientes();

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
    public void ListarPrestamosAprobados_VariosEstados_DevuelveSoloLosAprobados()
    {
        Memoria memoria = new Memoria();
        ServicioPrestamo servicio = CrearServicio(memoria);

        // La camara tiene 3 unidades: la del administrador pide 1 para que no se rechace por falta de unidades
        Guid idAprobadaDocente = CrearSolicitudAprobada(servicio);
        SolicitudPrestamoDto datosAdministrador =
            CrearSolicitud(tipoEquipamiento: NombreTipoConAprobacion, cantidadSolicitada: 1);
        PrestamoDto solicitudAdministrador = servicio.Solicitar(datosAdministrador, DniAdministrador);
        servicio.Aprobar(solicitudAdministrador.Id, DniSupervisor);
        Guid idAprobadaAdministrador = solicitudAdministrador.Id;

        CrearSolicitudPendiente(servicio);
        Guid idRechazada = CrearSolicitudPendiente(servicio, dniSolicitante: DniAdministrador);
        servicio.Rechazar(idRechazada, MotivoDeRechazo, DniSupervisor);
        servicio.Solicitar(CrearSolicitud(), DniAdministrador);

        List<PrestamoDto> resultado = servicio.ListarPrestamosAprobados();

        Assert.AreEqual(2, resultado.Count);
        CollectionAssert.AreEquivalent(
            new List<Guid> { idAprobadaDocente, idAprobadaAdministrador },
            resultado.Select(prestamo => prestamo.Id).ToList());

        Assert.AreEqual("Renato Alves", resultado.Single(
            prestamo => prestamo.Id == idAprobadaDocente).Solicitante);
        Assert.AreEqual("Lucia Fernandez", resultado.Single(
            prestamo => prestamo.Id == idAprobadaAdministrador).Solicitante);
    }

    private static ServicioPrestamo CrearServicio(Memoria memoria)
    {
        CargarCatalogo(memoria);
        CargarUsuarios(memoria);

        return CrearServicioConReloj(memoria, new RelojFijo(FechaFija));
    }

    private static ServicioPrestamo CrearServicioConReloj(Memoria memoria, TimeProvider reloj)
    {
        return new ServicioPrestamo(
            prestamos: new RepositorioPrestamo(memoria),
            usuarios: new RepositorioUsuario(memoria),
            catalogo: new RepositorioCatalogo(memoria),
            movimientos: new RepositorioMovimiento(memoria),
            reloj: reloj);
    }

    private static SolicitudPrestamoDto CrearSolicitud(
        string tipoEquipamiento = NombreTipoSinAprobacion,
        int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        return new SolicitudPrestamoDto
        {
            TipoEquipamiento = tipoEquipamiento,
            CantidadSolicitada = cantidadSolicitada
        };
    }

    private static Guid CrearSolicitudPendiente(
        ServicioPrestamo servicio,
        string tipoEquipamiento = NombreTipoConAprobacion,
        string dniSolicitante = DniDocente)
    {
        SolicitudPrestamoDto datos = CrearSolicitud(tipoEquipamiento: tipoEquipamiento);
        PrestamoDto pendiente = servicio.Solicitar(datos, dniSolicitante);

        return pendiente.Id;
    }

    private static Guid CrearSolicitudAprobada(ServicioPrestamo servicio)
    {
        Guid idPrestamo = CrearSolicitudPendiente(servicio);
        servicio.Aprobar(idPrestamo, DniSupervisor);

        return idPrestamo;
    }

    private static Prestamo ObtenerPrestamoGuardado(Memoria memoria, Guid idPrestamo)
    {
        return new RepositorioPrestamo(memoria).ObtenerPrestamoPorId(idPrestamo)!;
    }

    private static void CargarCatalogo(Memoria memoria)
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

        new RepositorioCatalogo(memoria).Guardar(catalogo);
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

    private static void CargarUsuarios(Memoria memoria)
    {
        RepositorioUsuario usuarios = new RepositorioUsuario(memoria);

        usuarios.AgregarUsuario(CrearUsuario(dni: DniDocente, tipoUsuario: TipoUsuario.Docente));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniEstudiante, tipoUsuario: TipoUsuario.Estudiante));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniFuncionario, tipoUsuario: TipoUsuario.FuncionarioCE));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniSupervisor, tipoUsuario: TipoUsuario.Supervisor,
            nombre: "Sofia", apellido: "Martinez"));
        usuarios.AgregarUsuario(CrearUsuario(dni: DniAdministrador, tipoUsuario: TipoUsuario.Administrador,
            nombre: "Lucia", apellido: "Fernandez"));

        Usuario desactivado = CrearUsuario(dni: DniDocenteDesactivado, tipoUsuario: TipoUsuario.Docente);
        desactivado.Desactivar();
        usuarios.AgregarUsuario(desactivado);

        Usuario supervisorDesactivado = CrearUsuario(dni: DniSupervisorDesactivado, tipoUsuario: TipoUsuario.Supervisor,
            nombre: "Diego", apellido: "Rodriguez");
        supervisorDesactivado.Desactivar();
        usuarios.AgregarUsuario(supervisorDesactivado);

        Usuario funcionarioDesactivado = CrearUsuario(
            dni: DniFuncionarioDesactivado,
            tipoUsuario: TipoUsuario.FuncionarioCE,
            nombre: "Marcos",
            apellido: "Pereira");
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

    private static void VerificarQueNoSeGuardoNiSePrestoNada(Memoria memoria, string dni)
    {
        Assert.AreEqual(0, new RepositorioPrestamo(memoria).ObtenerPrestamosDeSolicitante(dni: dni).Count,
            "No deberia haberse guardado ningun prestamo");
        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count,
            "No deberia haberse registrado ningun movimiento");

        Catalogo catalogo = new RepositorioCatalogo(memoria).Obtener()!;
        Assert.IsTrue(catalogo.Tipos.SelectMany(tipo => tipo.Unidades).All(unidad => unidad.EstaDisponible()),
            "Ninguna unidad deberia estar prestada");
    }

    private static void VerificarQueSigueSolicitadaSinPrestarNada(Memoria memoria, Guid idPrestamo)
    {
        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Solicitado, guardado.Estado,
            "El prestamo deberia seguir solicitado");
        Assert.AreEqual(0, guardado.UnidadesAsignadas.Count,
            "No deberia haberse asignado ninguna unidad");
        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count,
            "No deberia haberse registrado ningun movimiento");
    }

    private static void VerificarQueSigueAprobadoSinMovimientos(Memoria memoria, Guid idPrestamo)
    {
        Prestamo guardado = ObtenerPrestamoGuardado(memoria, idPrestamo);

        Assert.AreEqual(EstadoPrestamo.Aprobado, guardado.Estado,
            "El prestamo deberia seguir aprobado");
        Assert.AreEqual(CantidadSolicitadaPorDefecto, guardado.UnidadesAsignadas.Count,
            "Deberia conservar sus unidades");
        Assert.AreEqual(0, new RepositorioMovimiento(memoria).ObtenerTodos().Count,
            "No deberia haberse registrado ningun movimiento");
    }
}