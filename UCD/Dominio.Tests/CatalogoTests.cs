namespace Dominio.Tests;

[TestClass]
public sealed class CatalogoTests
{
    [TestMethod]
    public void ModificarNombre_NombreValido_ActualizaElNombre()
    {
        var catalogo = CrearCatalogo(nombre: "CatalogoViejo");

        catalogo.ModificarNombre("CatalogoNuevo");

        Assert.AreEqual("CatalogoNuevo", catalogo.Nombre);
    }

    [TestMethod]
    public void Constructor_NombreMuyCorto_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: "ca"));
    }

    [TestMethod]
    public void Constructor_NombreMuyLargo_LanzaDominioException()
    {
        var nombre = new string('z', 81);

        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: nombre));
    }

    [TestMethod]
    public void ModificarNombre_NombreMuyCorto_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();

        Assert.ThrowsException<DominioException>(() => catalogo.ModificarNombre("Ca"));
    }

    [TestMethod]
    public void Constructor_NombreVacio_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: "     "));
    }

    private static Catalogo CrearCatalogo(string nombre = "Catalogo General")
    {
        return new Catalogo(nombre);
    }
}


