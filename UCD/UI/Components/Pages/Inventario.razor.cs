using DTOs;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Inventario
{
    private List<CantidadesPorEstadoDto> _porCategoria = new List<CantidadesPorEstadoDto>();
    private List<CantidadesPorEstadoDto> _porTipo = new List<CantidadesPorEstadoDto>();

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
        _porTipo = ServicioInventario.ContarPorTipo(new FiltroInventarioDto());
    }
}