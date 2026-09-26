namespace Dominio.Tests;

[TestClass]
public sealed class UnidadFisicaTests
{
    [TestMethod]
    public void Constructor_DatosValidos_GuardaElIdInstitucional()
    {
        var unidad = CrearUnidad(idInstitucional: "EQP-0042");

        Assert.AreEqual("EQP-0042", unidad.IdInstitucional);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaUbicacionFisica()
    {
        var unidad = CrearUnidad(ubiFisica: "Laboratorio Prueba");

        Assert.AreEqual("Laboratorio Prueba", unidad.UbiFisica);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaElNumeroDeSerie()
    {
        var unidad = CrearUnidad(numSerie: "PF3A7X21");

        Assert.AreEqual("PF3A7X21", unidad.NumSerie);
    }

    [TestMethod]
    public void Constructor_DatosValidos_QuedaDisponible()
    {
        var unidad = CrearUnidad();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    public void Constructor_UbiFisicaVacia_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearUnidad(ubiFisica: ""));
    }

    [TestMethod]
    public void Constructor_UbiFisicaMuyCorta_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearUnidad(ubiFisica: "AA"));
    }

    [TestMethod]
    public void Constructor_UbiFisicaMuyLarga_LanzaDominioException()
    {
        var ubiFisica = new string('a', 81);

        Assert.ThrowsException<DominioException>(() => CrearUnidad(ubiFisica: ubiFisica));
    }

    [TestMethod]
    public void Constructor_IdInstitucionalMalFormato_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearUnidad(idInstitucional: "Eq-0a2"));
    }

    [TestMethod]
    public void ModificarInfo_InfoValida_ActualizaLaUbicacionFisica()
    {
        var unidad = CrearUnidad(ubiFisica: "Laboratorio Viejo");

        unidad.ModificarInfo("Laboratorio Nuevo", "");

        Assert.AreEqual("Laboratorio Nuevo", unidad.UbiFisica);
    }

    [TestMethod]
    public void ModificarInfo_InfoValida_ActualizaLasObservaciones()
    {
        var unidad = CrearUnidad();

        unidad.ModificarInfo("Laboratorio Nuevo", "");

        Assert.AreEqual("", unidad.Observaciones);
    }

    [TestMethod]
    public void EstaDisponible_UnidadRecienCreada_DevuelveTrue()
    {
        var unidad = CrearUnidad();

        Assert.IsTrue(unidad.EstaDisponible());
    }

    [TestMethod]
    public void Prestada_UnidadDisponible_CambiaEstadoAPrestada()
    {
        var unidad = CrearUnidad();

        unidad.Prestada();

        Assert.AreEqual(EstadoUnidadFisica.Prestada, unidad.Estado);
    }

    [TestMethod]
    public void Prestada_UnidadNoDisponible_LanzaDominioException()
    {
        var unidad = CrearUnidad();
        unidad.Prestada();

        Assert.ThrowsException<DominioException>(() => unidad.Prestada());
    }

    [TestMethod]
    public void Disponible_UnidadPrestada_CambiaEstadoADisponible()
    {
        var unidad = CrearUnidad();
        unidad.Prestada();

        unidad.Disponible();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    public void Disponible_UnidadYaDisponible_LanzaDominioException()
    {
        var unidad = CrearUnidad();

        Assert.ThrowsException<DominioException>(() => unidad.Disponible());
    }

    [TestMethod]
    public void Disponible_UnidadEnMantenimiento_CambiaEstadoADisponible()
    {
        var unidad = CrearUnidad();
        unidad.Prestada();
        unidad.AMantenimiento();

        unidad.Disponible();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    public void AMantenimiento_UnidadNoPrestada_LanzaDominioException()
    {
        var unidad = CrearUnidad();

        Assert.ThrowsException<DominioException>(() => unidad.AMantenimiento());
    }

    [TestMethod]
    public void DarDeBaja_UnidadDisponible_CambiaEstadoADadaDeBaja()
    {
        var unidad = CrearUnidad();

        unidad.DarDeBaja();

        Assert.AreEqual(EstadoUnidadFisica.DadaDeBaja, unidad.Estado);
    }

    [TestMethod]
    public void DarDeBaja_UnidadPrestada_LanzaDominioException()
    {
        var unidad = CrearUnidad();
        unidad.Prestada();

        Assert.ThrowsException<DominioException>(() => unidad.DarDeBaja());
    }

    [TestMethod]
    public void Disponible_UnidadDadaDeBaja_LanzaDominioException()
    {
        var unidad = CrearUnidad();
        unidad.DarDeBaja();

        Assert.ThrowsException<DominioException>(() => unidad.Disponible());
    }

    [TestMethod]
    public void Constructor_SinNumeroDeSerie_QuedaSinNumeroDeSerie()
    {
        var unidad = CrearUnidad(numSerie: null);
        Assert.IsNull(unidad.NumSerie);
    }

    [TestMethod]
    public void Constructor_NumSerieMuyLargo_LanzaDominioException()
    {
        var numSerie = new string('A', 51);
        
        Assert.ThrowsException<DominioException>(() => CrearUnidad(numSerie: numSerie));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesMuyLargas_LanzaDominioException()
    {
        var unidad = CrearUnidad();
        var ubiFisica = "Laboratorio Prueba";
        var observaciones = new string('B', 301);
        
        Assert.ThrowsException<DominioException>(() => unidad.ModificarInfo(ubiFisica, observaciones));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesNulas_QuedaSinObservaciones()
    {
        var unidad = CrearUnidad();
        var ubiFisica = "Laboratorio Prueba";
        unidad.ModificarInfo(ubiFisica, "Pantalla rayada");
        
        unidad.ModificarInfo(ubiFisica, null);
        
        Assert.IsNull(unidad.Observaciones);
    }

    private static UnidadFisica CrearUnidad(
        string idInstitucional = "EQP-0042",
        string ubiFisica = "Laboratorio Prueba",
        string? numSerie = "PF3A7X21")
    {
        return new UnidadFisica(idInstitucional, ubiFisica, numSerie);
    }
}
