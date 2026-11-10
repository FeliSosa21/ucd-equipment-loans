using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Prestamos
{
    private const string SinDato = "—";
    private const string TodosLosEstados = "";
    private const string CampoDesde = nameof(FiltroDePrestamosDto.Desde);

    // Estados tal como los devuelve el servicio; tambien arman el filtro.
    private static readonly string[] Estados =
    {
        EstadosPrestamo.Solicitado, EstadosPrestamo.Activo, EstadosPrestamo.Rechazado, EstadosPrestamo.Devuelto
    };

    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();
    private DateTime _desde;
    private DateTime _hasta;
    private string _estado = TodosLosEstados;
    private readonly ErroresDeFormulario _errores = new ErroresDeFormulario(CampoDesde);

    private bool HayError => _errores.Tiene(CampoDesde) || _errores.General is not null;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarPrestamos))
        {
            return;
        }

        FiltroDePrestamosDto rangoPorDefecto = ServicioPrestamo.RangoPorDefecto();
        _desde = rangoPorDefecto.Desde;
        _hasta = rangoPorDefecto.Hasta;
        Buscar();
    }

    // Si el rango no es valido o quien consulta perdio el permiso, el servicio lo niega y se muestra el motivo.
    private void Buscar()
    {
        _errores.Limpiar();
        FiltroDePrestamosDto filtro = new FiltroDePrestamosDto { Desde = _desde, Hasta = _hasta, Estado = _estado };

        try
        {
            _prestamos = ServicioPrestamo.ListarPrestamos(DniUsuario, filtro);
        }
        catch (ServicioException ex)
        {
            _prestamos = new List<PrestamoDto>();
            _errores.Agregar(ex);
        }
    }
}
