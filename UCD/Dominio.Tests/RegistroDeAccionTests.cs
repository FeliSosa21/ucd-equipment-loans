namespace Dominio.Tests;

[TestClass]
public sealed class RegistroDeAccionTests
{
    private static readonly DateTime FechaHora = new DateTime(2026, 11, 2, 10, 30, 0);

    [TestMethod]
    public void Constructor_DatosValidos_AsignaResponsableYFechaHora()
    {
        Usuario responsable = CrearUsuario();

        RegistroDeAccion registro = new RegistroDeAccion(responsable, FechaHora);

        Assert.AreEqual(responsable, registro.Responsable);
        Assert.AreEqual(FechaHora, registro.FechaHora);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario("12345672", "Felipe", "Sosa", "felipe@hotmail.com", "VivaLaOrt2026");
    }
}
