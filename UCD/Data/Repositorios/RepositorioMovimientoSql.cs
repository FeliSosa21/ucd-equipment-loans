using Dominio;
using Microsoft.EntityFrameworkCore;
using static Data.Configuraciones.NombresDeMapeo;

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

    public IReadOnlyList<Movimiento> ObtenerMovimientos(FiltroPorCatalogoYFechas filtro)
    {
        if (filtro.IdCatalogo is null && filtro.Desde is null && filtro.Hasta is null)
        {
            return new List<Movimiento>();
        }

        return FiltrarPor(filtro)
            .Include(movimiento => movimiento.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(movimiento => movimiento.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento)
            .Include(movimiento => movimiento.Evaluador)
            .Include(movimiento => movimiento.Unidad)
            .Include($"{CampoRegistro}.{nameof(RegistroDeAccion.Responsable)}")
            .OrderByDescending(movimiento => EF.Property<RegistroDeAccion>(movimiento, CampoRegistro).FechaHora)
            .ThenByDescending(movimiento => movimiento.Id)
            .ToList();
    }

    private IQueryable<Movimiento> FiltrarPor(FiltroPorCatalogoYFechas filtro)
    {
        IQueryable<Movimiento> movimientos = _contexto.Movimientos;

        if (filtro.IdCatalogo is not null)
        {
            movimientos = movimientos.Where(movimiento =>
                EF.Property<int>(movimiento.Prestamo.TipoEquipamiento, ColumnaCatalogo) == filtro.IdCatalogo);
        }

        if (filtro.Desde is not null)
        {
            movimientos = movimientos.Where(movimiento =>
                EF.Property<RegistroDeAccion>(movimiento, CampoRegistro).FechaHora >= filtro.Desde);
        }

        if (filtro.Hasta is not null)
        {
            movimientos = movimientos.Where(movimiento =>
                EF.Property<RegistroDeAccion>(movimiento, CampoRegistro).FechaHora < filtro.Hasta);
        }

        return movimientos;
    }
}
