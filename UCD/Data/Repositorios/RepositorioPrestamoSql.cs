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
        _contexto.Prestamos.Add(prestamo);
        _contexto.SaveChanges();
    }

    public Prestamo? ObtenerPrestamoPorId(Guid id)
    {
        return _contexto.Prestamos.FirstOrDefault(prestamo => prestamo.Id == id);
    }
}