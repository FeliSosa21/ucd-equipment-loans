using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class FiltroPorCatalogoYFechasTests
{
    [TestMethod]
    public void TieneCriterios_SinCatalogoNiFechas_DevuelveFalse()
    {
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas();

        bool tieneCriterios = filtro.TieneCriterios();

        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_ConCatalogo_DevuelveTrue()
    {
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { IdCatalogo = 1 };

        bool tieneCriterios = filtro.TieneCriterios();

        Assert.IsTrue(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_ConDesde_DevuelveTrue()
    {
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Desde = new DateTime(2026, 11, 10) };

        bool tieneCriterios = filtro.TieneCriterios();

        Assert.IsTrue(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_ConHasta_DevuelveTrue()
    {
        FiltroPorCatalogoYFechas filtro = new FiltroPorCatalogoYFechas { Hasta = new DateTime(2026, 11, 10) };

        bool tieneCriterios = filtro.TieneCriterios();

        Assert.IsTrue(tieneCriterios);
    }
}
