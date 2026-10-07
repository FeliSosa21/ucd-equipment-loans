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

    [TestMethod]
    [DataRow(2029, 12, 31, 1, 2030, 1, 2)]
    [DataRow(2029, 4, 30, 1, 2029, 5, 2)]
    [DataRow(2029, 7, 17, 1, 2029, 7, 19)]
    [DataRow(2026, 8, 24, 1, 2026, 8, 26)]
    [DataRow(2029, 12, 24, 1, 2029, 12, 26)]
    public void SumarDiasHabiles_FeriadoNoLaborable_LoSalta(
        int anio, int mes, int dia, int diasHabiles, int anioEsperado, int mesEsperado, int diaEsperado)
    {
        DateTime desde = new DateTime(anio, mes, dia);
        DateTime fechaEsperada = new DateTime(anioEsperado, mesEsperado, diaEsperado);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: diasHabiles);

        Assert.AreEqual(fechaEsperada, resultado);
    }

    [TestMethod]
    [DataRow(2026, 12, 24, 2, 2026, 12, 29)]
    [DataRow(2026, 12, 31, 1, 2027, 1, 4)]
    public void SumarDiasHabiles_FeriadoJuntoAFinDeSemana_SaltaAmbos(
        int anio, int mes, int dia, int diasHabiles, int anioEsperado, int mesEsperado, int diaEsperado)
    {
        DateTime desde = new DateTime(anio, mes, dia);
        DateTime fechaEsperada = new DateTime(anioEsperado, mesEsperado, diaEsperado);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: diasHabiles);

        Assert.AreEqual(fechaEsperada, resultado);
    }

    [TestMethod]
    public void SumarDiasHabiles_FeriadoLaborable_CuentaComoHabil()
    {
        DateTime desde = new DateTime(2026, 10, 8);
        DateTime fechaEsperada = new DateTime(2026, 10, 15);

        DateTime resultado = CalendarioLaboral.SumarDiasHabiles(desde: desde, diasHabiles: 5);

        Assert.AreEqual(fechaEsperada, resultado);
    }
}