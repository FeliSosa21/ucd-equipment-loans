using DTOs;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Devoluciones
{
    private List<PrestamoActivoDto> _prestamos = new List<PrestamoActivoDto>();
    private List<string> _condiciones = new List<string>();
    private List<UnidadDevueltaDto> _unidades = new List<UnidadDevueltaDto>();
    private PrestamoActivoDto? _seleccionado;
    private string _busqueda = string.Empty;
    private string? _error;
    private string? _exito;

    private IEnumerable<PrestamoActivoDto> PrestamosFiltrados =>
        _prestamos.Where(prestamo => string.IsNullOrWhiteSpace(_busqueda) ||
                                     $"{prestamo.Solicitante} {prestamo.TipoEquipamiento}"
                                         .Contains(_busqueda.Trim(), StringComparison.OrdinalIgnoreCase));

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.RegistrarDevolucion))
        {
            return;
        }

        _condiciones = ServicioDevolucion.CondicionesPosibles();
        Recargar();
    }

    private void Seleccionar(PrestamoActivoDto prestamo)
    {
        _seleccionado = prestamo;
        _error = null;
        _exito = null;
        _unidades = prestamo.Unidades
            .Select(id => new UnidadDevueltaDto { IdInstitucional = id, Condicion = _condiciones.First() })
            .ToList();
    }

    private void Cancelar()
    {
        _seleccionado = null;
        _error = null;
    }

    private void Confirmar()
    {
        PrestamoActivoDto prestamo = _seleccionado!;
        DevolucionDto datos = new DevolucionDto
        {
            IdPrestamo = prestamo.Id,
            FechaHoraDevolucion = DateTime.Now,
            Unidades = _unidades
        };

        try
        {
            ServicioDevolucion.RegistrarDevolucion(datos, SesionActual.Usuario!.Dni);
            _exito = $"Se registró la devolución de {prestamo.TipoEquipamiento} de {prestamo.Solicitante}.";
            _seleccionado = null;
            Recargar();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }

    private void Recargar()
    {
        _prestamos = ServicioDevolucion.ListarPrestamosActivos();
    }
}
