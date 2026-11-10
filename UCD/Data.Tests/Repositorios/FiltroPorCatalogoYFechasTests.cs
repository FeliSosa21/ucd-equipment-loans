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
}
