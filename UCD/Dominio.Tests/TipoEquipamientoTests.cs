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
    [TestMethod]
    public void Constructor_NombreDe2Caracteres_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(nombre: "TV"));
    }
    [TestMethod]
    public void Constructor_NombreDe81Caracteres_LanzaDominioException()
    {
        var nombre = new string('a', 81);

        Assert.ThrowsException<DominioException>(() => CrearTipo(nombre: nombre));
    }

    [TestMethod]
    public void Constructor_NombreSoloEspacios_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(nombre: "     "));
    }
    
    [TestMethod]
    public void Constructor_NombreDe3Caracteres_GuardaElNombre()
    {
        var nombre = new string('a', 3);

        var tipo = CrearTipo(nombre: nombre);

        Assert.AreEqual(nombre, tipo.Nombre);
    }

    [TestMethod]
    public void Constructor_NombreDe80Caracteres_GuardaElNombre()
    {
        var nombre = new string('a', 80);

        var tipo = CrearTipo(nombre: nombre);

        Assert.AreEqual(nombre, tipo.Nombre);
    }
    [TestMethod]
    public void Constructor_DescripcionDe301Caracteres_LanzaDominioException()
    {
        var descripcion = new string('a', 301);

        Assert.ThrowsException<DominioException>(() => CrearTipo(descripcion: descripcion));
    }
    [TestMethod]
    public void Constructor_DuracionCero_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(duracionMaximaDias: 0));
    }

    [TestMethod]
    public void Constructor_DuracionNegativa_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(duracionMaximaDias: -1));
    }
    [TestMethod]
    public void Constructor_DescripcionDe300Caracteres_GuardaLaDescripcion()
    {
        var descripcion = new string('a', 300);

        var tipo = CrearTipo(descripcion: descripcion);

        Assert.AreEqual(descripcion, tipo.Descripcion);
    }

    [TestMethod]
    public void Constructor_DuracionDe1Dia_GuardaLaDuracion()
    {
        var tipo = CrearTipo(duracionMaximaDias: 1);

        Assert.AreEqual(1, tipo.DuracionMaximaDias);
    }
    [TestMethod]
    public void Discontinuar_TipoActivo_QuedaDiscontinuado()
    {
        var tipo = CrearTipo();

        tipo.Discontinuar();

        Assert.AreEqual(EstadoTipoEquipamiento.Discontinuado, tipo.Estado);
    }
    [TestMethod]
    public void EstaActivo_TipoNuevo_DevuelveTrue()
    {
        var tipo = CrearTipo();

        Assert.IsTrue(tipo.EstaActivo);
    }

    [TestMethod]
    public void EstaActivo_TipoDiscontinuado_DevuelveFalse()
    {
        var tipo = CrearTipo();
        tipo.Discontinuar();

        Assert.IsFalse(tipo.EstaActivo);
    }
    [TestMethod]
    public void Modificar_DatosValidos_ActualizaLosDatos()
    {
        var tipo = CrearTipo();

        tipo.Modificar("Notebook Lenovo", "Notebook para préstamo en aula",
            10, true, Categoria.Computacion);

        Assert.AreEqual("Notebook Lenovo", tipo.Nombre);
        Assert.AreEqual("Notebook para préstamo en aula", tipo.Descripcion);
        Assert.AreEqual(10, tipo.DuracionMaximaDias);
        Assert.IsTrue(tipo.RequiereAprobacion);
        Assert.AreEqual(Categoria.Computacion, tipo.Categoria);
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