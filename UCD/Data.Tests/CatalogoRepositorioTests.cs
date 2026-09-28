using Dominio;
using Data.Repositorios;

namespace Data.Tests;

[TestClass]
public sealed class CatalogoRepositorioTests
{
    [TestMethod]
    public void Obtener_BaseNueva_DevuelveNull()
    {
        var BaseDeDatos = new BaseDeDatosEnMemoria();
        var repositorio = new CatalogoRepositorio(BaseDeDatos);

        var resultado = repositorio.Obtener();

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoValido_ObtenerDevuelveElMismoCatalogo()
    {
        var BaseDeDatos = new BaseDeDatosEnMemoria();
        var repositorio = new CatalogoRepositorio(BaseDeDatos);
        var catalogo = CrearCatalogo();

        repositorio.Guardar(catalogo);
        var resultado = repositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Obtener_OtroRepositorioConLaMismaBase_DevuelveElCatalogoGuardado()
    {
        var BaseDeDatos = new BaseDeDatosEnMemoria();
        var primerRepositorio = new CatalogoRepositorio(BaseDeDatos);
        var segundoRepositorio = new CatalogoRepositorio(BaseDeDatos);
        var catalogo = CrearCatalogo();

        primerRepositorio.Guardar(catalogo);
        var resultado = segundoRepositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoNull_LanzaDominioException()
    {
        var BaseDeDatos = new BaseDeDatosEnMemoria();
        var repositorio = new CatalogoRepositorio(BaseDeDatos);

        Assert.ThrowsException<DominioException>(() => repositorio.Guardar(null!));
    }

    [TestMethod]
    public void Guardar_YaExisteUnCatalogo_LanzaDominioException()
    {
        var BaseDeDatos = new BaseDeDatosEnMemoria();
        var repositorio = new CatalogoRepositorio(BaseDeDatos);
        var catalogoExistente = CrearCatalogo();
        repositorio.Guardar(catalogoExistente);
        var catalogoNuevo = CrearCatalogo(nombre: "catalogo nuevo");

        Assert.ThrowsException<DominioException>(() => repositorio.Guardar(catalogoNuevo));
    }

    private static Catalogo CrearCatalogo(string nombre = "Catalogo General")
    {
        return new Catalogo(nombre);
    }
}