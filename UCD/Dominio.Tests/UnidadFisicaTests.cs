namespace Dominio.Tests;

[TestClass]
public sealed class UnidadFisicaTests
{
    [TestMethod]
    public void Constructor_DatosValidos_GuardaElIdInstitucional()
    {
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0042");

        Assert.AreEqual("EQP-0042", unidad.IdInstitucional);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLaUbicacionFisica()
    {
        UnidadFisica unidad = CrearUnidad(ubiFisica: "Laboratorio Prueba");

        Assert.AreEqual("Laboratorio Prueba", unidad.UbiFisica);
    }

    [TestMethod]
    public void Constructor_DatosValidos_GuardaElNumeroDeSerie()
    {
        UnidadFisica unidad = CrearUnidad(numSerie: "PF3A7X21");

        Assert.AreEqual("PF3A7X21", unidad.NumSerie);
    }

    [TestMethod]
    public void Constructor_DatosValidos_QuedaDisponible()
    {
        UnidadFisica unidad = CrearUnidad();

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
        string ubiFisica = new string('a', 81);

        Assert.ThrowsException<DominioException>(() => CrearUnidad(ubiFisica: ubiFisica));
    }

    [TestMethod]
    public void Constructor_UbiFisicaSoloEspacios_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() => CrearUnidad(ubiFisica: "     "));
    }

    [DataTestMethod]
    [DataRow(3)]
    [DataRow(80)]
    public void Constructor_UbiFisicaEnElBorde_GuardaLaUbicacion(int largo)
    {
        string ubiFisica = new string('a', largo);

        UnidadFisica unidad = CrearUnidad(ubiFisica: ubiFisica);

        Assert.AreEqual(ubiFisica, unidad.UbiFisica);
    }

    [TestMethod]
    public void Constructor_NumSerieDe50Caracteres_GuardaElNumeroDeSerie()
    {
        string numSerie = new string('A', 50);

        UnidadFisica unidad = CrearUnidad(numSerie: numSerie);

        Assert.AreEqual(numSerie, unidad.NumSerie);
    }

    [DataTestMethod]
    [DataRow("Eq-0a2")]
    [DataRow("eqp-0042")]
    [DataRow("EQP0042")]
    [DataRow("EQ-00042")]
    [DataRow("EQP-004A")]
    [DataRow("EQP_0042")]
    [DataRow("EQP-00421")]
    [DataRow("ÁÉÍ-0042")]
    [DataRow("EQP-٠١٢٣")]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_IdInstitucionalMalFormato_LanzaDominioException(string idInstitucional)
    {
        Assert.ThrowsException<DominioException>(() => CrearUnidad(idInstitucional: idInstitucional));
    }

    [TestMethod]
    public void ModificarInfo_InfoValida_ActualizaLaUbicacionFisica()
    {
        UnidadFisica unidad = CrearUnidad(ubiFisica: "Laboratorio Viejo");

        unidad.ModificarInfo("Laboratorio Nuevo", "PF3A7X21", "");

        Assert.AreEqual("Laboratorio Nuevo", unidad.UbiFisica);
    }

    [TestMethod]
    public void ModificarInfo_InfoValida_ActualizaLasObservaciones()
    {
        UnidadFisica unidad = CrearUnidad();

        unidad.ModificarInfo("Laboratorio Nuevo", "PF3A7X21", "");

        Assert.AreEqual("", unidad.Observaciones);
    }

    [TestMethod]
    public void EstaDisponible_UnidadRecienCreada_DevuelveTrue()
    {
        UnidadFisica unidad = CrearUnidad();

        Assert.IsTrue(unidad.EstaDisponible());
    }

    [TestMethod]
    public void Prestada_UnidadDisponible_CambiaEstadoAPrestada()
    {
        UnidadFisica unidad = CrearUnidad();

        unidad.Prestada();

        Assert.AreEqual(EstadoUnidadFisica.Prestada, unidad.Estado);
    }

    [TestMethod]
    public void Prestada_UnidadNoDisponible_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.Prestada();

        Assert.ThrowsException<DominioException>(() => unidad.Prestada());
    }

    [TestMethod]
    public void Disponible_UnidadPrestada_CambiaEstadoADisponible()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.Prestada();

        unidad.Disponible();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    public void Disponible_UnidadYaDisponible_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();

        Assert.ThrowsException<DominioException>(() => unidad.Disponible());
    }

    [TestMethod]
    public void Disponible_UnidadEnMantenimiento_CambiaEstadoADisponible()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.Prestada();
        unidad.AMantenimiento();

        unidad.Disponible();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    public void DarDeBaja_UnidadDisponible_CambiaEstadoADadaDeBaja()
    {
        UnidadFisica unidad = CrearUnidad();

        unidad.DarDeBaja();

        Assert.AreEqual(EstadoUnidadFisica.DadaDeBaja, unidad.Estado);
    }

    [TestMethod]
    public void DarDeBaja_UnidadPrestada_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.Prestada();

        Assert.ThrowsException<DominioException>(() => unidad.DarDeBaja());
    }

    [TestMethod]
    public void DarDeBaja_UnidadEnMantenimiento_CambiaEstadoADadaDeBaja()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.AMantenimiento();

        unidad.DarDeBaja();

        Assert.AreEqual(EstadoUnidadFisica.DadaDeBaja, unidad.Estado);
    }

    [TestMethod]
    public void DarDeBaja_UnidadYaDadaDeBaja_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.DarDeBaja();

        Assert.ThrowsException<DominioException>(() => unidad.DarDeBaja());
    }

    [TestMethod]
    public void Disponible_UnidadDadaDeBaja_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.DarDeBaja();

        Assert.ThrowsException<DominioException>(() => unidad.Disponible());
    }

    [TestMethod]
    public void Constructor_SinNumeroDeSerie_QuedaSinNumeroDeSerie()
    {
        UnidadFisica unidad = CrearUnidad(numSerie: null);

        Assert.IsNull(unidad.NumSerie);
    }

    [TestMethod]
    public void Constructor_NumSerieMuyLargo_LanzaDominioException()
    {
        string numSerie = new string('A', 51);

        Assert.ThrowsException<DominioException>(() => CrearUnidad(numSerie: numSerie));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesMuyLargas_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        string ubiFisica = "Laboratorio Prueba";
        string observaciones = new string('B', 301);

        Assert.ThrowsException<DominioException>(() => unidad.ModificarInfo(ubiFisica, "PF3A7X21", observaciones));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesDe300Caracteres_GuardaLasObservaciones()
    {
        UnidadFisica unidad = CrearUnidad();
        string observaciones = new string('B', 300);

        unidad.ModificarInfo("Laboratorio Prueba", "PF3A7X21", observaciones);

        Assert.AreEqual(observaciones, unidad.Observaciones);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("AB")]
    public void ModificarInfo_UbiFisicaInvalida_LanzaDominioException(string ubiFisica)
    {
        UnidadFisica unidad = CrearUnidad();

        Assert.ThrowsException<DominioException>(() => unidad.ModificarInfo(ubiFisica, "PF3A7X21", null));
    }

    [TestMethod]
    public void ModificarInfo_UbiFisicaMuyLarga_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        string ubiFisica = new string('a', 81);

        Assert.ThrowsException<DominioException>(() => unidad.ModificarInfo(ubiFisica, "PF3A7X21", null));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesNulas_QuedaSinObservaciones()
    {
        UnidadFisica unidad = CrearUnidad();
        string ubiFisica = "Laboratorio Prueba";
        unidad.ModificarInfo(ubiFisica, "PF3A7X21", "Pantalla rayada");

        unidad.ModificarInfo(ubiFisica, "PF3A7X21", null);

        Assert.IsNull(unidad.Observaciones);
    }

    [TestMethod]
    public void ModificarInfo_NumSerieNuevo_ActualizaElNumSerie()
    {
        UnidadFisica unidad = CrearUnidad();
        string numSerieNuevo = "PF5A7J22";

        unidad.ModificarInfo("Laboratorio Prueba", numSerieNuevo, null);

        Assert.AreEqual(numSerieNuevo, unidad.NumSerie);
    }

    [TestMethod]
    public void ModificarInfo_NumSerieMuyLargo_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        string numSerieMuyLargo = new string('A', 51);

        Assert.ThrowsException<DominioException>(() =>
            unidad.ModificarInfo("Laboratorio Prueba", numSerieMuyLargo, null));
    }

    [TestMethod]
    public void ModificarInfo_ObservacionesInvalidas_NoModificaLaUbicacion()
    {
        UnidadFisica unidad = CrearUnidad(ubiFisica: "Laboratorio Viejo");
        string observacionesInvalidas = new string('B', 301);

        Assert.ThrowsException<DominioException>(
            () => unidad.ModificarInfo("Laboratorio Nuevo", "PF3A7X21", observacionesInvalidas));

        Assert.AreEqual("Laboratorio Viejo", unidad.UbiFisica);
    }

    [TestMethod]
    public void AMantenimiento_UnidadDisponible_CambiaEstadoAEnMantenimiento()
    {
        UnidadFisica unidad = CrearUnidad();

        unidad.AMantenimiento();

        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, unidad.Estado);
    }

    [TestMethod]
    public void AMantenimiento_UnidadYaEnMantenimiento_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.Prestada();
        unidad.AMantenimiento();

        Assert.ThrowsException<DominioException>(() => unidad.AMantenimiento());
    }

    [TestMethod]
    public void AMantenimiento_UnidadDadaDeBaja_LanzaDominioException()
    {
        UnidadFisica unidad = CrearUnidad();
        unidad.DarDeBaja();

        Assert.ThrowsException<DominioException>(() => unidad.AMantenimiento());
    }

    private static UnidadFisica CrearUnidad(
        string idInstitucional = "EQP-0042",
        string ubiFisica = "Laboratorio Prueba",
        string? numSerie = "PF3A7X21")
    {
        return new UnidadFisica(idInstitucional, ubiFisica, numSerie);
    }
}
