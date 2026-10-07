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

    [TestMethod]
    [DataRow(2026, 10, 9, 1, 2026, 10, 12)]
    [DataRow(2026, 10, 9, 3, 2026, 10, 14)]
    public void SumarDiasHabiles_CruzaUnFinDeSemana_LoSalta(
        int anio, int mes, int dia, int diasHabiles, int anioEsperado, int mesEsperado, int diaEsperado)
    {
        DateTime desde = new DateTime(anio, mes, dia);
        DateTime fechaEsperada = new DateTime(anioEsperado, mesEsperado, diaEsperado);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: diasHabiles);

        Assert.AreEqual(fechaEsperada, resultado);
    }

    [TestMethod]
    [DataRow(2026, 10, 10, 1, 2026, 10, 12)]
    [DataRow(2026, 10, 11, 1, 2026, 10, 12)]
    public void SumarDiasHabiles_EmpiezaEnFinDeSemana_CuentaDesdeElLunes(
        int anio, int mes, int dia, int diasHabiles, int anioEsperado, int mesEsperado, int diaEsperado)
    {
        DateTime desde = new DateTime(anio, mes, dia);
        DateTime fechaEsperada = new DateTime(anioEsperado, mesEsperado, diaEsperado);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: diasHabiles);

        Assert.AreEqual(fechaEsperada, resultado);
    }
}