using Dominio;

namespace Data.Repositorios;

public class RepositorioPrestamo
{
    private readonly Memoria _memoria;
    
    public RepositorioPrestamo(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarPrestamo(Prestamo prestamo)
    {
        
    }

    public Prestamo ObtenerPrestamoPorId(Guid id)
    {
        return null;
    }
}