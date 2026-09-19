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
    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaDescripcion()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite", "Proyector para aulas",
            5, false, Categoria.Audiovisual);

        Assert.AreEqual("Proyector para aulas", tipo.Descripcion);
    }
    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaDuracionMaxima()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite", "Proyector para aulas",
            5, false, Categoria.Audiovisual);

        Assert.AreEqual(5, tipo.DuracionMaximaDias);
    }
    [TestMethod]
    public void Constructor_ConAprobacionPrevia_GuardaQueRequiereAprobacion()
    {
        var tipo = new TipoEquipamiento("Proyector Epson PowerLite", "Proyector para aulas",
            5, true, Categoria.Audiovisual);

        Assert.IsTrue(tipo.RequiereAprobacion);
    }
}