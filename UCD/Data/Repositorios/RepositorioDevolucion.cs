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

    public IReadOnlyList<Devolucion> ObtenerTodas()
    {
        return _memoria.Devoluciones.AsReadOnly();
    }
}
