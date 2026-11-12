using DTOs;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class PrestamosActivos
{
    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.RealizarPrestamo))
        {
            return;
        }

        _prestamos = ServicioPrestamo.ListarPrestamosActivos(DniUsuario);
    }
}
