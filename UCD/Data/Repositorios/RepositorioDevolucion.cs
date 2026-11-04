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
            .OrderBy(devolucion => devolucion.FechaHoraDevolucion)
            .ToList();
    }

    public IReadOnlyList<Devolucion> ObtenerTodas()
    {
        return _memoria.Devoluciones.AsReadOnly();
    }
}
