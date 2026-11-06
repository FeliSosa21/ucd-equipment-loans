using Dominio;
using Microsoft.EntityFrameworkCore;

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
        return _contexto.Prestamos
            .Include(prestamo => prestamo.Solicitante)
            .Include(prestamo => prestamo.Evaluador)
            .Include(prestamo => prestamo.TipoEquipamiento)
            .ThenInclude(tipo => tipo.Unidades)
            .Include(prestamo => prestamo.UnidadesAsignadas)
            .FirstOrDefault(prestamo => prestamo.Id == id);
    }

    public void ActualizarPrestamo(Prestamo prestamo)
    {
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Prestamo> ObtenerPrestamos(FiltroPorCatalogoYFechas filtro)
    {
        return _contexto.Prestamos.ToList();
    }
}