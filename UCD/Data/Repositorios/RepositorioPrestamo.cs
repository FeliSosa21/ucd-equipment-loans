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
        _memoria.Prestamos.Add(prestamo);
    }

    public Prestamo? ObtenerPrestamoPorId(Guid id)
    {
        return _memoria.Prestamos.FirstOrDefault(prestamo => prestamo.Id == id);
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamosActivos()
    {
        return _memoria.Prestamos
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Activo)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamosDeSolicitante(string dni)
    {
        return _memoria.Prestamos
            .Where(prestamo => prestamo.Solicitante.Dni == dni)
            .ToList()
            .AsReadOnly();
    }
}