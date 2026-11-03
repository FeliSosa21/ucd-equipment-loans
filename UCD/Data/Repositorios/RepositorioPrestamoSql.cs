using Dominio;

namespace Data.Repositorios;

public class RepositorioPrestamoSql
{
    private readonly SqlContext _contexto;

    public RepositorioPrestamoSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarPrestamo(Prestamo prestamo)
    {
    }
}