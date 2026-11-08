using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioAuditoriaSqlTests
{
    private static readonly DateTime FechaFija = new(2026, 11, 4, 10, 0, 0);

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void AgregarRegistro_DeUsuario_OtroContextoLeeTodosLosDatos()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        RegistroAuditoria registro = new RegistroAuditoria(
            registro: RegistroDeAccion.DeUsuario(responsable, FechaFija),
            accion: AccionAuditada.AprobacionDePrestamo,
            detalle: "Prestamo de 2 Notebook aprobado");

        GuardarConOtroContexto(registro);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RegistroAuditoria leido = contextoQueLee.RegistrosAuditoria
            .Include("_registro.Responsable")
            .Single();
        Assert.AreEqual(FechaFija, leido.FechaHora);
        Assert.AreEqual(AccionAuditada.AprobacionDePrestamo, leido.Accion);
        Assert.AreEqual("Prestamo de 2 Notebook aprobado", leido.Detalle);
        Assert.AreEqual("45678905", leido.Responsable?.Dni);
    }

    [TestMethod]
    public void AgregarRegistro_DelSistema_OtroContextoLoLeeSinResponsable()
    {
        RegistroAuditoria registro = new RegistroAuditoria(
            registro: RegistroDeAccion.DelSistema(FechaFija),
            accion: AccionAuditada.ActivacionDePrestamo,
            detalle: "Prestamo activado automaticamente");

        GuardarConOtroContexto(registro);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RegistroAuditoria leido = contextoQueLee.RegistrosAuditoria
            .Include("_registro.Responsable")
            .Single();
        Assert.IsTrue(leido.EsDelSistema());
        Assert.AreEqual(FechaFija, leido.FechaHora);
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_SinFiltro_DelMasRecienteAlMasViejo()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Viejo"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(2)), "Reciente"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(1)), "Intermedio"));

        IReadOnlyList<RegistroAuditoria> resultado =
            LeerPagina(new FiltroAuditoria(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<string> { "Reciente", "Intermedio", "Viejo" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_MismaFechaHora_PrimeroElDeMayorId()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Primero"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Segundo"));

        IReadOnlyList<RegistroAuditoria> resultado =
            LeerPagina(new FiltroAuditoria(), pagina: 1, tamanoPagina: 10);

        CollectionAssert.AreEqual(new List<string> { "Segundo", "Primero" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeRegistros_SegundaPaginaDeUno_DevuelveSoloElSegundo()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Viejo"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(1)), "Intermedio"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(2)), "Reciente"));

        IReadOnlyList<RegistroAuditoria> resultado =
            LeerPagina(new FiltroAuditoria(), pagina: 2, tamanoPagina: 1);

        CollectionAssert.AreEqual(new List<string> { "Intermedio" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => LeerRegistros(null!));
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroSinCriterios_DevuelveListaVacia()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Guardado"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria());

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroDesde_IncluyeElDeEsaFechaYDejaAfueraLosAnteriores()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddMinutes(-1)), "Anterior"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "EnElDesde"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(1)), "Posterior"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEquivalent(new List<string> { "EnElDesde", "Posterior" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_VariosRegistros_DelMasRecienteAlMasViejo()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Viejo"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(2)), "Reciente"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(1)), "Intermedio"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEqual(new List<string> { "Reciente", "Intermedio", "Viejo" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_MismaFechaHora_PrimeroElDeMayorId()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Primero"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "Segundo"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria { Desde = FechaFija });

        CollectionAssert.AreEqual(new List<string> { "Segundo", "Primero" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroHasta_DejaAfueraElDeEsaFechaYLosPosteriores()
    {
        Usuario responsable = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddMinutes(-1)), "Anterior"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija), "EnElHasta"),
            CrearRegistro(RegistroDeAccion.DeUsuario(responsable, FechaFija.AddHours(1)), "Posterior"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria { Hasta = FechaFija });

        CollectionAssert.AreEqual(new List<string> { "Anterior" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ObtenerRegistros_FiltroDniUsuario_DevuelveSoloLosDeEseResponsable()
    {
        Usuario felipe = CrearUsuario(dni: "12345672", correo: "felipe@hotmail.com");
        Usuario laura = CrearUsuario(dni: "45678905", correo: "laura.diaz@ejemplo.com");
        GuardarConOtroContexto(
            CrearRegistro(RegistroDeAccion.DeUsuario(felipe, FechaFija), "DeFelipe"),
            CrearRegistro(RegistroDeAccion.DeUsuario(laura, FechaFija), "DeLaura"),
            CrearRegistro(RegistroDeAccion.DelSistema(FechaFija), "DelSistema"));

        IReadOnlyList<RegistroAuditoria> resultado = LeerRegistros(new FiltroAuditoria { DniUsuario = "45678905" });

        CollectionAssert.AreEqual(new List<string> { "DeLaura" },
            resultado.Select(registro => registro.Detalle).ToList());
    }

    private void GuardarConOtroContexto(params RegistroAuditoria[] registros)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioAuditoriaSql repositorio = new RepositorioAuditoriaSql(contextoQueGuarda);

        foreach (RegistroAuditoria registro in registros)
        {
            repositorio.AgregarRegistro(registro);
        }
    }

    private IReadOnlyList<RegistroAuditoria> LeerPagina(FiltroAuditoria filtro, int pagina, int tamanoPagina)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioAuditoriaSql repositorio = new RepositorioAuditoriaSql(contextoQueLee);

        return repositorio.ObtenerPaginaDeRegistros(filtro, pagina, tamanoPagina);
    }

    private IReadOnlyList<RegistroAuditoria> LeerRegistros(FiltroAuditoria filtro)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioAuditoriaSql repositorio = new RepositorioAuditoriaSql(contextoQueLee);

        return repositorio.ObtenerRegistros(filtro);
    }

    private static RegistroAuditoria CrearRegistro(RegistroDeAccion registro, string detalle)
    {
        return new RegistroAuditoria(registro, AccionAuditada.SolicitudDePrestamo, detalle);
    }

    private static Usuario CrearUsuario(string dni, string correo)
    {
        return new Usuario(dni: dni, nombre: "Laura", apellido: "Diaz", correo: correo,
            contrasena: "VivaLaOrt2026");
    }
}
