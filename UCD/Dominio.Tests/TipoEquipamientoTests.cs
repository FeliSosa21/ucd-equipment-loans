namespace Dominio.Tests;

[TestClass]
public sealed class TipoEquipamientoTests
{
    [TestMethod]
    public void Constructor_DatosValidos_QuedaActivo()
    {
        var tipo = CrearTipo();

        Assert.AreEqual(EstadoTipoEquipamiento.Activo, tipo.Estado);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaElNombre()
    {
        var tipo = CrearTipo(nombre: "Proyector Epson PowerLite");

        Assert.AreEqual("Proyector Epson PowerLite", tipo.Nombre);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaDescripcion()
    {
        var tipo = CrearTipo(descripcion: "Proyector para aulas");

        Assert.AreEqual("Proyector para aulas", tipo.Descripcion);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaDuracionMaxima()
    {
        var tipo = CrearTipo(duracionMaximaDias: 5);

        Assert.AreEqual(5, tipo.DuracionMaximaDias);
    }

    [TestMethod]
    public void Constructor_ConAprobacionPrevia_GuardaQueRequiereAprobacion()
    {
        var tipo = CrearTipo(requiereAprobacion: true);

        Assert.IsTrue(tipo.RequiereAprobacion);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaCategoria()
    {
        var tipo = CrearTipo(categoria: Categoria.Audiovisual);

        Assert.AreEqual(Categoria.Audiovisual, tipo.Categoria);
    }

    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson PowerLite",
        string descripcion = "Proyector para aulas",
        int duracionMaximaDias = 5,
        bool requiereAprobacion = false,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaximaDias, requiereAprobacion, categoria);
    }
}