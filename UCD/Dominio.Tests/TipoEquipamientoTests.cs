namespace Dominio.Tests;

[TestClass]
public sealed class TipoEquipamientoTests
{
    [TestMethod]
    public void Constructor_DatosValidos_QuedaActivo()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite", "Proyector para aulas",
            5, false, Categoria.Audiovisual);

        Assert.AreEqual(EstadoTipoEquipamiento.Activo, tipo.Estado);
    }
    [TestMethod]
    public void Constructor_DatosValidos_GuardaElNombre()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite", "Proyector para aulas",
            5, false, Categoria.Audiovisual);

        Assert.AreEqual("Proyector Epson PowerLite", tipo.Nombre);
    }
}