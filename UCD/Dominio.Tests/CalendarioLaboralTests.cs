namespace Dominio.Tests;

[TestClass]
public sealed class CalendarioLaboralTests
{
    [TestMethod]
    public void SumarDiasHabiles_DiaDeSemanaSinFeriados_DevuelveElDiaHabilSiguiente()
    {
        DateTime desde = new DateTime(2026, 10, 5, 10, 0, 0);
        DateTime fechaEsperada = new DateTime(2026, 10, 6);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: 1);

        Assert.AreEqual(fechaEsperada, resultado);
    }
}