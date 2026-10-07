using DTOs;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Entregas
{
    private const string FormatoFecha = "dd/MM/yyyy HH:mm";
    private const string FormatoFechaEsperada = "dd/MM/yyyy";

    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();
    private string _busqueda = string.Empty;
    private string? _exito;
    private string? _avisoLista;

    private IEnumerable<PrestamoDto> PrestamosFiltrados =>
        _prestamos.Where(prestamo => string.IsNullOrWhiteSpace(_busqueda) ||
                                     $"{prestamo.Solicitante} {prestamo.TipoEquipamiento}"
                                         .Contains(_busqueda.Trim(), StringComparison.OrdinalIgnoreCase));

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.RealizarPrestamo))
        {
            return;
        }

        Recargar();
    }

    private void Entregar(PrestamoDto prestamo)
    {
        // Un doble clic manda dos envios: el segundo llega con el prestamo ya fuera de la lista.
        if (!_prestamos.Any(aprobado => aprobado.Id == prestamo.Id))
        {
            return;
        }

        _exito = null;
        _avisoLista = null;

        try
        {
            ServicioPrestamo.Realizar(prestamo.Id, DniUsuario);
            _exito = $"Préstamo entregado: quedó activo. {prestamo.TipoEquipamiento} de {prestamo.Solicitante}, " +
                     $"unidades {string.Join(", ", prestamo.Unidades)}.";
        }
        catch (ServicioException ex)
        {
            _avisoLista = ex.Message;
        }

        Recargar();
    }

    // Los listados no reciben quien los pide: el permiso lo controla la pagina al abrirse.
    private void Recargar()
    {
        _prestamos = ServicioPrestamo.ListarPrestamosAprobados();
    }
}
