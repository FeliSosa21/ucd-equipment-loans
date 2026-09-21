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
    
    
    
}

