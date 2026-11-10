using Dominio;

namespace Data.Repositorios;

public class RepositorioMovimiento
{
    private readonly Memoria _memoria;

    public RepositorioMovimiento(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarMovimiento(Movimiento movimiento)
    {
        ArgumentNullException.ThrowIfNull(movimiento);
        _memoria.Movimientos.Add(movimiento);
    }

    public IReadOnlyList<Movimiento> ObtenerMovimientos(FiltroPorCatalogoYFechas filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        if (filtro.IdCatalogo is null && filtro.Desde is null && filtro.Hasta is null)
        {
            return new List<Movimiento>();
        }

        // En memoria no hay Id: invertir el orden de registro y ordenar con OrderByDescending, que es estable,
        // deja primero el ultimo registrado en los empates, igual que el desempate por Id en la base.
        return Enumerable.Reverse(_memoria.Movimientos)
            .Where(movimiento => filtro.Desde is null || movimiento.FechaHora >= filtro.Desde)
            .Where(movimiento => filtro.Hasta is null || movimiento.FechaHora < filtro.Hasta)
            .Where(movimiento => filtro.IdCatalogo is null || PerteneceAlCatalogo(movimiento, filtro.IdCatalogo.Value))
            .OrderByDescending(movimiento => movimiento.FechaHora)
            .ToList();
    }

    private bool PerteneceAlCatalogo(Movimiento movimiento, int idCatalogo)
    {
        Catalogo? catalogo = _memoria.Catalogo;

        return catalogo is not null
               && catalogo.Id == idCatalogo
               && catalogo.Tipos.Contains(movimiento.Prestamo.TipoEquipamiento);
    }
}
