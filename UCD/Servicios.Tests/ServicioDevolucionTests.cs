using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioDevolucionTests
{
    private const string DniFuncionario = "12345672";
    private const string DniSolicitante = "45678905";
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);
    private static readonly DateTime MomentoDevolucion = InicioPrestamo.AddDays(2);

    private const string TipoQueSeDevuelve = "Proyector Epson";
    private const string TipoDeUnaUnidad = "Notebook Lenovo";

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;
    private int _idCatalogo;

    // El servicio usa un solo contexto para todos sus repositorios, como en la app. Los datos se preparan
    // con otro contexto y se verifican con uno nuevo, para no leer lo que quedo en memoria.
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
    public void CondicionesPosibles_DevuelveLasTresCondiciones()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        List<string> condiciones = servicio.CondicionesPosibles();

        CollectionAssert.AreEqual(new List<string> { "Sin observaciones", "Con fallas", "Dañada" }, condiciones);
    }

    [TestMethod]
    public void ListarPrestamosActivos_UnoActivo_DevuelveSusDatos()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        List<PrestamoActivoDto> activos = servicio.ListarPrestamosActivos(DniFuncionario);

        Assert.AreEqual(1, activos.Count);
        Assert.AreEqual(prestamo.Id, activos[0].Id);
        Assert.AreEqual("Ana Perez", activos[0].Solicitante);
        Assert.AreEqual("Proyector Epson", activos[0].TipoEquipamiento);
        CollectionAssert.AreEqual(new List<string> { "EQP-0001", "EQP-0002" }, activos[0].Unidades);
    }

    [TestMethod]
    public void ListarPrestamosActivos_ResponsableSinPermiso_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ListarPrestamosActivos(DniSolicitante));
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_ElPrestamoDejaDeEstarActivo()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.AreEqual(EstadoPrestamo.Devuelto, LeerPrestamo(prestamo.Id).Estado);
        Assert.AreEqual(0, servicio.ListarPrestamosActivos(DniFuncionario).Count);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_RegistraLaDevolucionConSuResponsableYSuHora()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        RegistroAuditoria registro = LeerAccionesDelFuncionario().Single();
        Assert.AreEqual(AccionAuditada.DevolucionDePrestamo, registro.Accion);
        Assert.AreEqual(MomentoDevolucion, registro.FechaHora);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_ElDetalleTieneElSolicitanteYLaCantidadDeUnidades()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.AreEqual("Devolución del préstamo de Ana Perez (DNI 45678905): 2 unidades",
            LeerAccionesDelFuncionario().Single().Detalle);
    }

    [TestMethod]
    public void RegistrarDevolucion_UnaUnidad_ElDetalleDiceUnaUnidad()
    {
        ServicioDevolucion servicio = CrearServicio(out _);
        Prestamo deUnaUnidad = AgregarPrestamoDeUnaUnidad();
        DevolucionDto datos = new DevolucionDto
        {
            IdPrestamo = deUnaUnidad.Id,
            Unidades = new List<UnidadDevueltaDto>
            {
                new UnidadDevueltaDto { IdInstitucional = "EQP-0003", Condicion = "Sin observaciones" }
            }
        };

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        Assert.AreEqual("Devolución del préstamo de Ana Perez (DNI 45678905): 1 unidad",
            LeerAccionesDelFuncionario().Single().Detalle);
    }

    [TestMethod]
    public void RegistrarDevolucion_CondicionPorUnidad_ActualizaCadaUnidad()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Dañada");

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        Prestamo guardado = LeerPrestamo(prestamo.Id);
        Assert.AreEqual(EstadoUnidadFisica.Disponible, UnidadAsignada(guardado, "EQP-0001").Estado);
        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, UnidadAsignada(guardado, "EQP-0002").Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_LaGuarda()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Devolucion guardada = LeerDevoluciones().Single();
        Assert.AreEqual(prestamo.Id, guardada.Prestamo.Id);
        Assert.AreEqual(DniFuncionario, guardada.RegistradaPor.Dni);
        Assert.AreEqual(MomentoDevolucion, guardada.FechaHoraDevolucion);
    }

    [TestMethod]
    public void RegistrarDevolucion_Supervisor_LaRegistra()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        AsignarAlFuncionarioElTipo(TipoUsuario.Supervisor().Nombre);

        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.AreEqual(EstadoPrestamo.Devuelto, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_RegistraUnMovimientoPorUnidad()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        IReadOnlyList<Movimiento> movimientos = LeerMovimientos();
        Assert.AreEqual(2, movimientos.Count);
        CollectionAssert.AreEquivalent(prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList(),
            movimientos.Select(movimiento => movimiento.Unidad!.IdInstitucional).ToList());
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_ElMovimientoTieneTipoFechaYSolicitante()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);

        servicio.RegistrarDevolucion(datos, DniFuncionario);

        Movimiento movimiento = LeerMovimientos()[0];
        Assert.AreEqual(TipoMovimiento.Devolucion, movimiento.Tipo);
        Assert.AreEqual(MomentoDevolucion, movimiento.FechaHora);
        Assert.AreEqual(prestamo.Solicitante.Dni, movimiento.Solicitante.Dni);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_DevuelveQuienLaRegistro()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        DevolucionRegistradaDto registrada = servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.AreEqual("Felipe Sosa", registrada.RegistradaPor);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosValidos_DevuelveLaCondicionDeCadaUnidad()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Con fallas");
        datos.Unidades[1].Observaciones = "No enciende";

        DevolucionRegistradaDto registrada = servicio.RegistrarDevolucion(datos, DniFuncionario);

        Assert.AreEqual(2, registrada.Unidades.Count);
        UnidadDevueltaDto segunda = registrada.Unidades.Single(unidad => unidad.IdInstitucional == "EQP-0002");
        Assert.AreEqual("Con fallas", segunda.Condicion);
        Assert.AreEqual("No enciende", segunda.Observaciones);
    }

    [TestMethod]
    public void RegistrarDevolucion_DevolucionFallida_NoRegistraMovimientos()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades.RemoveAt(1);

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.AreEqual(0, LeerMovimientos().Count);
    }

    [TestMethod]
    public void RegistrarDevolucion_DatosNulos_LanzaArgumentNullException()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        Assert.ThrowsException<ArgumentNullException>(() => servicio.RegistrarDevolucion(null!, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_UnidadesNulas_LanzaArgumentNullException()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades = null!;

        Assert.ThrowsException<ArgumentNullException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_PrestamoInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(Guid.NewGuid()), DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableSinPermiso_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniSolicitante));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), "30000004"));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_UnidadQueNoEsDelPrestamo_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades[1].IdInstitucional = "EQP-0099";

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_CondicionInexistente_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Perdida");

        Assert.ThrowsException<ServicioException>(() => servicio.RegistrarDevolucion(datos, DniFuncionario));
    }

    [TestMethod]
    public void RegistrarDevolucion_FaltaUnaUnidad_LanzaExcepcionYNoCambiaNada()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id);
        datos.Unidades.RemoveAt(1);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
        Assert.AreEqual(EstadoUnidadFisica.Prestada, UnidadAsignada(LeerPrestamo(prestamo.Id), "EQP-0001").Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_FechaAnteriorAlPrestamo_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo, InicioPrestamo.AddDays(-1));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_ResponsableDesactivado_LanzaExcepcion()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DesactivarAlFuncionario();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_ObservacionesDemasiadoLargas_LanzaExcepcionConLaDelDominio()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        DevolucionDto datos = CrearDatos(prestamo.Id, condicionSegunda: "Con fallas");
        datos.Unidades[1].Observaciones = new string('a', 301);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(datos, DniFuncionario));
        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
        Assert.AreEqual(EstadoPrestamo.Activo, LeerPrestamo(prestamo.Id).Estado);
    }

    [TestMethod]
    public void RegistrarDevolucion_PrestamoYaDevuelto_LanzaExcepcionYNoLaGuardaDosVeces()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));
        Assert.AreEqual(1, LeerDevoluciones().Count);
    }

    [TestMethod]
    public void RegistrarDevolucion_PrestamoYaDevuelto_NoAgregaNadaAlLog()
    {
        ServicioDevolucion servicio = CrearServicio(out Prestamo prestamo);
        servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.RegistrarDevolucion(CrearDatos(prestamo.Id), DniFuncionario));

        Assert.AreEqual(1, LeerAccionesDelFuncionario().Count);
    }

    private static DevolucionDto CrearDatos(Guid idPrestamo, string condicionSegunda = "Sin observaciones")
    {
        return new DevolucionDto
        {
            IdPrestamo = idPrestamo,
            Unidades = new List<UnidadDevueltaDto>
            {
                new UnidadDevueltaDto { IdInstitucional = "EQP-0001", Condicion = "Sin observaciones" },
                new UnidadDevueltaDto { IdInstitucional = "EQP-0002", Condicion = condicionSegunda }
            }
        };
    }

    private ServicioDevolucion CrearServicio(out Prestamo prestamo)
    {
        return CrearServicio(out prestamo, MomentoDevolucion);
    }

    private ServicioDevolucion CrearServicio(out Prestamo prestamo, DateTime ahora)
    {
        prestamo = GuardarUsuariosCatalogoYPrestamo();
        TimeProvider reloj = new RelojFijo(ahora);

        return new ServicioDevolucion(new RepositorioPrestamoSql(_contexto), new RepositorioUsuarioSql(_contexto),
            new RepositorioDevolucionSql(_contexto), new RepositorioMovimientoSql(_contexto), reloj,
            new RegistradorDeAuditoria(new RepositorioAuditoriaSql(_contexto), reloj));
    }

    // Los tipos de usuario se guardan aca y no se toman de la base: la carga inicial puede no traerlos (D19).
    private Prestamo GuardarUsuariosCatalogoYPrestamo()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioTipoUsuarioSql tipos = new RepositorioTipoUsuarioSql(preparacion);
        TipoUsuario funcionarioCE = TipoUsuario.FuncionarioCE();
        TipoUsuario docente = TipoUsuario.Docente();
        tipos.AgregarTipo(funcionarioCE);
        tipos.AgregarTipo(docente);
        tipos.AgregarTipo(TipoUsuario.Supervisor());

        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        Usuario funcionario = new Usuario(dni: DniFuncionario, nombre: "Felipe", apellido: "Sosa",
            correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");
        funcionario.AsignarTipo(funcionarioCE);
        Usuario solicitante = new Usuario(dni: DniSolicitante, nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        solicitante.AsignarTipo(docente);
        usuarios.AgregarUsuario(funcionario);
        usuarios.AgregarUsuario(solicitante);

        TipoEquipamiento proyector = new TipoEquipamiento(nombre: TipoQueSeDevuelve,
            descripcion: "Proyector para aulas", duracionMaxPrestamo: 5, requiereAprobacion: false,
            categoria: Categoria.Audiovisual);
        List<UnidadFisica> unidades = new List<UnidadFisica>
        {
            new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null),
            new UnidadFisica(idInstitucional: "EQP-0002", ubiFisica: "Deposito A", numSerie: null)
        };
        foreach (UnidadFisica unidad in unidades)
        {
            proyector.AgregarUnidad(unidad);
        }
        TipoEquipamiento notebook = new TipoEquipamiento(nombre: TipoDeUnaUnidad,
            descripcion: "Notebook para clases", duracionMaxPrestamo: 5, requiereAprobacion: false,
            categoria: Categoria.Computacion);
        notebook.AgregarUnidad(new UnidadFisica(idInstitucional: "EQP-0003", ubiFisica: "Deposito A", numSerie: null));
        Catalogo catalogo = new Catalogo("Catalogo general");
        catalogo.AgregarTipo(proyector);
        catalogo.AgregarTipo(notebook);
        new RepositorioCatalogoSql(preparacion).AgregarCatalogo(catalogo);
        _idCatalogo = catalogo.Id;

        Prestamo prestamo = new Prestamo(solicitante, proyector, cantidadSolicitada: 2,
            fechaHoraPrestamo: InicioPrestamo);
        prestamo.Activar(unidades);
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
        return prestamo;
    }

    private Prestamo AgregarPrestamoDeUnaUnidad()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Usuario solicitante = new RepositorioUsuarioSql(preparacion).ObtenerUsuarioPorDni(DniSolicitante)
            ?? throw new InvalidOperationException($"No existe el usuario con DNI {DniSolicitante}");
        TipoEquipamiento tipo = new RepositorioCatalogoSql(preparacion)
                .ObtenerTipoPorNombre(_idCatalogo, TipoDeUnaUnidad)
            ?? throw new InvalidOperationException($"No existe el tipo {TipoDeUnaUnidad}");
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: InicioPrestamo);
        prestamo.Activar(tipo.Unidades.ToList());
        new RepositorioPrestamoSql(preparacion).AgregarPrestamo(prestamo);
        return prestamo;
    }

    private void AsignarAlFuncionarioElTipo(string nombreTipo)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        Usuario funcionario = ObtenerFuncionario(usuarios);
        TipoUsuario tipo = new RepositorioTipoUsuarioSql(preparacion).ObtenerTipoPorNombre(nombreTipo)
            ?? throw new InvalidOperationException($"No existe el tipo de usuario {nombreTipo}");
        funcionario.AsignarTipo(tipo);
        usuarios.ActualizarUsuario(funcionario);
    }

    private void DesactivarAlFuncionario()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        Usuario funcionario = ObtenerFuncionario(usuarios);
        funcionario.Desactivar();
        usuarios.ActualizarUsuario(funcionario);
    }

    private static Usuario ObtenerFuncionario(RepositorioUsuarioSql usuarios)
    {
        return usuarios.ObtenerUsuarioPorDni(DniFuncionario)
            ?? throw new InvalidOperationException($"No existe el usuario con DNI {DniFuncionario}");
    }

    private Prestamo LeerPrestamo(Guid idPrestamo)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        return new RepositorioPrestamoSql(contextoQueLee).ObtenerPrestamoPorId(idPrestamo)
            ?? throw new InvalidOperationException($"No existe el prestamo {idPrestamo}");
    }

    private static UnidadFisica UnidadAsignada(Prestamo prestamo, string idInstitucional)
    {
        return prestamo.UnidadesAsignadas.Single(unidad => unidad.IdInstitucional == idInstitucional);
    }

    // Con un filtro sin criterios el repositorio no devuelve nada: Desde = MinValue abarca todo lo registrado.
    private IReadOnlyList<Movimiento> LeerMovimientos()
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        return new RepositorioMovimientoSql(contextoQueLee)
            .ObtenerMovimientos(new FiltroPorCatalogoYFechas { Desde = DateTime.MinValue });
    }

    // Es la consulta de la exportacion: sin criterios trae todas las devoluciones.
    private IReadOnlyList<Devolucion> LeerDevoluciones()
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        return new RepositorioDevolucionSql(contextoQueLee).ObtenerDevoluciones(new FiltroPorCatalogoYFechas());
    }

    private IReadOnlyList<RegistroAuditoria> LeerAccionesDelFuncionario()
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        return new RepositorioAuditoriaSql(contextoQueLee)
            .ObtenerRegistros(new FiltroAuditoria { DniUsuario = DniFuncionario });
    }
}
