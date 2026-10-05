using Microsoft.AspNetCore.Components;

namespace UI.Sesion;

// Base de las paginas que solo se pueden usar con una sesion iniciada.
public abstract class PaginaConSesion : ComponentBase
{
    [Inject] protected SesionActual SesionActual { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    protected bool RedirigirSiNoHaySesion()
    {
        if (SesionActual.EstaIniciada)
        {
            return false;
        }

        Navigation.NavigateTo("/ingresar");
        return true;
    }

    // Sin sesion se va a ingresar; con sesion pero sin el permiso, al inicio.
    protected bool RedirigirSiNoTienePermiso(string permiso)
    {
        if (RedirigirSiNoHaySesion())
        {
            return true;
        }

        if (SesionActual.Usuario!.Permisos.Contains(permiso))
        {
            return false;
        }

        Navigation.NavigateTo("/");
        return true;
    }
}
