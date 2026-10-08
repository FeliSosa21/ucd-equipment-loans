using DTOs;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Entregas
{
    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();
    private string? _exito;
    private string? _avisoLista;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.RealizarPrestamo))
        {
            return;
        }

        Recargar();
    }

    private void RegistrarEntrega(PrestamoDto prestamo)
    {
        // Un doble clic manda dos envios: el segundo llega con el prestamo ya fuera de la lista.
        if (!EstaPorEntregar(prestamo))
        {
            return;
        }

        _exito = null;
        _avisoLista = null;

        try
        {
            InformarEntrega(ServicioPrestamo.RegistrarEntrega(prestamo.Id, DniUsuario));
        }
        catch (ServicioException ex)
        {
            _avisoLista = ex.Message;
        }

        Recargar();
    }

    private void InformarEntrega(PrestamoDto resultado)
    {
        _exito = $"Se registró la entrega del préstamo de {resultado.TipoEquipamiento} de {resultado.Solicitante}. " +
                 $"Unidades: {string.Join(", ", resultado.Unidades)}.";
    }

    private bool EstaPorEntregar(PrestamoDto prestamo) =>
        _prestamos.Any(porEntregar => porEntregar.Id == prestamo.Id);

    // Los listados no reciben quien los pide: el permiso lo controla la pagina al abrirse.
    private void Recargar()
    {
        _prestamos = ServicioPrestamo.ListarPrestamosPorEntregar();
    }
}
