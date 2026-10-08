using DTOs;
using UI.Sesion;
using UI.Presentacion;

namespace UI.Components.Pages;

public partial class Inventario
{
    private List<CantidadesPorEstadoDto> _porCategoria = new List<CantidadesPorEstadoDto>();
    private List<CantidadesPorEstadoDto> _porTipo = new List<CantidadesPorEstadoDto>();
    private string _categoria = string.Empty;
    private string _estado = string.Empty;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUnidades))
        {
            return;
        }

        Consultar();
    }

    private void Consultar()
    {
        _porCategoria = ServicioInventario.ContarPorCategoria();
        FiltroInventarioDto filtro = new FiltroInventarioDto
        {
            Categoria = ValoresDeFiltro.ValorONulo(_categoria),
            Estado = ValoresDeFiltro.ValorONulo(_estado)
        };
        _porTipo = ServicioInventario.ContarPorTipo(filtro);
    }

    private void LimpiarFiltros()
    {
        _categoria = string.Empty;
        _estado = string.Empty;
        Consultar();
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