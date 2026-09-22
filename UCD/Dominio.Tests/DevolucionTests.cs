namespace Dominio.Tests;

[TestClass]
public sealed class DevolucionTests
{
    [TestMethod]
    public void Constructor_FechaValida_GuardaLaFecha()
    {
        var fecha = new DateTime(2026, 9, 22);
        var devolucion = CrearDevolucion(fechaHoraDevolucion: fecha);

        Assert.AreEqual(fecha, devolucion.FechaHoraDevolucion);
    }

    private static Devolucion CrearDevolucion(DateTime fechaHoraDevolucion = default)
    {
        return new Devolucion(fechaHoraDevolucion);
    }
}