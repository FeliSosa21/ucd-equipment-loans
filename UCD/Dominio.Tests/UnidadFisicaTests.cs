namespace Dominio.Tests;

[TestClass]
public sealed class UnidadFisicaTests
{
    [TestMethod]
    public void Constructor_DatosValidos_ActualizaLosDatos()
    {
        string id = "EQP-0042";
        string ubiFisica = "Laboratorio Prueba";
        string numSerie = "PF3A7X21";
        var unidadFisica = new UnidadFisica(id, ubiFisica, numSerie);

        Assert.AreEqual("EQP-0042", unidadFisica.IdInstitucional);
        Assert.AreEqual("Laboratorio Prueba", unidadFisica.UbiFisica);
        Assert.AreEqual("PF3A7X21", unidadFisica.NumSerie);
        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidadFisica.Estado);
    }
    
    [TestMethod]
    public void Constructor_UbiFisicaVacia_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            var unidadFisica = new UnidadFisica("EQP-0042", "", "PF3A7X21");
        });
    }
    
    [TestMethod]
    public void Constructor_UbiFisicaMuyCorta_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            var unidadFisica = new UnidadFisica("EQP-0042", "AA", "PF3A7X21");
        });
    }
    
    [TestMethod]
    public void Constructor_UbiFisicaMuyLarga_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            string ubiFisica = new string('a', 81);
            var unidadFisica = new UnidadFisica("EQP-0042", ubiFisica, "PF3A7X21");
        });
    }
    [TestMethod]
    public void Constructor_IdInstitucionalMalFormato_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
        {
            var unidadFisica = new UnidadFisica("Eq-0a2", "aaaaa", "PF3A7X21");
        });
    }
    
    [TestMethod]
    public void ModificarInfo_InfoValido_ActualizaInfo()
    {
        var unidadFisica = new UnidadFisica("EQP-0042", "Laboratorio Viejo", "PF3A7X21");

        unidadFisica.ModificarInfo("Laboratorio Nuevo","");

        Assert.AreEqual("Laboratorio Nuevo", unidadFisica.UbiFisica);
        Assert.AreEqual("", unidadFisica.Observaciones);
    }
    
    
    
}

