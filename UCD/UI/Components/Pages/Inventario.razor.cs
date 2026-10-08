using DTOs;
using UI.Sesion;

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
            Categoria = ValorONulo(_categoria),
            Estado = ValorONulo(_estado)
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

    // "Todas" se representa con texto vacio y para el servicio significa no filtrar.
    private static string? ValorONulo(string valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return null;
        }

        return valor;
    }
}