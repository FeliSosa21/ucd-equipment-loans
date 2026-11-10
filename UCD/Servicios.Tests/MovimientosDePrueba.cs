using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

internal static class MovimientosDePrueba
{
    // Con un filtro sin criterios el repositorio no devuelve nada: Desde = MinValue abarca todo lo registrado.
    public static IReadOnlyList<Movimiento> Registrados(Memoria memoria)
    {
        FiltroPorCatalogoYFechas desdeSiempre = new FiltroPorCatalogoYFechas { Desde = DateTime.MinValue };

        return new RepositorioMovimiento(memoria).ObtenerMovimientos(desdeSiempre);
    }
}
