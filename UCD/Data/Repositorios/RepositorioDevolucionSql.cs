using Dominio;
using Microsoft.EntityFrameworkCore;
using static Data.Configuraciones.NombresDeMapeo;

namespace Data.Repositorios;

public class RepositorioDevolucionSql
{
    private const string CampoUnidades = "_unidades";

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

    public IReadOnlyList<Devolucion> ObtenerDevoluciones(FiltroPorCatalogoYFechas filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return FiltrarPor(DevolucionesConSusDatos(), filtro)
            .OrderBy(devolucion => EF.Property<RegistroDeAccion>(devolucion, CampoRegistro).FechaHora)
            .ThenBy(devolucion => devolucion.Id)
            .ToList();
    }

    // Los Include van antes del filtro: filtrar primero por la fecha del registro owned hace que EF no cargue
    // su responsable.
    private IQueryable<Devolucion> DevolucionesConSusDatos()
    {
        return _contexto.Devoluciones
            .Include(devolucion => devolucion.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(devolucion => devolucion.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento)
            .Include($"{CampoRegistro}.{nameof(RegistroDeAccion.Responsable)}")
            .Include($"{CampoUnidades}.{nameof(UnidadDevuelta.Unidad)}");
    }

    private static IQueryable<Devolucion> FiltrarPor(IQueryable<Devolucion> devoluciones,
        FiltroPorCatalogoYFechas filtro)
    {
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
