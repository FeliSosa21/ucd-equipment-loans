using DTOs;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Entregas
{
    private List<PrestamoDto> _prestamos = new List<PrestamoDto>();

    // Los listados no reciben quien los pide: el permiso lo controla la pagina al abrirse.
    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.RealizarPrestamo))
        {
            return;
        }

        _prestamos = ServicioPrestamo.ListarPrestamosPorEntregar();
    }
}
