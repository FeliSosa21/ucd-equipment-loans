namespace Dominio.Tests;

[TestClass]
public sealed class DominioExceptionTests
{
    [TestMethod]
    public void Constructor_SinParametros_TieneMensajePorDefectoYSinExcepcionInterna()
    {
        DominioException excepcion = new DominioException();

        Assert.IsFalse(string.IsNullOrEmpty(excepcion.Message));
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Constructor_ConMensaje_GuardaElMensaje()
    {
        DominioException excepcion = new DominioException("El nombre es invalido");

        Assert.AreEqual("El nombre es invalido", excepcion.Message);
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Constructor_ConMensajeYExcepcionInterna_GuardaAmbos()
    {
        Exception interna = new InvalidOperationException("detalle");

        DominioException excepcion = new DominioException("El nombre es invalido", interna);

        Assert.AreEqual("El nombre es invalido", excepcion.Message);
        Assert.AreSame(interna, excepcion.InnerException);
    }
}