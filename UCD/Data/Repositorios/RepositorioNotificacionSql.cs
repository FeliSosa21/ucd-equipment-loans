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

    // Los Include van antes de cualquier Where: filtrar antes de incluir puede dejar sin cargar lo incluido
    // (lo vio Felipe en InMemory con una propiedad owned).
    private IQueryable<Notificacion> NotificacionesConSolicitanteYTipo =>
        _contexto.Notificaciones
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.Solicitante)
            .Include(notificacion => notificacion.Prestamo)
            .ThenInclude(prestamo => prestamo.TipoEquipamiento);

    public void AgregarNotificacion(Notificacion notificacion)
    {
        _contexto.Notificaciones.Add(notificacion);

        try
        {
            _contexto.SaveChanges();
        }
        catch (DbUpdateException excepcion)
        {
            // Regla 5: quien llama atrapa el error y sigue usando este contexto; sin esto, el proximo guardado
            // reintentaria lo que fallo.
            _contexto.ChangeTracker.Clear();
            throw new InvalidOperationException("No se pudo guardar la notificacion", excepcion);
        }
    }

    public IReadOnlyList<Notificacion> ObtenerNoLeidasDeUsuario(string dni)
    {
        return NotificacionesConSolicitanteYTipo
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
        return NotificacionesConSolicitanteYTipo.FirstOrDefault(notificacion => notificacion.Id == id);
    }

    public void ActualizarNotificacion(Notificacion notificacion)
    {
        _contexto.SaveChanges();
    }
}
