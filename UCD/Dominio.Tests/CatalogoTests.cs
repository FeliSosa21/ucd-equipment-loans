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
    public void AgregarTipo_TipoConNombreRepetido_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();
        string nombreRepetido = "Proyector Epson EB-770Fi";
        var tipoEquipamiento = CrearTipo(nombre: nombreRepetido);
        var tipoEquipamientoNombreRepetido = CrearTipo(nombre: nombreRepetido, descripcion: "proyector para sala recreativa");
        catalogo.AgregarTipo(tipoEquipamiento);
        
        Assert.ThrowsException<DominioException>(() => catalogo.AgregarTipo(tipoEquipamientoNombreRepetido));
    }

    [TestMethod]
    public void AgregarTipo_NombreRepetidoConDistintasMayusculas_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();
        var tipoEquipamiento = CrearTipo(nombre: "Proyector Epson EB-770FI");
        var tipoEquipamientoNombreEnMayusculas = CrearTipo(nombre: "PROYECTOR EPSON EB-770Fi");
        catalogo.AgregarTipo(tipoEquipamiento);
        
        Assert.ThrowsException<DominioException>(() => catalogo.AgregarTipo(tipoEquipamientoNombreEnMayusculas));
    }
    
    [TestMethod]
    public void AgregarTipo_NombreRepetidoConEspaciosEnLosExtremos_LanzaDominioException()
    {
        var catalogo = CrearCatalogo();
        var tipoEquipamiento = CrearTipo(nombre: "Proyector Epson EB-770Fi");
        var tipoEquipamientoEspacioAlPrincipoYAlFinal =CrearTipo(nombre: " Proyector Epson EB-770Fi ");
        catalogo.AgregarTipo(tipoEquipamiento);
        
        Assert.ThrowsException<DominioException>(() => catalogo.AgregarTipo(tipoEquipamientoEspacioAlPrincipoYAlFinal));
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


