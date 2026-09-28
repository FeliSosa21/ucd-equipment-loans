using Dominio;
using Data.Repositorios;

namespace Data.Tests;

[TestClass]
public sealed class CatalogoRepositorioTests
{
    [TestMethod]
    public void Obtener_BaseNueva_DevuelveNull()
    {
        var memoria = new BaseDeDatosEnMemoria();
        var repositorio = new CatalogoRepositorio(memoria);

        var resultado = repositorio.Obtener();
        
        Assert.IsNull(resultado);
    }
    
    private static Catalogo CrearCatalogo(string nombre = "Catalogo General")
    {
        return new Catalogo(nombre);
    }
}