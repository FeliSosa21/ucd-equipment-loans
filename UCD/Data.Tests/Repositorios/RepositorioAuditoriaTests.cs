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

    private static RegistroAuditoria CrearRegistro(DateTime fechaHora)
    {
        return new RegistroAuditoria(
            registro: RegistroDeAccion.DeUsuario(CrearUsuario(), fechaHora),
            accion: AccionAuditada.SolicitudDePrestamo,
            detalle: "Prestamo de 2 Notebook");
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(
            dni: "12345672",
            nombre: "Felipe",
            apellido: "Sosa",
            correo: "felipe@hotmail.com",
            contrasena: "VivaLaOrt2026");
    }
}
