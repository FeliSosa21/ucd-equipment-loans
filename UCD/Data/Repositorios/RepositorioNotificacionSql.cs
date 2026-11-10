using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioNotificacionSql
{
    private readonly SqlContext _contexto;

    public RepositorioNotificacionSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarNotificacion(Notificacion notificacion)
    {
        _contexto.Notificaciones.Add(notificacion);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Notificacion> ObtenerNoLeidasDeUsuario(string dni)
    {
        return _contexto.Notificaciones
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento)
            .Where(notificacion => notificacion.Prestamo.Solicitante.Dni == dni)
            .Where(notificacion => !notificacion.EstaLeida)
            .OrderByDescending(notificacion => notificacion.FechaHora)
            .ThenByDescending(notificacion => notificacion.Id)
            .ToList();
    }

    public bool ExisteVencidoSinLeer(Guid idPrestamo)
    {
        return _contexto.Notificaciones.Any(notificacion => notificacion.Prestamo.Id == idPrestamo
                                                           && !notificacion.EstaLeida
                                                           && notificacion.Tipo == TipoNotificacion.PrestamoVencido);
    }

    public Notificacion? ObtenerNotificacionPorId(int id)
    {
        return _contexto.Notificaciones
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento)
            .FirstOrDefault(notificacion => notificacion.Id == id);
    }
}
