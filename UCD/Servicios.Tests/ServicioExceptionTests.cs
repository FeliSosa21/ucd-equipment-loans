namespace Servicios.Tests;

[TestClass]
public class ServicioExceptionTests
{
    [TestMethod]
    public void Crear_SinParametros_TieneMensajePorDefectoYSinExcepcionInterna()
    {
        // Act
        ServicioException excepcion = new ServicioException();

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(excepcion.Message));
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Crear_ConMensaje_GuardaElMensaje()
    {
        // Act
        ServicioException excepcion = new ServicioException("El DNI ya existe");

        // Assert
        Assert.AreEqual("El DNI ya existe", excepcion.Message);
        Assert.IsNull(excepcion.InnerException);
    }

    [TestMethod]
    public void Crear_ConMensajeYExcepcionInterna_GuardaAmbos()
    {
        // Arrange
        Exception interna = new InvalidOperationException("detalle");

        // Act
        ServicioException excepcion = new ServicioException("El DNI ya existe", interna);

        // Assert
        Assert.AreEqual("El DNI ya existe", excepcion.Message);
        Assert.AreSame(interna, excepcion.InnerException);
    }
}
