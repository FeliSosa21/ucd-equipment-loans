using Dominio;

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
}
