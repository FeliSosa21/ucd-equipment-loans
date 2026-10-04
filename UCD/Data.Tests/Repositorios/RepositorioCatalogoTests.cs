using Dominio;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioCatalogoTests
{
    [TestMethod]
    public void Obtener_BaseNueva_DevuelveNull()
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);

        var resultado = repositorio.Obtener();

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoValido_ObtenerDevuelveElMismoCatalogo()
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);
        var catalogo = CrearCatalogo();

        repositorio.Guardar(catalogo);
        var resultado = repositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Obtener_OtroRepositorioConLaMismaBase_DevuelveElCatalogoGuardado()
    {
        var memoria = new Memoria();
        var primerRepositorio = new RepositorioCatalogo(memoria);
        var segundoRepositorio = new RepositorioCatalogo(memoria);
        var catalogo = CrearCatalogo();

        primerRepositorio.Guardar(catalogo);
        var resultado = segundoRepositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoNull_LanzaDominioException()
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);

        Assert.ThrowsException<DominioException>(() => repositorio.Guardar(null!));
    }

    [TestMethod]
    public void Guardar_YaExisteUnCatalogo_LanzaDominioException()
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);
        var catalogoExistente = CrearCatalogo();
        repositorio.Guardar(catalogoExistente);
        var catalogoNuevo = CrearCatalogo(nombre: "catalogo nuevo");

        Assert.ThrowsException<DominioException>(() => repositorio.Guardar(catalogoNuevo));
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_TipoExistente_DevuelveElTipo()
    {
        var primerTipo = CrearTipo(nombre: "Proyector Epson PowerLite");
        var segundoTipo = CrearTipo(nombre: "Proyector Sony");
        var repositorio = CrearRepositorioCatalogo(primerTipo, segundoTipo);

        var encontrado = repositorio.ObtenerTipoPorNombre("Proyector Sony");

        Assert.AreSame(segundoTipo, encontrado);
    }

    [TestMethod]
    [DataRow("PROYECTOR EPSON EB-770FI")]
    [DataRow(" Proyector Epson EB-770Fi ")]
    public void ObtenerTipoPorNombre_NombreEquivalente_DevuelveElTipo(string nombreBuscado)
    {
        var tipo = CrearTipo(nombre: "Proyector Epson EB-770Fi");
        var repositorio = CrearRepositorioCatalogo(tipo);

        var encontrado = repositorio.ObtenerTipoPorNombre(nombreBuscado);

        Assert.AreSame(tipo, encontrado);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_NombreInexistente_DevuelveNull()
    {
        var tipo = CrearTipo(nombre: "Proyector Epson EB-770Fi");
        var repositorio = CrearRepositorioCatalogo(tipo);
        var nombreInexistente = "Proyector Sony";

        var encontrado = repositorio.ObtenerTipoPorNombre(nombreInexistente);

        Assert.IsNull(encontrado);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_SinCatalogo_DevuelveNull()
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);

        var encontrado = repositorio.ObtenerTipoPorNombre("Proyector Sony");

        Assert.IsNull(encontrado);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_NombreNull_LanzaArgumentNullException()
    {
        var tipo = CrearTipo(nombre: "Proyector Sony");
        var repositorio = CrearRepositorioCatalogo(tipo);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.ObtenerTipoPorNombre(null!));
    }

    private static Catalogo CrearCatalogo(string nombre = "Catalogo General")
    {
        return new Catalogo(nombre);
    }

    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson PowerLite",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 5,
        bool requiereAprobacion = false,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }

    private static RepositorioCatalogo CrearRepositorioCatalogo(params TipoEquipamiento[] tipos)
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);
        var catalogo = CrearCatalogo();
        foreach (var tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorio.Guardar(catalogo);
        return repositorio;
    }
}