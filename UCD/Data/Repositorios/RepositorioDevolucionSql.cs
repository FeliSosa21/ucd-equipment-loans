using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioDevolucionSql
{
    private const string CampoRegistro = "_registro";
    private const string CampoUnidades = "_unidades";
    private const string ColumnaCatalogo = "CatalogoId";

    private readonly SqlContext _contexto;

    public RepositorioDevolucionSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarDevolucion(Devolucion devolucion)
    {
        _contexto.Devoluciones.Add(devolucion);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Devolucion> ObtenerPaginaDeDevoluciones(FiltroPorCatalogoYFechas filtro, int pagina,
        int tamanoPagina)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return FiltrarPor(filtro)
            .Include(devolucion => devolucion.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(devolucion => devolucion.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento)
            .Include($"{CampoRegistro}.{nameof(RegistroDeAccion.Responsable)}")
            .Include($"{CampoUnidades}.{nameof(UnidadDevuelta.Unidad)}")
            .OrderBy(devolucion => EF.Property<RegistroDeAccion>(devolucion, CampoRegistro).FechaHora)
            .ThenBy(devolucion => devolucion.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }

    public int ContarDevoluciones(FiltroPorCatalogoYFechas filtro)
    {
        return FiltrarPor(filtro).Count();
    }

    private IQueryable<Devolucion> FiltrarPor(FiltroPorCatalogoYFechas filtro)
    {
        IQueryable<Devolucion> devoluciones = _contexto.Devoluciones;

        if (filtro.IdCatalogo is not null)
        {
            devoluciones = devoluciones.Where(devolucion =>
                EF.Property<int>(devolucion.Prestamo.TipoEquipamiento, ColumnaCatalogo) == filtro.IdCatalogo);
        }

        if (filtro.Desde is not null)
        {
            devoluciones = devoluciones.Where(devolucion =>
                EF.Property<RegistroDeAccion>(devolucion, CampoRegistro).FechaHora >= filtro.Desde);
        }

        if (filtro.Hasta is not null)
        {
            devoluciones = devoluciones.Where(devolucion =>
                EF.Property<RegistroDeAccion>(devolucion, CampoRegistro).FechaHora < filtro.Hasta);
        }

        return devoluciones;
    }
}
