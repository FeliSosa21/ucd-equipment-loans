using UI.Sesion;

namespace UI.Components.Pages;

public partial class Catalogo
{
    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarCatalogo))
        {
            return;
        }
    }
}