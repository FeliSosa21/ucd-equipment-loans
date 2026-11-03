using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioMovimientoSql
{
    private readonly SqlContext _contexto;

    public RepositorioMovimientoSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarMovimiento(Movimiento movimiento)
    {
        _contexto.Movimientos.Add(movimiento);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Movimiento> ObtenerPaginaDeMovimientos(FiltroMovimientos filtro, int pagina, int tamanoPagina)
    {
        IQueryable<Movimiento> movimientos = _contexto.Movimientos;

        if (filtro.IdCatalogo is not null)
        {
            movimientos = movimientos.Where(movimiento =>
                EF.Property<int>(movimiento.Prestamo.TipoEquipamiento, "CatalogoId") == filtro.IdCatalogo);
        }

        return movimientos
            .OrderByDescending(movimiento => EF.Property<RegistroDeAccion>(movimiento, "_registro").FechaHora)
            .ThenByDescending(movimiento => movimiento.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }
}
