using DTOs;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Prestamos
{
    private const string FormatoFecha = "dd/MM/yyyy HH:mm";
    private const string FormatoFechaEsperada = "dd/MM/yyyy";
    private const string SinDato = "—";
    private const string TodosLosEstados = "";

    // Un prestamo rechazado nunca empezo: no tiene devolucion esperada que mostrar.
    private const string EstadoRechazado = "Rechazado";

    // Estados tal como los devuelve el servicio; tambien arman el filtro.
    private static readonly string[] Estados = { "Solicitado", "Activo", "Rechazado", "Devuelto" };

    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();
    private string _estado = TodosLosEstados;

    private IEnumerable<PrestamoDto> PrestamosFiltrados =>
        _prestamos.Where(prestamo => _estado == TodosLosEstados || prestamo.Estado == _estado);

    // El listado no recibe quien lo pide: el permiso lo controla la pagina al abrirse.
    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarPrestamos))
        {
            return;
        }

        _prestamos = ServicioPrestamo.ListarPrestamos()
            .OrderByDescending(prestamo => prestamo.FechaHoraPrestamo)
            .ToList();
    }
}
