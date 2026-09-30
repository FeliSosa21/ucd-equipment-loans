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
        var tipo = CrearTipo(duracionMaxPrestamo: 5);

        Assert.AreEqual(5, tipo.DuracionMaxPrestamo);
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
        Assert.ThrowsException<DominioException>(() => CrearTipo(duracionMaxPrestamo: 0));
    }

    [TestMethod]
    public void Constructor_DuracionNegativa_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(duracionMaxPrestamo: -1));
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
        var tipo = CrearTipo(duracionMaxPrestamo: 1);

        Assert.AreEqual(1, tipo.DuracionMaxPrestamo);
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
        Assert.AreEqual(10, tipo.DuracionMaxPrestamo);
        Assert.IsTrue(tipo.RequiereAprobacion);
        Assert.AreEqual(Categoria.Computacion, tipo.Categoria);
    }
    [TestMethod]
    public void Modificar_NombreInvalido_LanzaDominioException()
    {
        var tipo = CrearTipo();

        Assert.ThrowsException<DominioException>(() => ModificarTipo(tipo, nombre: "TV"));
    }

    [TestMethod]
    public void Modificar_DescripcionInvalida_LanzaDominioException()
    {
        var tipo = CrearTipo();
        var descripcion = new string('a', 301);

        Assert.ThrowsException<DominioException>(() => ModificarTipo(tipo, descripcion: descripcion));
    }

    [TestMethod]
    public void Modificar_DuracionInvalida_LanzaDominioException()
    {
        var tipo = CrearTipo();

        Assert.ThrowsException<DominioException>(() => ModificarTipo(tipo, duracionMaxPrestamo: 0));
    }
    [TestMethod]
    public void Modificar_DatosInvalidos_ConservaLosDatosAnteriores()
    {
        var tipo = CrearTipo(nombre: "Proyector Epson PowerLite");

        Assert.ThrowsException<DominioException>(
            () => ModificarTipo(tipo, nombre: "Notebook Lenovo ThinkPad T14", duracionMaxPrestamo: 0));

        Assert.AreEqual("Proyector Epson PowerLite", tipo.Nombre);
    }

    [TestMethod]
    public void Constructor_TipoNuevo_NoTieneUnidades()
    {
        var tipo = CrearTipo();

        Assert.AreEqual(0, tipo.Unidades.Count);
    }

    [TestMethod]
    public void AgregarUnidad_TipoActivo_LaUnidadQuedaEnElTipo()
    {
        var tipo = CrearTipo();
        var unidad = CrearUnidad();

        tipo.AgregarUnidad(unidad);

        Assert.IsTrue(tipo.Unidades.Contains(unidad));
    }

    [TestMethod]
    public void AgregarUnidad_UnidadNull_LanzaDominioException()
    {
        var tipo = CrearTipo();

        Assert.ThrowsException<DominioException>(() => tipo.AgregarUnidad(null!));
    }

    [TestMethod]
    public void AgregarUnidad_TipoDiscontinuado_LanzaDominioException()
    {
        var tipo = CrearTipo();
        tipo.Discontinuar();
        var unidad = CrearUnidad();

        Assert.ThrowsException<DominioException>(() => tipo.AgregarUnidad(unidad));
    }

    [TestMethod]
    public void AgregarUnidad_IdInstitucionalRepetido_LanzaDominioException()
    {
        var tipo = CrearTipo();
        var unidad = CrearUnidad(idInstitucional: "EQP-0001");
        var unidadIdRepetido = CrearUnidad(idInstitucional: "EQP-0001");
        tipo.AgregarUnidad(unidad);

        Assert.ThrowsException<DominioException>(() => tipo.AgregarUnidad(unidadIdRepetido));
    }

    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson PowerLite",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 5,
        bool requiereAprobacion = false,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }

    private static void ModificarTipo(
        TipoEquipamiento tipo,
        string nombre = "Proyector Epson PowerLite",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 5,
        bool requiereAprobacion = false,
        Categoria categoria = Categoria.Audiovisual)
    {
        tipo.Modificar(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }

    private static UnidadFisica CrearUnidad(
        string idInstitucional = "EQP-0001",
        string ubiFisica = "Laboratorio Prueba",
        string? numSerie = "PF3A7X21")
    {
        return new UnidadFisica(idInstitucional, ubiFisica, numSerie);
    }
}