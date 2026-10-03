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
        _memoria.Movimientos.Add(movimiento);
    }

    public IReadOnlyList<Movimiento> ObtenerTodos()
    {
        return _memoria.Movimientos;
    }
}
