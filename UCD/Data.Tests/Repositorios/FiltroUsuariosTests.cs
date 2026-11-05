namespace Data.Tests.Repositorios;
using Data.Repositorios;

[TestClass]
public sealed class FiltroUsuariosTests
{
    [TestMethod]
    public void TieneCriterios_SinTextoNiTipo_DevuelveFalse()
    {
        FiltroUsuarios filtro = new FiltroUsuarios();

        
        bool tieneCriterios = filtro.TieneCriterios();
        
        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_ConTexto_DevuelveTrue()
    {
        FiltroUsuarios filtro = new FiltroUsuarios { Texto = "Lucia" };
        
        bool tieneCriterios = filtro.TieneCriterios();
        
        Assert.IsTrue(tieneCriterios);
    }
}