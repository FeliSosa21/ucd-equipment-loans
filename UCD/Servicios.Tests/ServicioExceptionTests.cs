namespace Servicios.Tests;

[TestClass]
public class ServicioExceptionTests
{
    [TestMethod]
    public void Constructor_SinParametros_TieneMensajePorDefectoYSinExcepcionInterna()
    {
        ServicioException excepcion = new ServicioException();

        Assert.IsFalse(string.IsNullOrEmpty(excepcion.Message));
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Constructor_ConMensaje_GuardaElMensaje()
    {
        ServicioException excepcion = new ServicioException("El DNI ya existe");

        Assert.AreEqual("El DNI ya existe", excepcion.Message);
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Constructor_ConMensajeYExcepcionInterna_GuardaAmbos()
    {
        Exception interna = new InvalidOperationException("detalle");

        ServicioException excepcion = new ServicioException("El DNI ya existe", interna);

        Assert.AreEqual("El DNI ya existe", excepcion.Message);
        Assert.AreSame(interna, excepcion.InnerException);
    }
}
