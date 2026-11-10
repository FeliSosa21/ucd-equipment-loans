using System.Diagnostics;
using System.Globalization;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Servicios;

public class ServicioNotificacion
{
    private const string MensajeFallaAlGuardarVencido =
        "No se pudo guardar la notificacion de vencido del prestamo {IdPrestamo}";
    private const string FormatoFecha = "dd/MM/yyyy";
    private const string MensajeNotificacionAjena = "La notificacion no es del usuario";

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
        return _notificaciones.ObtenerNoLeidasDeUsuario(dniUsuario).Select(CrearNotificacionDto).ToList();
    }

    public void MarcarComoLeida(int idNotificacion, string dniUsuario)
    {
        Notificacion? notificacion = _notificaciones.ObtenerNotificacionPorId(idNotificacion);

        if (notificacion is null || !notificacion.EsDe(dniUsuario))
        {
            throw new ServicioException(MensajeNotificacionAjena);
        }

        notificacion.MarcarComoLeida();
        _notificaciones.ActualizarNotificacion(notificacion);
    }

    public void RegistrarResultadoDeEvaluacion(Prestamo prestamo, DateTime fechaHoraEvaluacion)
    {
    }

    private static NotificacionDto CrearNotificacionDto(Notificacion notificacion)
    {
        return new NotificacionDto
        {
            Id = notificacion.Id,
            Texto = ArmarTexto(notificacion),
            FechaHora = notificacion.FechaHora,
            Tipo = notificacion.Tipo.ToString()
        };
    }

    private static string ArmarTexto(Notificacion notificacion)
    {
        Prestamo prestamo = notificacion.Prestamo;
        string tipoEquipamiento = prestamo.TipoEquipamiento.Nombre;
        string solicitud = $"Tu solicitud de {tipoEquipamiento} del {FormatearFecha(prestamo.FechaHoraPrestamo)}";

        return notificacion.Tipo switch
        {
            TipoNotificacion.SolicitudAprobada => $"{solicitud} fue aprobada.",
            TipoNotificacion.SolicitudRechazada => $"{solicitud} fue rechazada. Motivo: {prestamo.MotivoRechazo}",
            TipoNotificacion.PrestamoVencido => $"Tu préstamo de {tipoEquipamiento} venció: " +
                                                $"tenías que devolverlo el {FormatearFecha(prestamo.FechaEsperadaDevolucion)}.",
            _ => throw new UnreachableException($"Tipo de notificacion sin texto: {notificacion.Tipo}")
        };
    }

    private static string FormatearFecha(DateTime fecha)
    {
        return fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
    }
}
