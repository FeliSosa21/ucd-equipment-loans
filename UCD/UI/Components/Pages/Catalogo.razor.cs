using DTOs;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Catalogo
{
    private const string EstadoActivo = "Activo";
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarCatalogo))
        {
            return;
        }

        Consultar();
    }

    private void Consultar()
    {
        FiltroCatalogoDto filtro = new FiltroCatalogoDto();
        _tipos = ServicioCatalogo.ConsultarTipos(filtro);
    }

    private static string ClaseDeEstado(string estado)
    {
        return estado == EstadoActivo ? "badge-ok" : "badge-nuevo";
    }
}