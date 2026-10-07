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
    [DataRow(TipoMovimiento.Solicitud)]
    [DataRow(TipoMovimiento.Aprobacion)]
    [DataRow(TipoMovimiento.Rechazo)]
    public void Constructor_CadaTipo_ConservaElTipo(TipoMovimiento tipo)
    {
        Movimiento movimiento = CrearMovimiento(tipo: tipo);

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
        Usuario solicitante = CrearSolicitante();
        Usuario evaluador = CrearEvaluador();

        Assert.ThrowsException<DominioException>(() => new Movimiento(
            fechaHora: FechaFija,
            tipo: tipo,
            solicitante: solicitante,
            evaluador: evaluador,
            unidad: null));
    }

    [TestMethod]
    [DataRow(TipoMovimiento.Solicitud)]
    [DataRow(TipoMovimiento.Rechazo)]
    public void Constructor_SolicitudORechazoSinUnidad_LoCrea(TipoMovimiento tipo)
    {
        Usuario solicitante = CrearSolicitante();
        Usuario evaluador = CrearEvaluador();

        Movimiento movimiento = new Movimiento(
            fechaHora: FechaFija,
            tipo: tipo,
            solicitante: solicitante,
            evaluador: evaluador,
            unidad: null);

        Assert.IsNull(movimiento.Unidad);
        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.AreEqual(tipo, movimiento.Tipo);
        Assert.AreSame(solicitante, movimiento.Solicitante);
        Assert.AreSame(evaluador, movimiento.Evaluador);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(99)]
    public void Constructor_TipoInexistente_LanzaExcepcion(int valorInvalido)
    {
        TipoMovimiento tipoInexistente = (TipoMovimiento)valorInvalido;

        Assert.ThrowsException<DominioException>(() => CrearMovimiento(tipo: tipoInexistente));
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
}
