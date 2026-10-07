using Data;
using Data.Repositorios;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioHistoricoTests
{
    [TestMethod]
    public void ListarMovimientos_SinMovimientos_DevuelveListaVacia()
    {
        ServicioHistorico servicio = CrearServicio(new Memoria());

        List<MovimientoDto> movimientos = servicio.ListarMovimientos();

        Assert.AreEqual(0, movimientos.Count);
    }

    private static ServicioHistorico CrearServicio(Memoria memoria)
    {
        return new ServicioHistorico(new RepositorioMovimiento(memoria));
    }
}
