namespace Dominio.Tests;

[TestClass]
public sealed class CatalogoTests
{
    [TestMethod]
    public void ModificarNombre_NombreValido_ActualizaElNombre()
    {
        var catalogo = CrearCatalogo(nombre: "CatalogoViejo");

        catalogo.ModificarNombre("CatalogoNuevo");

        Assert.AreEqual("CatalogoNuevo", catalogo.Nombre);
    }

    [TestMethod]
    public void Constructor_NombreMuyCorto_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: "ca"));
    }

    [TestMethod]
    public void Constructor_NombreMuyLargo_LanzaDominioException()
    {
        var nombre = new string('z', 81);

        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: nombre));
    }

    [TestMethod]
    public void ModificarNombre_NombreMuyCorto_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();

        Assert.ThrowsException<DominioException>(() => catalogo.ModificarNombre("Ca"));
    }

    [TestMethod]
    public void Constructor_NombreVacio_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearCatalogo(nombre: "     "));
    }
    
    [TestMethod]
    public void Constructor_CatalogoNuevo_NoTieneTipos()
    {
        var catalogo = CrearCatalogo();
        
        Assert.AreEqual(0, catalogo.Tipos.Count);
    }

    [TestMethod]
    public void AgregarTipo_TipoValido_SeAgregaTipo()
    {
        var catalogo = CrearCatalogo();
        var tipoEquipamiento = CrearTipo();
        
        catalogo.AgregarTipo(tipoEquipamiento);

        CollectionAssert.Contains(catalogo.Tipos, tipoEquipamiento);
    }

    [TestMethod]
    public void AgregarTipo_TipoNull_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();
        
        Assert.ThrowsException<DominioException>(() => catalogo.AgregarTipo(null!));
        
    }
    
    [TestMethod]
    [DataRow("Proyector Epson EB-770Fi", "Proyector Epson EB-770Fi")]
    [DataRow("Proyector Epson EB-770Fi", "PROYECTOR EPSON EB-770FI")]
    [DataRow("Proyector Epson EB-770Fi", " Proyector Epson EB-770Fi ")]
    [DataRow(" Proyector Epson EB-770Fi ", "Proyector Epson EB-770Fi")]
    public void AgregarTipo_NombreEquivalenteAUnoExistente_LanzaDominioException(string nombreExistente,
        string nombreNuevo)
    {
        var catalogo = CrearCatalogo();
        var tipoEquipamientoExistente = CrearTipo(nombre:nombreExistente);
        var tipoEquipamientoRepetido = CrearTipo(nombre:nombreNuevo);
        
        catalogo.AgregarTipo(tipoEquipamientoExistente);
        
        Assert.ThrowsException<DominioException>(() => catalogo.AgregarTipo(tipoEquipamientoRepetido));       
    }
    
    private static Catalogo CrearCatalogo(string nombre = "Catalogo General")
    {
        return new Catalogo(nombre);
    }
    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson EB-770Fi",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 8,
        bool requiereAprobacion = true,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }
}


