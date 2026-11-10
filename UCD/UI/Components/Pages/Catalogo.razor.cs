using DTOs;
using UI.Sesion;
using Servicios;
using UI.Presentacion;

namespace UI.Components.Pages;

public partial class Catalogo
{
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string? _error;
    private string _categoria = string.Empty;
    private string _estado = string.Empty;
    private int? _minimoDisponibles;
    private bool _puedeFiltrarPorEstado;
    private string? _tipoSeleccionado;
    private List<UnidadFisicaDto> _unidades = new List<UnidadFisicaDto>();

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarCatalogo))
        {
            return;
        }

        // Solo quien gestiona el catalogo ve el filtro por estado; los discontinuados los oculta el servicio.
        _puedeFiltrarPorEstado = SesionActual.Usuario!.Permisos.Contains(Permisos.GestionarCatalogo);
    }

    private async Task AlCambiarCatalogoAsync()
    {
        CerrarUnidades();
        if (!HayCatalogoElegido())
        {
            _error = null;
            _tipos = new List<TipoEquipamientoDto>();
            return;
        }

        await ConsultarAsync();
    }

    private async Task ConsultarAsync()
    {
        _error = null;
        // Un filtro vacio (todo en null) trae todos los tipos: el servicio no acepta un filtro null.
        FiltroCatalogoDto filtro = new FiltroCatalogoDto
        {
            Categoria = ValoresDeFiltro.ValorONulo(_categoria),
            Estado = ValoresDeFiltro.ValorONulo(_estado),
            MinimoDisponibles = _minimoDisponibles
        };

        try
        {
            _tipos = ServicioCatalogo.ConsultarTipos(IdCatalogo, filtro, DniUsuario);
            CerrarUnidadesSiElTipoNoEstaEnLaLista();
        }
        catch (ServicioException ex)
        {
            _tipos = new List<TipoEquipamientoDto>();
            CerrarUnidades();
            await RecargarSelectorAsync();
            _error = ex.Message;
        }
    }

    private async Task LimpiarFiltrosAsync()
    {
        _categoria = string.Empty;
        _estado = string.Empty;
        _minimoDisponibles = null;
        await ConsultarAsync();
    }

    private bool HayFiltrosAplicados()
    {
        if (_categoria != string.Empty || _estado != string.Empty)
        {
            return true;
        }

        return _minimoDisponibles is not null;
    }

    private async Task VerUnidadesAsync(string nombreTipo)
    {
        _error = null;
        try
        {
            _unidades = ServicioCatalogo.ObtenerUnidadesDisponibles(IdCatalogo, nombreTipo,
                DniUsuario);
            _tipoSeleccionado = nombreTipo;
        }
        catch (ServicioException ex)
        {
            CerrarUnidades();
            await RecargarSelectorAsync();
            _error = ex.Message;
        }
    }

    private void CerrarUnidades()
    {
        _tipoSeleccionado = null;
        _unidades = new List<UnidadFisicaDto>();
    }

    private void CerrarUnidadesSiElTipoNoEstaEnLaLista()
    {
        if (_tipoSeleccionado is null)
        {
            return;
        }

        if (!_tipos.Any(tipo => tipo.Nombre == _tipoSeleccionado))
        {
            CerrarUnidades();
        }
    }

    private string ClaseDeFila(string nombreTipo)
    {
        if (nombreTipo == _tipoSeleccionado)
        {
            return "seleccionada";
        }

        return string.Empty;
    }

    private static string TextoDeSerie(string? numSerie)
    {
        if (string.IsNullOrEmpty(numSerie))
        {
            return "—";
        }

        return numSerie;
    }

    private string TextoResultados()
    {
        if (_tipos.Count == 1)
        {
            return "1 tipo encontrado";
        }

        return _tipos.Count + " tipos encontrados";
    }

    private static string ClaseDeEstado(string estado)
    {
        if (estado == EstadosCatalogo.TipoActivo)
        {
            return "badge-ok";
        }

        return "badge-neutro";
    }

    private static string ClaseDeDisponibles(int cantidad)
    {
        if (cantidad > 0)
        {
            return "numero hay-disponibles";
        }

        return "numero sin-disponibles";
    }
}