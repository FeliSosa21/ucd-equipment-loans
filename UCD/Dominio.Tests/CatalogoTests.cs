namespace Dominio.Tests;

[TestClass]
public sealed class CatalogoTests {

    [TestMethod]
    public void Catalogo_ModificarNombre()
    {

        var catalogo = new Catalogo("CatalogoViejo");

        catalogo.ModificarNombre("CatalogoNuevo");

        Assert.AreEqual("CatalogoNuevo", catalogo.Nombre);


    }
    
}