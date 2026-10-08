using DTOs;
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

    private IEnumerable<MovimientoDto> MovimientosFiltrados =>
        _movimientos.Where(movimiento => _tipo == TodosLosTipos || movimiento.Tipo == _tipo);

    // El listado no recibe quien lo pide: el permiso lo controla la pagina al abrirse.
    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarHistorico))
        {
            return;
        }

        _movimientos = ServicioHistorico.ListarMovimientos();
    }

    private static string TraducirTipo(string tipo) => NombresDeTipo.GetValueOrDefault(tipo, tipo);
}
