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
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUnidades))
        {
            return;
        }

        Consultar();
    }

    private void Consultar()
    {
        _error = null;
        FiltroInventarioDto filtro = new FiltroInventarioDto
        {
            Categoria = ValoresDeFiltro.ValorONulo(_categoria),
            Estado = ValoresDeFiltro.ValorONulo(_estado)
        };

        try
        {
            int idCatalogo = ObtenerIdCatalogoProvisorio();
            _porCategoria = ServicioInventario.ContarPorCategoria(idCatalogo, DniUsuario);
            _porTipo = ServicioInventario.ContarPorTipo(idCatalogo, filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _porCategoria = new List<CantidadesPorEstadoDto>();
            _porTipo = new List<CantidadesPorEstadoDto>();
        }
    }

    // Provisorio hasta que exista el selector de catalogo (T4): el primer catalogo activo.
    private int ObtenerIdCatalogoProvisorio()
    {
        List<CatalogoDto> catalogos = ServicioCatalogo.ListarCatalogos(DniUsuario);
        if (catalogos.Count == 0)
        {
            throw new ServicioException("No hay catalogos disponibles");
        }

        return catalogos[0].Id;
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