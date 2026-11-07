using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioAuditoriaTests
{
    private static readonly DateTime FechaFija = new(2026, 11, 4, 10, 0, 0);

    private RepositorioAuditoria _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorio = new RepositorioAuditoria(new Memoria());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_SinFiltro_DelMasRecienteAlMasViejo()
    {
        RegistroAuditoria viejo = CrearRegistro(FechaFija);
        RegistroAuditoria reciente = CrearRegistro(FechaFija.AddHours(2));
        RegistroAuditoria intermedio = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(viejo);
        _repositorio.AgregarRegistro(reciente);
        _repositorio.AgregarRegistro(intermedio);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerPaginaDeRegistros(new FiltroAuditoria(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { reciente, intermedio, viejo }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_MismaFechaHora_PrimeroElUltimoAgregado()
    {
        RegistroAuditoria primero = CrearRegistro(FechaFija);
        RegistroAuditoria segundo = CrearRegistro(FechaFija);
        _repositorio.AgregarRegistro(primero);
        _repositorio.AgregarRegistro(segundo);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerPaginaDeRegistros(new FiltroAuditoria(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { segundo, primero }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_SegundaPaginaDeUno_DevuelveSoloElSegundo()
    {
        RegistroAuditoria viejo = CrearRegistro(FechaFija);
        RegistroAuditoria intermedio = CrearRegistro(FechaFija.AddHours(1));
        RegistroAuditoria reciente = CrearRegistro(FechaFija.AddHours(2));
        _repositorio.AgregarRegistro(viejo);
        _repositorio.AgregarRegistro(intermedio);
        _repositorio.AgregarRegistro(reciente);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerPaginaDeRegistros(new FiltroAuditoria(), pagina: 2, tamanoPagina: 1);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { intermedio }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_FiltroDesde_IncluyeElDeEsaFechaYDejaAfueraLosAnteriores()
    {
        RegistroAuditoria anterior = CrearRegistro(FechaFija.AddMinutes(-1));
        RegistroAuditoria enElDesde = CrearRegistro(FechaFija);
        RegistroAuditoria posterior = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(anterior);
        _repositorio.AgregarRegistro(enElDesde);
        _repositorio.AgregarRegistro(posterior);

        IReadOnlyList<RegistroAuditoria> resultado = _repositorio.ObtenerPaginaDeRegistros(
            new FiltroAuditoria { Desde = FechaFija }, pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { posterior, enElDesde }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_FiltroHasta_DejaAfueraElDeEsaFechaYLosPosteriores()
    {
        RegistroAuditoria anterior = CrearRegistro(FechaFija.AddMinutes(-1));
        RegistroAuditoria enElHasta = CrearRegistro(FechaFija);
        RegistroAuditoria posterior = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(anterior);
        _repositorio.AgregarRegistro(enElHasta);
        _repositorio.AgregarRegistro(posterior);

        IReadOnlyList<RegistroAuditoria> resultado = _repositorio.ObtenerPaginaDeRegistros(
            new FiltroAuditoria { Hasta = FechaFija }, pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { anterior }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_FiltroDniUsuario_DevuelveSoloLosDeEseResponsable()
    {
        Usuario felipe = CrearUsuario(dni: "12345672", correo: "felipe@hotmail.com");
        Usuario laura = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        RegistroAuditoria deFelipe = CrearRegistro(RegistroDeAccion.DeUsuario(felipe, FechaFija));
        RegistroAuditoria deLaura = CrearRegistro(RegistroDeAccion.DeUsuario(laura, FechaFija));
        RegistroAuditoria delSistema = CrearRegistro(RegistroDeAccion.DelSistema(FechaFija));
        _repositorio.AgregarRegistro(deFelipe);
        _repositorio.AgregarRegistro(deLaura);
        _repositorio.AgregarRegistro(delSistema);

        IReadOnlyList<RegistroAuditoria> resultado = _repositorio.ObtenerPaginaDeRegistros(
            new FiltroAuditoria { DniUsuario = "45678905" }, pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { deLaura }, resultado.ToList());
    }

    [TestMethod]
    public void ContarRegistros_FiltroDesde_CuentaSoloLosQueCumplenElFiltro()
    {
        _repositorio.AgregarRegistro(CrearRegistro(FechaFija.AddMinutes(-1)));
        _repositorio.AgregarRegistro(CrearRegistro(FechaFija));
        _repositorio.AgregarRegistro(CrearRegistro(FechaFija.AddHours(1)));

        int resultado = _repositorio.ContarRegistros(new FiltroAuditoria { Desde = FechaFija });

        Assert.AreEqual(2, resultado);
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            _repositorio.ObtenerPaginaDeRegistros(null!, pagina: 1, tamanoPagina: 10));
    }

    [TestMethod]
    public void ContarRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.ContarRegistros(null!));
    }

    [TestMethod]
    public void AgregarRegistro_RegistroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.AgregarRegistro(null!));
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.ObtenerRegistros(null!));
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroSinCriterios_DevuelveListaVacia()
    {
        _repositorio.AgregarRegistro(CrearRegistro(FechaFija));

        IReadOnlyList<RegistroAuditoria> resultado = _repositorio.ObtenerRegistros(new FiltroAuditoria());

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroDesde_IncluyeElDeEsaFechaYDejaAfueraLosAnteriores()
    {
        RegistroAuditoria anterior = CrearRegistro(FechaFija.AddMinutes(-1));
        RegistroAuditoria enElDesde = CrearRegistro(FechaFija);
        RegistroAuditoria posterior = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(anterior);
        _repositorio.AgregarRegistro(enElDesde);
        _repositorio.AgregarRegistro(posterior);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEquivalent(new List<RegistroAuditoria> { enElDesde, posterior }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_VariosRegistros_DelMasRecienteAlMasViejo()
    {
        RegistroAuditoria viejo = CrearRegistro(FechaFija);
        RegistroAuditoria reciente = CrearRegistro(FechaFija.AddHours(2));
        RegistroAuditoria intermedio = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(viejo);
        _repositorio.AgregarRegistro(reciente);
        _repositorio.AgregarRegistro(intermedio);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { reciente, intermedio, viejo }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_MismaFechaHora_PrimeroElUltimoAgregado()
    {
        RegistroAuditoria primero = CrearRegistro(FechaFija);
        RegistroAuditoria segundo = CrearRegistro(FechaFija);
        _repositorio.AgregarRegistro(primero);
        _repositorio.AgregarRegistro(segundo);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { segundo, primero }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroHasta_DejaAfueraElDeEsaFechaYLosPosteriores()
    {
        RegistroAuditoria anterior = CrearRegistro(FechaFija.AddMinutes(-1));
        RegistroAuditoria enElHasta = CrearRegistro(FechaFija);
        RegistroAuditoria posterior = CrearRegistro(FechaFija.AddHours(1));
        _repositorio.AgregarRegistro(anterior);
        _repositorio.AgregarRegistro(enElHasta);
        _repositorio.AgregarRegistro(posterior);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerRegistros(new FiltroAuditoria { Hasta = FechaFija });

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { anterior }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroDniUsuario_DevuelveSoloLosDeEseResponsable()
    {
        Usuario felipe = CrearUsuario(dni: "12345672", correo: "felipe@hotmail.com");
        Usuario laura = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        RegistroAuditoria deFelipe = CrearRegistro(RegistroDeAccion.DeUsuario(felipe, FechaFija));
        RegistroAuditoria deLaura = CrearRegistro(RegistroDeAccion.DeUsuario(laura, FechaFija));
        RegistroAuditoria delSistema = CrearRegistro(RegistroDeAccion.DelSistema(FechaFija));
        _repositorio.AgregarRegistro(deFelipe);
        _repositorio.AgregarRegistro(deLaura);
        _repositorio.AgregarRegistro(delSistema);

        IReadOnlyList<RegistroAuditoria> resultado =
            _repositorio.ObtenerRegistros(new FiltroAuditoria { DniUsuario = "45678905" });

        CollectionAssert.AreEqual(new List<RegistroAuditoria> { deLaura }, resultado.ToList());
    }

    private static RegistroAuditoria CrearRegistro(DateTime fechaHora)
    {
        return CrearRegistro(RegistroDeAccion.DeUsuario(CrearUsuario(), fechaHora));
    }

    private static RegistroAuditoria CrearRegistro(RegistroDeAccion registro)
    {
        return new RegistroAuditoria(
            registro: registro,
            accion: AccionAuditada.SolicitudDePrestamo,
            detalle: "Prestamo de 2 Notebook");
    }

    private static Usuario CrearUsuario(string dni = "12345672", string correo = "felipe@hotmail.com")
    {
        return new Usuario(
            dni: dni,
            nombre: "Felipe",
            apellido: "Sosa",
            correo: correo,
            contrasena: "VivaLaOrt2026");
    }
}
