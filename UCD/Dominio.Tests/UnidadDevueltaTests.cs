namespace Dominio.Tests;

[TestClass]
public sealed class UnidadDevueltaTests
{
    [TestMethod]
    public void Constructor_DatosValidos_GuardaLosDatos()
    {
        UnidadFisica unidad = CrearUnidadPrestada();

        UnidadDevuelta devuelta = new UnidadDevuelta(unidad, CondicionDevolucion.ConFallas, "Pantalla rayada");

        Assert.AreSame(unidad, devuelta.Unidad);
        Assert.AreEqual(CondicionDevolucion.ConFallas, devuelta.Condicion);
        Assert.AreEqual("Pantalla rayada", devuelta.Observaciones);
    }

    [TestMethod]
    public void Constructor_SinObservaciones_LasDejaNulas()
    {
        UnidadDevuelta devuelta = new UnidadDevuelta(CrearUnidadPrestada(), CondicionDevolucion.SinObservaciones,
            null);

        Assert.IsNull(devuelta.Observaciones);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_ObservacionesEnBlanco_LasDejaNulas(string observaciones)
    {
        UnidadDevuelta devuelta = new UnidadDevuelta(CrearUnidadPrestada(), CondicionDevolucion.ConFallas,
            observaciones);

        Assert.IsNull(devuelta.Observaciones);
    }

    [TestMethod]
    public void Constructor_ObservacionesConEspaciosAlBorde_LasRecorta()
    {
        UnidadDevuelta devuelta = new UnidadDevuelta(CrearUnidadPrestada(), CondicionDevolucion.ConFallas,
            "  Pantalla rayada  ");

        Assert.AreEqual("Pantalla rayada", devuelta.Observaciones);
    }

    [TestMethod]
    public void Constructor_UnidadNula_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            new UnidadDevuelta(null!, CondicionDevolucion.SinObservaciones, null));
    }

    [TestMethod]
    [DataRow(301)]
    [DataRow(400)]
    public void Constructor_ObservacionesDeMasDe300Caracteres_LanzaExcepcion(int largo)
    {
        string observaciones = new string('a', largo);

        Assert.ThrowsException<DominioException>(() =>
            new UnidadDevuelta(CrearUnidadPrestada(), CondicionDevolucion.ConFallas, observaciones));
    }

    [TestMethod]
    public void Constructor_ObservacionesDe300Caracteres_SeCrea()
    {
        string observaciones = new string('a', 300);

        UnidadDevuelta devuelta = new UnidadDevuelta(CrearUnidadPrestada(), CondicionDevolucion.ConFallas,
            observaciones);

        Assert.AreEqual(observaciones, devuelta.Observaciones);
    }

    [TestMethod]
    public void ActualizarEstadoDeLaUnidad_SinObservaciones_QuedaDisponible()
    {
        UnidadFisica unidad = CrearUnidadPrestada();
        UnidadDevuelta devuelta = new UnidadDevuelta(unidad, CondicionDevolucion.SinObservaciones, null);

        devuelta.ActualizarEstadoDeLaUnidad();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
    }

    [TestMethod]
    [DataRow(CondicionDevolucion.ConFallas)]
    [DataRow(CondicionDevolucion.Daniada)]
    public void ActualizarEstadoDeLaUnidad_ConFallasODaniada_PasaAMantenimiento(CondicionDevolucion condicion)
    {
        UnidadFisica unidad = CrearUnidadPrestada();
        UnidadDevuelta devuelta = new UnidadDevuelta(unidad, condicion, null);

        devuelta.ActualizarEstadoDeLaUnidad();

        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, unidad.Estado);
    }

    private static UnidadFisica CrearUnidadPrestada()
    {
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0042", ubiFisica: "Deposito A",
            numSerie: "PF3A7X21");
        unidad.Prestada();
        return unidad;
    }
}
