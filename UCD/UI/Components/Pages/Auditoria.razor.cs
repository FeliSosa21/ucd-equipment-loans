using DTOs;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Auditoria
{
    private const string SinDato = "—";
    private const string TodosLosTipos = "";

    // Tipo que devuelve el servicio (el nombre del enum) y como se muestra, con tildes. Tambien arma el filtro.
    private static readonly Dictionary<string, string> NombresDeTipo = new Dictionary<string, string>
    {
        { "Solicitud", "Solicitud" },
        { "Aprobacion", "Aprobación" },
        { "Rechazo", "Rechazo" },
        { "Prestamo", "Préstamo" },
        { "Devolucion", "Devolución" }
    };

    private List<MovimientoDto> _movimientos = new List<MovimientoDto>();
    private string _tipo = TodosLosTipos;
    private string? _error;

    private IEnumerable<MovimientoDto> MovimientosFiltrados =>
        _movimientos.Where(movimiento => _tipo == TodosLosTipos || movimiento.Tipo == _tipo);

    protected override void OnInitialized()
    {
        RedirigirSiNoTienePermiso(Permisos.ConsultarHistorico);
    }

    private async Task AlCambiarCatalogoAsync()
    {
        _error = null;
        if (!HayCatalogoElegido())
        {
            _movimientos = new List<MovimientoDto>();
            return;
        }

        await CargarMovimientosAsync();
    }

    private async Task CargarMovimientosAsync()
    {
        try
        {
            _movimientos = ServicioHistorico.ListarMovimientos(IdCatalogo, new FiltroMovimientosDto(), DniUsuario);
        }
        catch (ServicioException ex)
        {
            _movimientos = new List<MovimientoDto>();
            await RecargarSelectorAsync();
            _error = ex.Message;
        }
    }

    private static string TraducirTipo(string tipo) => NombresDeTipo.GetValueOrDefault(tipo, tipo);
}
