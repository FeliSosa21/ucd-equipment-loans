namespace Dominio.Tests;

[TestClass]
public sealed class DominioExceptionTests
{
    [TestMethod]
    public void Constructor_ConMensaje_GuardaElMensaje()
    {
        var excepcion = new DominioException("El nombre es inválido.");

        Assert.AreEqual("El nombre es inválido.", excepcion.Message);
    }
}