using Data.Repositorios;
using Microsoft.Extensions.Logging;

namespace Servicios;

public class ServicioNotificacion
{
    private readonly RepositorioNotificacionSql _notificaciones;
    private readonly RepositorioPrestamoSql _prestamos;
    private readonly TimeProvider _reloj;
    private readonly ILogger<ServicioNotificacion> _logger;

    public ServicioNotificacion(RepositorioNotificacionSql notificaciones, RepositorioPrestamoSql prestamos,
        TimeProvider reloj, ILogger<ServicioNotificacion> logger)
    {
        _notificaciones = notificaciones;
        _prestamos = prestamos;
        _reloj = reloj;
        _logger = logger;
    }

    public void GenerarNotificacionesDeVencidos(string dniUsuario)
    {
    }
}
