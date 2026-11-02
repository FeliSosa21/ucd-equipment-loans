namespace Dominio.Tests;

[TestClass]
public sealed class MovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

    [TestMethod]
    public void Constructor_DatosValidos_ConservaFechaSolicitanteEvaluadorYUnidad()
    {
        Usuario solicitante = CrearSolicitante();
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");

        Movimiento movimiento = CrearMovimiento(
            solicitante: solicitante,
            evaluador: evaluador,
            unidad: unidad);

        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.AreSame(solicitante, movimiento.Solicitante);
        Assert.AreSame(evaluador, movimiento.Evaluador);
        Assert.AreSame(unidad, movimiento.Unidad);
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Prestamo)]
    [DataRow(TipoMovimiento.Devolucion)]
    [DataRow(TipoMovimiento.Aprobacion)]
    public void Constructor_MovimientoDeUnidad_ConservaElTipo(TipoMovimiento tipo)
    {
        Movimiento movimiento = CrearMovimiento(tipo: tipo, evaluador: CrearEvaluador());

        Assert.AreEqual(tipo, movimiento.Tipo);
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Solicitud)]
    [DataRow(TipoMovimiento.Rechazo)]
    public void Constructor_SolicitudORechazo_ConservaElTipo(TipoMovimiento tipo)
    {
        Movimiento movimiento = CrearMovimientoSinUnidad(tipo: tipo);

        Assert.AreEqual(tipo, movimiento.Tipo);
    }

    [TestMethod]
    public void Constructor_SinEvaluador_QuedaSinEvaluador()
    {
        Movimiento movimiento = CrearMovimiento(evaluador: null);

        Assert.IsNull(movimiento.Evaluador);
    }

    [TestMethod]
    public void Constructor_SolicitanteNulo_LanzaExcepcion()
    {
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");

        Assert.ThrowsException<DominioException>(() => new Movimiento(
            fechaHora: FechaFija,
            tipo: TipoMovimiento.Prestamo,
            solicitante: null!,
            evaluador: evaluador,
            unidad: unidad));
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Prestamo)]
    [DataRow(TipoMovimiento.Devolucion)]
    [DataRow(TipoMovimiento.Aprobacion)]
    public void Constructor_MovimientoDeUnidadSinUnidad_LanzaExcepcion(TipoMovimiento tipo)
    {
        Assert.ThrowsException<DominioException>(() => CrearMovimientoSinUnidad(tipo: tipo));
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Solicitud)]
    [DataRow(TipoMovimiento.Rechazo)]
    public void Constructor_SolicitudORechazoSinUnidad_LoCrea(TipoMovimiento tipo)
    {
        Movimiento movimiento = CrearMovimientoSinUnidad(tipo: tipo);

        Assert.IsNull(movimiento.Unidad);
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Solicitud)]
    [DataRow(TipoMovimiento.Rechazo)]
    public void Constructor_SolicitudORechazoConUnidad_LanzaExcepcion(TipoMovimiento tipo)
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() =>
            CrearMovimiento(tipo: tipo, unidad: CrearUnidad(idInstitucional: "EQP-0001")));

        Assert.AreEqual("La solicitud y el rechazo no llevan unidad", excepcion.Message);
    }

    [TestMethod]
    public void Constructor_AprobacionSinEvaluador_LanzaExcepcion()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() =>
            CrearMovimiento(tipo: TipoMovimiento.Aprobacion, evaluador: null));

        Assert.AreEqual("La aprobacion debe tener un evaluador", excepcion.Message);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(99)]
    public void Constructor_TipoInexistente_LanzaExcepcion(int valorInvalido)
    {
        TipoMovimiento tipoInexistente = (TipoMovimiento)valorInvalido;

        Assert.ThrowsException<DominioException>(() => CrearMovimiento(tipo: tipoInexistente));
    }

    [TestMethod]
    public void DeSolicitud_Prestamo_TomaElSolicitanteDelPrestamoYQuienYCuandoDelRegistro()
    {
        Prestamo prestamo = CrearPrestamo();
        RegistroDeAccion registro = RegistroDeAccion.DeUsuario(prestamo.Solicitante, FechaFija);

        Movimiento movimiento = Movimiento.DeSolicitud(prestamo, registro);

        Assert.AreEqual(TipoMovimiento.Solicitud, movimiento.Tipo);
        Assert.AreSame(prestamo, movimiento.Prestamo);
        Assert.AreSame(prestamo.Solicitante, movimiento.Solicitante);
        Assert.AreSame(prestamo.Solicitante, movimiento.Responsable);
        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.IsNull(movimiento.Unidad);
    }

    [TestMethod]
    public void DeRechazo_DelSistema_QuedaSinResponsableYSinUnidad()
    {
        Prestamo prestamo = CrearPrestamo();

        Movimiento movimiento = Movimiento.DeRechazo(prestamo, RegistroDeAccion.DelSistema(FechaFija));

        Assert.AreEqual(TipoMovimiento.Rechazo, movimiento.Tipo);
        Assert.AreSame(prestamo, movimiento.Prestamo);
        Assert.IsNull(movimiento.Responsable);
        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.IsNull(movimiento.Unidad);
    }

    private static Prestamo CrearPrestamo()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: true, categoria: Categoria.Audiovisual);

        return new Prestamo(CrearSolicitante(), tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: "12345672",
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(
            dni: "45678905",
            nombre: "Ana",
            apellido: "Perez",
            correo: "ana.perez@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Supervisor);

        return usuario;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        UnidadFisica unidad = new UnidadFisica(
            idInstitucional: idInstitucional,
            ubiFisica: "Laboratorio Prueba",
            numSerie: null);

        return unidad;
    }

    private static Movimiento CrearMovimiento(
        TipoMovimiento tipo = TipoMovimiento.Prestamo,
        Usuario? solicitante = null,
        Usuario? evaluador = null,
        UnidadFisica? unidad = null)
    {
        Movimiento movimiento = new Movimiento(
            fechaHora: FechaFija,
            tipo: tipo,
            solicitante: solicitante ?? CrearSolicitante(),
            evaluador: evaluador,
            unidad: unidad ?? CrearUnidad(idInstitucional: "EQP-0001"));

        return movimiento;
    }

    private static Movimiento CrearMovimientoSinUnidad(TipoMovimiento tipo)
    {
        Movimiento movimiento = new Movimiento(
            fechaHora: FechaFija,
            tipo: tipo,
            solicitante: CrearSolicitante(),
            evaluador: CrearEvaluador(),
            unidad: null);

        return movimiento;
    }
}
