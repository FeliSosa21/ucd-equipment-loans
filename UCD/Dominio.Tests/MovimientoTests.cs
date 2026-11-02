namespace Dominio.Tests;

[TestClass]
public sealed class MovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

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
    public void DeSolicitud_PrestamoSinEvaluar_QuedaSinEvaluador()
    {
        Movimiento movimiento = Movimiento.DeSolicitud(CrearPrestamo(), RegistroDeAccion.DelSistema(FechaFija));

        Assert.IsNull(movimiento.Evaluador);
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

    [TestMethod]
    public void DeAprobacion_PrestamoAprobado_LlevaLaUnidadYAlEvaluadorComoResponsable()
    {
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");
        Prestamo prestamo = CrearPrestamoAprobado(evaluador, unidad);

        Movimiento movimiento = Movimiento.DeAprobacion(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija),
            unidad);

        Assert.AreEqual(TipoMovimiento.Aprobacion, movimiento.Tipo);
        Assert.AreSame(unidad, movimiento.Unidad);
        Assert.AreSame(evaluador, movimiento.Responsable);
        Assert.AreSame(evaluador, movimiento.Evaluador);
    }

    [TestMethod]
    public void DeAprobacion_PrestamoSinEvaluador_LanzaExcepcion()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() =>
            Movimiento.DeAprobacion(CrearPrestamo(), RegistroDeAccion.DeUsuario(CrearEvaluador(), FechaFija),
                CrearUnidad(idInstitucional: "EQP-0001")));

        Assert.AreEqual("La aprobacion debe tener un evaluador", excepcion.Message);
    }

    [TestMethod]
    public void DePrestamo_PrestamoActivo_LlevaLaUnidadYQuienYCuandoDelRegistro()
    {
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");
        Prestamo prestamo = CrearPrestamoAprobado(evaluador, unidad);

        Movimiento movimiento = Movimiento.DePrestamo(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija),
            unidad);

        Assert.AreEqual(TipoMovimiento.Prestamo, movimiento.Tipo);
        Assert.AreSame(prestamo, movimiento.Prestamo);
        Assert.AreSame(unidad, movimiento.Unidad);
        Assert.AreSame(evaluador, movimiento.Responsable);
        Assert.AreEqual(FechaFija, movimiento.FechaHora);
    }

    [TestMethod]
    public void DeDevolucion_RegistradaPorFuncionario_ResponsableEsElFuncionarioYEvaluadorElDelPrestamo()
    {
        Usuario evaluador = CrearEvaluador();
        Usuario funcionario = CrearFuncionario();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");
        Prestamo prestamo = CrearPrestamoAprobado(evaluador, unidad);

        Movimiento movimiento = Movimiento.DeDevolucion(prestamo,
            RegistroDeAccion.DeUsuario(funcionario, FechaFija), unidad);

        Assert.AreEqual(TipoMovimiento.Devolucion, movimiento.Tipo);
        Assert.AreSame(unidad, movimiento.Unidad);
        Assert.AreSame(funcionario, movimiento.Responsable);
        Assert.AreSame(evaluador, movimiento.Evaluador);
    }

    [TestMethod]
    public void DePrestamo_SinUnidad_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            Movimiento.DePrestamo(CrearPrestamo(), RegistroDeAccion.DelSistema(FechaFija), null!));
    }

    [TestMethod]
    public void DeDevolucion_SinUnidad_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            Movimiento.DeDevolucion(CrearPrestamo(), RegistroDeAccion.DelSistema(FechaFija), null!));
    }

    [TestMethod]
    public void DeSolicitud_SinPrestamo_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            Movimiento.DeSolicitud(null!, RegistroDeAccion.DelSistema(FechaFija)));
    }

    [TestMethod]
    public void DeSolicitud_SinRegistro_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => Movimiento.DeSolicitud(CrearPrestamo(), null!));
    }

    private static Prestamo CrearPrestamoAprobado(Usuario evaluador, UnidadFisica unidad)
    {
        Prestamo prestamo = CrearPrestamo();
        prestamo.TipoEquipamiento.AgregarUnidad(unidad);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });

        return prestamo;
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

    private static Usuario CrearFuncionario()
    {
        Usuario usuario = new Usuario(
            dni: "11111111",
            nombre: "Laura",
            apellido: "Diaz",
            correo: "laura.diaz@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.FuncionarioCE);

        return usuario;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        return new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Laboratorio Prueba", numSerie: null);
    }
}
