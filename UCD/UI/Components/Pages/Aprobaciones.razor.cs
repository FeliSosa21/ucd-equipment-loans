using DTOs;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Aprobaciones
{
    private const string FormatoFecha = "dd/MM/yyyy HH:mm";
    private const string FormatoFechaEsperada = "dd/MM/yyyy";
    private const int LargoMaximoMotivo = 300;

    // Estado del prestamo que devuelve el servicio cuando lo pudo aprobar.
    private const string EstadoAprobado = "Aprobado";

    private List<PrestamoDto> _pendientes = new List<PrestamoDto>();
    private List<PrestamoDto> _aprobados = new List<PrestamoDto>();
    private PrestamoDto? _rechazando;
    private string _motivo = string.Empty;
    private string? _error;
    private string? _exito;
    private string? _avisoLista;

    private bool MotivoCompleto => !string.IsNullOrWhiteSpace(_motivo);

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.EvaluarSolicitud))
        {
            return;
        }

        Recargar();
    }

    private void Aprobar(PrestamoDto prestamo)
    {
        // Un doble clic manda dos envios: el segundo llega con la solicitud ya fuera de la lista.
        if (!EstaPendiente(prestamo))
        {
            return;
        }

        _rechazando = null;
        LimpiarMensajes();

        try
        {
            InformarAprobacion(ServicioPrestamo.Aprobar(prestamo.Id, DniUsuario));
        }
        catch (ServicioException ex)
        {
            _avisoLista = ex.Message;
        }

        Recargar();
    }

    // Si ya no hay unidades suficientes, el servicio no aprueba: deja el prestamo rechazado con el motivo.
    private void InformarAprobacion(PrestamoDto resultado)
    {
        if (resultado.Estado == EstadoAprobado)
        {
            _exito = $"Se aprobó el préstamo de {resultado.TipoEquipamiento} de {resultado.Solicitante}. " +
                     $"Unidades asignadas: {string.Join(", ", resultado.Unidades)}.";
            return;
        }

        _avisoLista = $"No se pudo aprobar el préstamo de {resultado.TipoEquipamiento} de {resultado.Solicitante}: " +
                      $"{resultado.MotivoRechazo}.";
    }

    private void AbrirRechazo(PrestamoDto prestamo)
    {
        _rechazando = prestamo;
        _motivo = string.Empty;
        LimpiarMensajes();
    }

    private void CancelarRechazo()
    {
        _rechazando = null;
        _error = null;
    }

    private void ConfirmarRechazo()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (_rechazando is null || !MotivoCompleto)
        {
            return;
        }

        PrestamoDto prestamo = _rechazando;

        try
        {
            ServicioPrestamo.Rechazar(prestamo.Id, _motivo.Trim(), DniUsuario);
            _exito = $"Se rechazó el préstamo de {prestamo.TipoEquipamiento} de {prestamo.Solicitante}.";
            _rechazando = null;
            Recargar();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            Recargar();
            CerrarSiYaNoSePuedeRechazar(prestamo);
        }
    }

    // Si otro evaluador lo resolvio mientras tanto, se cierra el formulario y se avisa arriba de las listas.
    private void CerrarSiYaNoSePuedeRechazar(PrestamoDto prestamo)
    {
        if (SePuedeRechazar(prestamo))
        {
            return;
        }

        _rechazando = null;
        _error = null;
        _avisoLista = $"El préstamo de {prestamo.TipoEquipamiento} de {prestamo.Solicitante} ya no está pendiente " +
                      "ni aprobado; las listas se actualizaron.";
    }

    private bool EstaPendiente(PrestamoDto prestamo) =>
        _pendientes.Any(pendiente => pendiente.Id == prestamo.Id);

    private bool SePuedeRechazar(PrestamoDto prestamo) =>
        EstaPendiente(prestamo) || _aprobados.Any(aprobado => aprobado.Id == prestamo.Id);

    // Al abrir un formulario o hacer otra accion se borran los mensajes de la accion anterior.
    private void LimpiarMensajes()
    {
        _error = null;
        _exito = null;
        _avisoLista = null;
    }

    // Los listados no reciben quien los pide: el permiso lo controla la pagina al abrirse.
    private void Recargar()
    {
        _pendientes = ServicioPrestamo.ListarSolicitudesPendientes();
        _aprobados = ServicioPrestamo.ListarPrestamosAprobados();
    }
}
