using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Servicios;

public class ServicioNotificacion
{
    private const string MensajeFallaAlGuardarVencido =
        "No se pudo guardar la notificacion de vencido del prestamo {IdPrestamo}";

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
        DateTimeOffset ahora = _reloj.GetLocalNow();

        foreach (Prestamo prestamo in _prestamos.ObtenerPrestamosVencidosDeSolicitante(dniUsuario, ahora.Date))
        {
            if (_notificaciones.ExisteVencidoSinLeer(prestamo.Id))
            {
                continue;
            }

            Notificacion notificacion = new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, ahora.DateTime);

            try
            {
                _notificaciones.AgregarNotificacion(notificacion);
            }
            catch (InvalidOperationException excepcion)
            {
                _logger.LogError(excepcion, MensajeFallaAlGuardarVencido, prestamo.Id);

                // El repositorio vacio el seguimiento: seguir agregaria de nuevo los prestamos ya cargados.
                return;
            }
        }
    }

    public List<NotificacionDto> ListarNoLeidas(string dniUsuario)
    {
        return new List<NotificacionDto>();
    }
}
