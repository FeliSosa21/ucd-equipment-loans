using Dominio;

namespace Data.Repositorios;

public class RepositorioDevolucion
{
    private readonly Memoria _memoria;

    public RepositorioDevolucion(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarDevolucion(Devolucion devolucion)
    {
        _memoria.Devoluciones.Add(devolucion);
    }

    public IReadOnlyList<Devolucion> ObtenerDevoluciones(FiltroPorCatalogoYFechas filtro)
    {
        return _memoria.Devoluciones
            .Where(devolucion => filtro.Desde is null || devolucion.FechaHoraDevolucion >= filtro.Desde)
            .Where(devolucion => filtro.Hasta is null || devolucion.FechaHoraDevolucion < filtro.Hasta)
            .Where(devolucion => filtro.IdCatalogo is null || PerteneceAlCatalogo(devolucion, filtro.IdCatalogo.Value))
            .OrderBy(devolucion => devolucion.FechaHoraDevolucion)
            .ToList();
    }

    public IReadOnlyList<Devolucion> ObtenerTodas()
    {
        return _memoria.Devoluciones.AsReadOnly();
    }

    private bool PerteneceAlCatalogo(Devolucion devolucion, int idCatalogo)
    {
        Catalogo? catalogo = _memoria.Catalogo;

        return catalogo is not null
               && catalogo.Id == idCatalogo
               && catalogo.Tipos.Contains(devolucion.Prestamo.TipoEquipamiento);
    }
}
