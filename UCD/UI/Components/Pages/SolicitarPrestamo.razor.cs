using DTOs;
using Microsoft.AspNetCore.Components;
using Servicios;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class SolicitarPrestamo
{
    private const string FormatoFecha = "dd/MM/yyyy HH:mm";
    private const string FormatoFechaEsperada = "dd/MM/yyyy";
    private const int CantidadPorDefecto = 1;

    // Estado de un tipo de equipamiento: solo se pueden solicitar los activos.
    private const string EstadoTipoActivo = "Activo";

    // Estados del prestamo que devuelve el servicio al solicitar.
    private const string EstadoActivo = "Activo";
    private const string EstadoSolicitado = "Solicitado";
    private const string EstadoRechazado = "Rechazado";

    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string _tipo = string.Empty;
    private int? _cantidad = CantidadPorDefecto;
    private string? _error;
    private PrestamoDto? _resultado;

    // Nombre del tipo elegido en el catalogo (/solicitar-prestamo?tipo=...).
    [SupplyParameterFromQuery(Name = "tipo")]
    public string? Tipo { get; set; }

    private TipoEquipamientoDto? TipoSeleccionado => _tipos.FirstOrDefault(tipo => tipo.Nombre == _tipo);

    // Con la cantidad vacia el campo queda en null y el boton se deshabilita.
    private bool FormularioCompleto => TipoSeleccionado is not null && _cantidad >= 1;

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

            if (_resultado.Estado == EstadoRechazado)
            {
                return $"{pedido} · solicitado el {_resultado.FechaHoraPrestamo.ToString(FormatoFecha)}";
            }

            return $"{pedido} · " +
                   $"inicio {_resultado.FechaHoraPrestamo.ToString(FormatoFecha)} · " +
                   $"devolución esperada {_resultado.FechaEsperadaDevolucion.ToString(FormatoFechaEsperada)}";
        }
    }

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.SolicitarPrestamo))
        {
            return;
        }

        CargarTipos();
    }

    // Si el tipo de la direccion no existe o no esta activo, la lista queda sin preseleccion y sin aviso.
    protected override void OnParametersSet()
    {
        _tipo = _tipos.FirstOrDefault(tipo => tipo.Nombre == Tipo)?.Nombre ?? string.Empty;
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
            TipoEquipamiento = _tipo, CantidadSolicitada = _cantidad.GetValueOrDefault()
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
        try
        {
            _tipos = ServicioCatalogo.ConsultarTipos(new FiltroCatalogoDto { Estado = EstadoTipoActivo });
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }
}
