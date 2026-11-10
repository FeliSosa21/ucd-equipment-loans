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
        GuardarTresMovimientos();

        IReadOnlyList<Movimiento> resultado =
            LeerPagina(new FiltroPorCatalogoYFechas(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Devolucion, TipoMovimiento.Prestamo, TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeMovimientos_SegundaPaginaDeUno_DevuelveSoloElSegundo()
    {
        GuardarTresMovimientos();

        IReadOnlyList<Movimiento> resultado =
            LeerPagina(new FiltroPorCatalogoYFechas(), pagina: 2, tamanoPagina: 1);

        CollectionAssert.AreEqual(new List<TipoMovimiento> { TipoMovimiento.Prestamo },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeMovimientos_FiltroPorCatalogo_DevuelveSoloLosDeEseCatalogo()
    {
        Usuario solicitante = CrearUsuario(dni: "12345672", correo: "renato.alves@ejemplo.com");
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo ThinkPad", Categoria.Computacion);
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        Catalogo electronica = CrearCatalogoCon("Electronica", notebook);
        Catalogo audiovisuales = CrearCatalogoCon("Audiovisuales", proyector);
        Prestamo deNotebook = new Prestamo(solicitante, notebook, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        Prestamo deProyector = new Prestamo(solicitante, proyector, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        GuardarCatalogosYMovimientos(new[] { electronica, audiovisuales },
            Movimiento.DeSolicitud(deNotebook, RegistroDeAccion.DeUsuario(solicitante, FechaFija)),
            Movimiento.DeRechazo(deProyector, RegistroDeAccion.DelSistema(FechaFija)));

        IReadOnlyList<Movimiento> resultado =
            LeerPagina(new FiltroPorCatalogoYFechas { IdCatalogo = electronica.Id }, pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<TipoMovimiento> { TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeMovimientos_FiltroPorFechas_IncluyeDesdeYExcluyeHasta()
    {
        GuardarTresMovimientos();
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(1) };

        IReadOnlyList<Movimiento> resultado = LeerPagina(filtro, pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<TipoMovimiento> { TipoMovimiento.Prestamo, TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ContarMovimientos_FiltroPorFechas_CuentaSoloLosDelRango()
    {
        GuardarTresMovimientos();
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(1) };

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        int cantidad = new RepositorioMovimientoSql(contextoQueLee).ContarMovimientos(filtro);

        Assert.AreEqual(2, cantidad);
    }

    [TestMethod]
    public void ObtenerPaginaDeMovimientos_Aprobacion_TraeSolicitanteEvaluadorUnidadYResponsable()
    {
        Usuario solicitante = CrearUsuario(dni: "12345672", correo: "renato.alves@ejemplo.com");
        Usuario evaluador = CrearUsuario(dni: "45678905", correo: "sofia.martinez@ejemplo.com");
        TipoEquipamiento tipo = CrearTipo("Notebook Lenovo ThinkPad", Categoria.Computacion);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Laboratorio Prueba",
            numSerie: null);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });
        GuardarConOtroContexto(
            Movimiento.DeAprobacion(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija), unidad));

        Movimiento leido = LeerPagina(new FiltroPorCatalogoYFechas(), pagina: 1, tamanoPagina: 10).Single();

        Assert.IsNotNull(leido.Prestamo);
        Assert.AreEqual("12345672", leido.Solicitante.Dni);
        Assert.AreEqual("45678905", leido.Evaluador?.Dni);
        Assert.AreEqual("EQP-0001", leido.Unidad?.IdInstitucional);
        Assert.AreEqual("45678905", leido.Responsable?.Dni);
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroSinCriterios_DevuelveListaVacia()
    {
        GuardarTresMovimientos();

        IReadOnlyList<Movimiento> resultado = LeerMovimientos(new FiltroPorCatalogoYFechas());

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorFechas_DelMasRecienteAlMasViejoYEnEmpateElUltimoRegistrado()
    {
        GuardarTresMovimientos();
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(2) };

        IReadOnlyList<Movimiento> resultado = LeerMovimientos(filtro);

        CollectionAssert.AreEqual(
            new List<TipoMovimiento> { TipoMovimiento.Devolucion, TipoMovimiento.Prestamo, TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorFechas_IncluyeDesdeYExcluyeHasta()
    {
        GuardarTresMovimientos();
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = FechaFija, Hasta = FechaFija.AddDays(1) };

        IReadOnlyList<Movimiento> resultado = LeerMovimientos(filtro);

        CollectionAssert.AreEqual(new List<TipoMovimiento> { TipoMovimiento.Prestamo, TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerMovimientos_FiltroPorCatalogo_DevuelveSoloLosDeEseCatalogo()
    {
        Usuario solicitante = CrearUsuario(dni: "12345672", correo: "renato.alves@ejemplo.com");
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo ThinkPad", Categoria.Computacion);
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        Catalogo electronica = CrearCatalogoCon("Electronica", notebook);
        Catalogo audiovisuales = CrearCatalogoCon("Audiovisuales", proyector);
        Prestamo deNotebook = new Prestamo(solicitante, notebook, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        Prestamo deProyector = new Prestamo(solicitante, proyector, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        GuardarCatalogosYMovimientos(new[] { electronica, audiovisuales },
            Movimiento.DeSolicitud(deNotebook, RegistroDeAccion.DeUsuario(solicitante, FechaFija)),
            Movimiento.DeRechazo(deProyector, RegistroDeAccion.DelSistema(FechaFija)));

        IReadOnlyList<Movimiento> resultado =
            LeerMovimientos(new FiltroPorCatalogoYFechas { IdCatalogo = electronica.Id });

        CollectionAssert.AreEqual(new List<TipoMovimiento> { TipoMovimiento.Solicitud },
            resultado.Select(movimiento => movimiento.Tipo).ToList());
    }

    [TestMethod]
    public void ObtenerMovimientos_Aprobacion_TraeSolicitanteEvaluadorUnidadYResponsable()
    {
        Usuario solicitante = CrearUsuario(dni: "12345672", correo: "renato.alves@ejemplo.com");
        Usuario evaluador = CrearUsuario(dni: "45678905", correo: "sofia.martinez@ejemplo.com");
        TipoEquipamiento tipo = CrearTipo("Notebook Lenovo ThinkPad", Categoria.Computacion);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Laboratorio Prueba",
            numSerie: null);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });
        GuardarConOtroContexto(
            Movimiento.DeAprobacion(prestamo, RegistroDeAccion.DeUsuario(evaluador, FechaFija), unidad));

        Movimiento leido = LeerMovimientos(new FiltroPorCatalogoYFechas { Desde = FechaFija }).Single();

        Assert.IsNotNull(leido.Prestamo);
        Assert.AreEqual("12345672", leido.Solicitante.Dni);
        Assert.AreEqual("45678905", leido.Evaluador?.Dni);
        Assert.AreEqual("EQP-0001", leido.Unidad?.IdInstitucional);
        Assert.AreEqual("45678905", leido.Responsable?.Dni);
    }

    private void GuardarTresMovimientos()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        UnidadFisica unidad = prestamo.UnidadesAsignadas[0];
        GuardarConOtroContexto(
            Movimiento.DeSolicitud(prestamo, RegistroDeAccion.DeUsuario(prestamo.Solicitante, FechaFija)),
            Movimiento.DePrestamo(prestamo, RegistroDeAccion.DelSistema(FechaFija), unidad),
            Movimiento.DeDevolucion(prestamo, RegistroDeAccion.DelSistema(FechaFija.AddDays(1)), unidad));
    }

    private IReadOnlyList<Movimiento> LeerPagina(FiltroPorCatalogoYFechas filtro, int pagina, int tamanoPagina)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioMovimientoSql repositorio = new RepositorioMovimientoSql(contextoQueLee);

        return repositorio.ObtenerPaginaDeMovimientos(filtro, pagina, tamanoPagina);
    }

    private IReadOnlyList<Movimiento> LeerMovimientos(FiltroPorCatalogoYFechas filtro)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioMovimientoSql repositorio = new RepositorioMovimientoSql(contextoQueLee);

        return repositorio.ObtenerMovimientos(filtro);
    }

    private void GuardarCatalogosYMovimientos(IEnumerable<Catalogo> catalogos, params Movimiento[] movimientos)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        contextoQueGuarda.Catalogos.AddRange(catalogos);
        contextoQueGuarda.SaveChanges();
        RepositorioMovimientoSql repositorio = new RepositorioMovimientoSql(contextoQueGuarda);

        foreach (Movimiento movimiento in movimientos)
        {
            repositorio.AgregarMovimiento(movimiento);
        }
    }

    private static TipoEquipamiento CrearTipo(string nombre, Categoria categoria)
    {
        return new TipoEquipamiento(nombre: nombre, descripcion: "Equipo de prueba", duracionMaxPrestamo: 5,
            requiereAprobacion: false, categoria: categoria);
    }

    private static Catalogo CrearCatalogoCon(string nombre, TipoEquipamiento tipo)
    {
        Catalogo catalogo = new Catalogo(nombre);
        catalogo.AgregarTipo(tipo);

        return catalogo;
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
