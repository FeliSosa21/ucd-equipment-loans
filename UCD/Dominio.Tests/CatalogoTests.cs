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
}


