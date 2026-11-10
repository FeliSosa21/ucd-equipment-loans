using DTOs;
using Servicios;
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

    protected override void OnInitialized()
    {
        RedirigirSiNoTienePermiso(Permisos.GestionarUnidades);
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
            _porCategoria = ServicioInventario.ContarPorCategoria(IdCatalogo, DniUsuario);
            _porTipo = ServicioInventario.ContarPorTipo(IdCatalogo, filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _porCategoria = new List<CantidadesPorEstadoDto>();
            _porTipo = new List<CantidadesPorEstadoDto>();
            await RecargarSelectorAsync();
            _error = ex.Message;
        }
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