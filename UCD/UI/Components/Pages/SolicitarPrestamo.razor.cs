using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class SolicitarPrestamo
{
    private const int CantidadPorDefecto = 1;

    private List<CatalogoDto> _catalogos = new List<CatalogoDto>();
    private int? _idCatalogo;
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string _tipo = string.Empty;
    private int? _cantidad = CantidadPorDefecto;
    private string? _error;
    private PrestamoDto? _resultado;

    private bool HayCatalogos => _catalogos.Count > 0;

    private bool HayCatalogoElegido => _idCatalogo is not null;

    // Si ListarCatalogos fallo, la lista tambien queda vacia, pero el aviso seria falso: se muestra solo el error.
    private bool MostrarSinCatalogos => !HayCatalogos && _error is null;

    private TipoEquipamientoDto? TipoSeleccionado => _tipos.FirstOrDefault(tipo => tipo.Nombre == _tipo);

    // Con la cantidad vacia el campo queda en null y el boton se deshabilita.
    private bool FormularioCompleto => HayCatalogoElegido && TipoSeleccionado is not null && _cantidad >= 1;

    private string TextoDisponibilidad
    {
        get
        {
            int disponibles = TipoSeleccionado?.CantidadDisponibles ?? 0;

            if (disponibles == 0)
            {
                return "No hay unidades disponibles en este momento.";
            }

            if (disponibles == 1)
            {
                return "Hay 1 unidad disponible.";
            }

            return $"Hay {disponibles} unidades disponibles.";
        }
    }

    // Un prestamo rechazado no tiene inicio ni devolucion: solo se muestra cuando se pidio.
    private string DetalleResultado
    {
        get
        {
            if (_resultado is null)
            {
                return string.Empty;
            }

            string pedido = $"{_resultado.TipoEquipamiento} × {_resultado.CantidadSolicitada}";

            if (_resultado.Estado == EstadosPrestamo.Rechazado)
            {
                return $"{pedido} · solicitado el {_resultado.FechaHoraPrestamo.ToString(Formatos.FechaHora)}";
            }

            return $"{pedido} · " +
                   $"inicio {_resultado.FechaHoraPrestamo.ToString(Formatos.FechaHora)} · " +
                   $"devolución esperada {_resultado.FechaEsperadaDevolucion.ToString(Formatos.Fecha)}";
        }
    }

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.SolicitarPrestamo))
        {
            return;
        }

        CargarCatalogos();
        ElegirElCatalogoSiEsElUnico();
    }

    private void CargarCatalogos()
    {
        try
        {
            _catalogos = ServicioCatalogo.ListarCatalogos(DniUsuario);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }

    private void ElegirElCatalogoSiEsElUnico()
    {
        if (_catalogos.Count == 1)
        {
            _idCatalogo = _catalogos[0].Id;
            CargarTipos();
        }
    }

    // El resultado de la solicitud anterior se mantiene: tiene las unidades que hay que retirar.
    private void CambiarCatalogo()
    {
        _error = null;
        _tipo = string.Empty;
        CargarTipos();
    }

    private void Solicitar()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya vaciado.
        if (!FormularioCompleto)
        {
            return;
        }

        SolicitudPrestamoDto datos = new SolicitudPrestamoDto
        {
            IdCatalogo = _idCatalogo.GetValueOrDefault(),
            TipoEquipamiento = _tipo,
            CantidadSolicitada = _cantidad.GetValueOrDefault()
        };

        _error = null;
        _resultado = null;

        try
        {
            _resultado = ServicioPrestamo.Solicitar(datos, DniUsuario);
            VaciarFormulario();
            CargarTipos();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }

    // Se vacia despues de cada solicitud: asi un doble clic no pide el mismo prestamo dos veces.
    private void VaciarFormulario()
    {
        _tipo = string.Empty;
        _cantidad = CantidadPorDefecto;
    }

    // Se recarga despues de solicitar para que la disponibilidad de cada tipo quede al dia.
    private void CargarTipos()
    {
        _tipos = new List<TipoEquipamientoDto>();
        if (_idCatalogo is null)
        {
            return;
        }

        try
        {
            _tipos = ServicioCatalogo.ConsultarTipos(_idCatalogo.Value,
                new FiltroCatalogoDto { Estado = EstadosCatalogo.TipoActivo }, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }
}
