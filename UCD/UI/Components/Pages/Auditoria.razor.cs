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
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarHistorico))
        {
            return;
        }

        try
        {
            _movimientos = ServicioHistorico.ListarMovimientos(ObtenerIdCatalogoProvisorio(),
                new FiltroMovimientosDto(), DniUsuario);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _movimientos = new List<MovimientoDto>();
        }
    }

    // Provisorio hasta que exista el selector de catalogo (T4): el primer catalogo activo.
    private int ObtenerIdCatalogoProvisorio()
    {
        List<CatalogoDto> catalogos = ServicioCatalogo.ListarCatalogos(DniUsuario);
        if (catalogos.Count == 0)
        {
            throw new ServicioException("No hay catalogos disponibles");
        }

        return catalogos[0].Id;
    }

    private static string TraducirTipo(string tipo) => NombresDeTipo.GetValueOrDefault(tipo, tipo);
}
