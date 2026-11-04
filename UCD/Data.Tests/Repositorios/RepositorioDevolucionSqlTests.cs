using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioDevolucionSqlTests
{
    private static readonly DateTime InicioPrestamo = new(2026, 11, 3, 10, 0, 0);
    private const string DniFuncionario = "45678905";

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_DevolucionAgregada_OtroContextoLeeQuienLaRegistroYCuando()
    {
        GuardarConOtroContexto(CrearDevolucion(diasDespuesDelPrestamo: 1));

        IReadOnlyList<Devolucion> leidas = LeerPrimeraPagina(new FiltroPorCatalogoYFechas());

        Assert.AreEqual(1, leidas.Count);
        Assert.AreEqual(InicioPrestamo.AddDays(1), leidas[0].FechaHoraDevolucion);
        Assert.AreEqual(DniFuncionario, leidas[0].RegistradaPor.Dni);
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_DevolucionAgregada_OtroContextoLeeSusUnidadesConCondicion()
    {
        GuardarConOtroContexto(CrearDevolucion(diasDespuesDelPrestamo: 1, condicion: CondicionDevolucion.ConFallas,
            observaciones: "No enciende"));

        Devolucion leida = LeerPrimeraPagina(new FiltroPorCatalogoYFechas()).Single();

        UnidadDevuelta unidad = leida.Unidades.Single();
        Assert.AreEqual("EQP-0001", unidad.Unidad.IdInstitucional);
        Assert.AreEqual(CondicionDevolucion.ConFallas, unidad.Condicion);
        Assert.AreEqual("No enciende", unidad.Observaciones);
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_VariasDevoluciones_DeLaMasViejaALaMasRecienteYEnEmpateLaPrimeraRegistrada()
    {
        Devolucion delDiaDos = CrearDevolucion(diasDespuesDelPrestamo: 2);
        Devolucion delDiaUno = CrearDevolucion(diasDespuesDelPrestamo: 1);
        Devolucion otraDelDiaUno = CrearDevolucion(diasDespuesDelPrestamo: 1);
        GuardarConOtroContexto(delDiaDos, delDiaUno, otraDelDiaUno);

        IReadOnlyList<Devolucion> leidas = LeerPrimeraPagina(new FiltroPorCatalogoYFechas());

        CollectionAssert.AreEqual(new List<int> { delDiaUno.Id, otraDelDiaUno.Id, delDiaDos.Id },
            leidas.Select(devolucion => devolucion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_FiltroPorFechas_IncluyeDesdeYExcluyeHasta()
    {
        Devolucion delDiaUno = CrearDevolucion(diasDespuesDelPrestamo: 1);
        Devolucion delDiaDos = CrearDevolucion(diasDespuesDelPrestamo: 2);
        Devolucion delDiaTres = CrearDevolucion(diasDespuesDelPrestamo: 3);
        GuardarConOtroContexto(delDiaUno, delDiaDos, delDiaTres);
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas
            { Desde = InicioPrestamo.AddDays(1), Hasta = InicioPrestamo.AddDays(3) };

        IReadOnlyList<Devolucion> leidas = LeerPrimeraPagina(filtro);

        CollectionAssert.AreEqual(new List<int> { delDiaUno.Id, delDiaDos.Id },
            leidas.Select(devolucion => devolucion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_FiltroPorCatalogo_DevuelveSoloLasDeEseCatalogo()
    {
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook Lenovo ThinkPad", categoria: Categoria.Computacion);
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector Epson", categoria: Categoria.Audiovisual);
        Catalogo electronica = CrearCatalogoCon(nombre: "Electronica", tipo: notebook);
        Catalogo audiovisuales = CrearCatalogoCon(nombre: "Audiovisuales", tipo: proyector);
        Devolucion deNotebook = CrearDevolucionDe(tipo: notebook);
        GuardarCatalogosYDevoluciones(new[] { electronica, audiovisuales }, deNotebook,
            CrearDevolucionDe(tipo: proyector));

        IReadOnlyList<Devolucion> leidas = LeerPrimeraPagina(new FiltroPorCatalogoYFechas { IdCatalogo = electronica.Id });

        CollectionAssert.AreEqual(new List<int> { deNotebook.Id },
            leidas.Select(devolucion => devolucion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_DevolucionAgregada_TraeElPrestamoConSolicitanteYTipo()
    {
        GuardarConOtroContexto(CrearDevolucion(diasDespuesDelPrestamo: 1));

        Devolucion leida = LeerPrimeraPagina(new FiltroPorCatalogoYFechas()).Single();

        Assert.IsNotNull(leida.Prestamo);
        Assert.AreEqual("12345672", leida.Prestamo.Solicitante.Dni);
        Assert.AreEqual("Proyector Epson", leida.Prestamo.TipoEquipamiento.Nombre);
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_SegundaPaginaDeUna_DevuelveSoloLaSegunda()
    {
        Devolucion delDiaUno = CrearDevolucion(diasDespuesDelPrestamo: 1);
        Devolucion delDiaDos = CrearDevolucion(diasDespuesDelPrestamo: 2);
        Devolucion delDiaTres = CrearDevolucion(diasDespuesDelPrestamo: 3);
        GuardarConOtroContexto(delDiaUno, delDiaDos, delDiaTres);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<Devolucion> leidas = new RepositorioDevolucionSql(contextoQueLee)
            .ObtenerPaginaDeDevoluciones(new FiltroPorCatalogoYFechas(), pagina: 2, tamanoPagina: 1);

        CollectionAssert.AreEqual(new List<int> { delDiaDos.Id },
            leidas.Select(devolucion => devolucion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeDevoluciones_FiltroNulo_LanzaArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioDevolucionSql repositorio = new RepositorioDevolucionSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() =>
            repositorio.ObtenerPaginaDeDevoluciones(null!, pagina: 1, tamanoPagina: 10));
    }

    private IReadOnlyList<Devolucion> LeerPrimeraPagina(FiltroPorCatalogoYFechas filtro)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioDevolucionSql(contextoQueLee)
            .ObtenerPaginaDeDevoluciones(filtro, pagina: 1, tamanoPagina: 10);
    }

    private void GuardarConOtroContexto(params Devolucion[] devoluciones)
    {
        GuardarCatalogosYDevoluciones(Array.Empty<Catalogo>(), devoluciones);
    }

    private void GuardarCatalogosYDevoluciones(IEnumerable<Catalogo> catalogos, params Devolucion[] devoluciones)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        contextoQueGuarda.Catalogos.AddRange(catalogos);
        contextoQueGuarda.SaveChanges();
        RepositorioDevolucionSql repositorio = new RepositorioDevolucionSql(contextoQueGuarda);

        foreach (Devolucion devolucion in devoluciones)
        {
            repositorio.AgregarDevolucion(devolucion);
        }
    }

    private static Devolucion CrearDevolucion(int diasDespuesDelPrestamo,
        CondicionDevolucion condicion = CondicionDevolucion.SinObservaciones, string? observaciones = null)
    {
        Prestamo prestamo = CrearPrestamoActivo(CrearTipo(nombre: "Proyector Epson", categoria: Categoria.Audiovisual));

        return Devolver(prestamo, diasDespuesDelPrestamo,
            new UnidadDevuelta(prestamo.UnidadesAsignadas[0], condicion, observaciones));
    }

    private static Devolucion CrearDevolucionDe(TipoEquipamiento tipo)
    {
        Prestamo prestamo = CrearPrestamoActivo(tipo);

        return Devolver(prestamo, diasDespuesDelPrestamo: 1,
            new UnidadDevuelta(prestamo.UnidadesAsignadas[0], CondicionDevolucion.SinObservaciones, null));
    }

    private static Devolucion Devolver(Prestamo prestamo, int diasDespuesDelPrestamo, UnidadDevuelta unidad)
    {
        RegistroDeAccion registro = RegistroDeAccion.DeUsuario(
            CrearUsuario(dni: DniFuncionario, correo: "laura.diaz@ejemplo.com"),
            InicioPrestamo.AddDays(diasDespuesDelPrestamo));

        return new Devolucion(prestamo, registro, new List<UnidadDevuelta> { unidad });
    }

    private static Prestamo CrearPrestamoActivo(TipoEquipamiento tipo)
    {
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(CrearUsuario(dni: "12345672", correo: "ana.perez@ejemplo.com"), tipo,
            cantidadSolicitada: 1, fechaHoraPrestamo: InicioPrestamo);
        prestamo.Activar(new List<UnidadFisica> { unidad });

        return prestamo;
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

    private static Usuario CrearUsuario(string dni, string correo)
    {
        return new Usuario(dni: dni, nombre: "Ana", apellido: "Perez", correo: correo, contrasena: "VivaLaOrt2026");
    }
}
