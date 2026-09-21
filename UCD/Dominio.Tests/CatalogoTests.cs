namespace Dominio.Tests;

[TestClass]
public sealed class CatalogoTests {
    [TestMethod]
    public void ModificarNombre_NombreValido_ActualizaElNombre()
    {
        var catalogo = new Catalogo("CatalogoViejo");

        catalogo.ModificarNombre("CatalogoNuevo");

        Assert.AreEqual("CatalogoNuevo", catalogo.Nombre);
    }
    [TestMethod]
    public void Constructor_NombreMuyCorto_LanzaDominioException()
    {

        Assert.ThrowsException<DominioException>(() =>
        {
            var catalogo = new Catalogo("ca");
        });
    }
    
    [TestMethod]
    public void Constructor_NombreMuyLargo_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            string nombre = new string('z', 81);
            var catalogo = new Catalogo(nombre);
        });
    }
    
    [TestMethod]
    public void ModificarNombre_NombreMuyCorto_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            var catalogo = new Catalogo("CatalogoViejo");

            catalogo.ModificarNombre("Ca");
        });
    }
    
    [TestMethod]
    public void Constructor_NombreVacio_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            var catalogo = new Catalogo("     ");
        });
    }
}


