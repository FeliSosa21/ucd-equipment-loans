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
    private DevolucionRegistradaDto? _registrada;
    private string? _avisoLista;

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
        _registrada = null;
        _avisoLista = null;
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
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (_seleccionado is null)
        {
            return;
        }

        PrestamoActivoDto prestamo = _seleccionado;
        DevolucionDto datos = new DevolucionDto
        {
            IdPrestamo = prestamo.Id,
            Unidades = _unidades
        };

        try
        {
            _registrada = ServicioDevolucion.RegistrarDevolucion(datos, DniUsuario);
            _exito = $"Se registró la devolución de {prestamo.TipoEquipamiento} de {prestamo.Solicitante}.";
            _seleccionado = null;
            Recargar();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            if (Recargar())
            {
                CerrarSiYaNoEstaActivo(prestamo);
            }
        }
    }

    // Si otro usuario lo devolvio mientras tanto, se cierra el formulario y se avisa arriba de la lista.
    private void CerrarSiYaNoEstaActivo(PrestamoActivoDto prestamo)
    {
        if (_prestamos.Any(activo => activo.Id == prestamo.Id))
        {
            return;
        }

        _seleccionado = null;
        _error = null;
        _avisoLista = $"El préstamo de {prestamo.TipoEquipamiento} de {prestamo.Solicitante} ya no está activo; " +
                      "la lista se actualizó.";
    }

    // Si quien registra perdio el permiso, el servicio niega la lista: se cierra el formulario y se avisa.
    private bool Recargar()
    {
        try
        {
            _prestamos = ServicioDevolucion.ListarPrestamosActivos(DniUsuario);
            return true;
        }
        catch (ServicioException ex)
        {
            _prestamos = new List<PrestamoActivoDto>();
            _seleccionado = null;
            _error = null;
            _exito = null;
            _registrada = null;
            _avisoLista = ex.Message;
            return false;
        }
    }
}
