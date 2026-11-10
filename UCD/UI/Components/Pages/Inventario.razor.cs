using DTOs;
using Servicios;
using UI.Components.Shared;
using UI.Sesion;
using UI.Presentacion;

namespace UI.Components.Pages;

public partial class Inventario
{
    private List<CantidadesPorEstadoDto> _porCategoria = new List<CantidadesPorEstadoDto>();
    private List<CantidadesPorEstadoDto> _porTipo = new List<CantidadesPorEstadoDto>();
    private string _categoria = string.Empty;
    private string _estado = string.Empty;
    private string? _error;
    private int _idCatalogo = SelectorCatalogo.SinCatalogo;
    private SelectorCatalogo? _selector;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUnidades))
        {
            return;
        }
    }

    private bool HayCatalogoElegido()
    {
        return _idCatalogo != SelectorCatalogo.SinCatalogo;
    }

    private async Task AlCambiarCatalogoAsync()
    {
        if (!HayCatalogoElegido())
        {
            _error = null;
            _porCategoria = new List<CantidadesPorEstadoDto>();
            _porTipo = new List<CantidadesPorEstadoDto>();
            return;
        }

        await ConsultarAsync();
    }

    private async Task ConsultarAsync()
    {
        _error = null;
        FiltroInventarioDto filtro = new FiltroInventarioDto
        {
            Categoria = ValoresDeFiltro.ValorONulo(_categoria),
            Estado = ValoresDeFiltro.ValorONulo(_estado)
        };

        try
        {
            _porCategoria = ServicioInventario.ContarPorCategoria(_idCatalogo, DniUsuario);
            _porTipo = ServicioInventario.ContarPorTipo(_idCatalogo, filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _porCategoria = new List<CantidadesPorEstadoDto>();
            _porTipo = new List<CantidadesPorEstadoDto>();
            await MostrarErrorYRecargarSelectorAsync(ex.Message);
        }
    }

    // Si el catalogo se dio de baja con la pagina abierta, el selector se actualiza antes de mostrar el mensaje:
    // al avisar el selector que ya no hay catalogo elegido, la pagina limpia el error anterior.
    private async Task MostrarErrorYRecargarSelectorAsync(string mensaje)
    {
        if (_selector is not null)
        {
            await _selector.RecargarAsync();
        }

        _error = mensaje;
    }

    private async Task LimpiarFiltrosAsync()
    {
        _categoria = string.Empty;
        _estado = string.Empty;
        await ConsultarAsync();
    }

    private bool HayFiltrosAplicados()
    {
        return _categoria != string.Empty || _estado != string.Empty;
    }

    // Los ceros se atenuan para que resalten las cantidades que importan.
    private static string ClaseDeCantidad(int cantidad)
    {
        if (cantidad == 0)
        {
            return "numero cero";
        }

        return "numero";
    }
}