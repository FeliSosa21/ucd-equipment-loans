using Dominio;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioCatalogoTests
{
    [TestMethod]
    public void Obtener_BaseNueva_DevuelveNull()
    {
        var baseDeDatos = new Memoria();
        var repositorio = new RepositorioCatalogo(baseDeDatos);

        var resultado = repositorio.Obtener();

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoValido_ObtenerDevuelveElMismoCatalogo()
    {
        var baseDeDatos = new Memoria();
        var repositorio = new RepositorioCatalogo(baseDeDatos);
        var catalogo = CrearCatalogo();

        repositorio.Guardar(catalogo);
        var resultado = repositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Obtener_OtroRepositorioConLaMismaBase_DevuelveElCatalogoGuardado()
    {
        var baseDeDatos = new Memoria();
        var primerRepositorio = new RepositorioCatalogo(baseDeDatos);
        var segundoRepositorio = new RepositorioCatalogo(baseDeDatos);
        var catalogo = CrearCatalogo();

        primerRepositorio.Guardar(catalogo);
        var resultado = segundoRepositorio.Obtener();

        Assert.AreSame(catalogo, resultado);
    }

    [TestMethod]
    public void Guardar_CatalogoNull_LanzaDominioException()
    {
        var baseDeDatos = new Memoria();
        var repositorio = new RepositorioCatalogo(baseDeDatos);

        Assert.ThrowsException<DominioException>(() => repositorio.Guardar(null!));
    }

    [TestMethod]
    public void Guardar_YaExisteUnCatalogo_LanzaDominioException()
    {
        var baseDeDatos = new Memoria();
        var repositorio = new RepositorioCatalogo(baseDeDatos);
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