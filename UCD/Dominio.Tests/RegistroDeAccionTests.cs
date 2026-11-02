namespace Dominio.Tests;

[TestClass]
public sealed class RegistroDeAccionTests
{
    private static readonly DateTime FechaHora = new DateTime(2026, 11, 2, 10, 30, 0);

    [TestMethod]
    public void DeUsuario_DatosValidos_AsignaResponsableYFechaHora()
    {
        Usuario responsable = CrearUsuario();

        RegistroDeAccion registro = RegistroDeAccion.DeUsuario(responsable, FechaHora);

        Assert.AreEqual(responsable, registro.Responsable);
        Assert.AreEqual(FechaHora, registro.FechaHora);
    }

    [TestMethod]
    public void DeUsuario_FechaHoraSinAsignar_LanzaExcepcion()
    {
        Usuario responsable = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => RegistroDeAccion.DeUsuario(responsable, default));
    }

    [TestMethod]
    public void DeUsuario_UsuarioNulo_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => RegistroDeAccion.DeUsuario(null!, FechaHora));
    }

    [TestMethod]
    public void DelSistema_FechaValida_NoTieneResponsable()
    {
        RegistroDeAccion registro = RegistroDeAccion.DelSistema(FechaHora);

        Assert.IsNull(registro.Responsable);
        Assert.AreEqual(FechaHora, registro.FechaHora);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario("12345672", "Felipe", "Sosa", "felipe@hotmail.com", "VivaLaOrt2026");
    }
}
