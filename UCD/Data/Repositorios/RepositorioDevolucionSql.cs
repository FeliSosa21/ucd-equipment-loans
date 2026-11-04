using Dominio;

namespace Data.Repositorios;

public class RepositorioDevolucionSql
{
    private readonly SqlContext _contexto;

    public RepositorioDevolucionSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarDevolucion(Devolucion devolucion)
    {
    }

    public IReadOnlyList<Devolucion> ObtenerDevoluciones(FiltroPorCatalogoYFechas filtro)
    {
        return new List<Devolucion>();
    }
}
