using Microsoft.AspNetCore.Components;
using UI.Sesion;

namespace UI.Components.Shared;

// Base de las paginas que trabajan sobre un catalogo elegido con el SelectorCatalogo. La pagina lo enlaza con
// <SelectorCatalogo @ref="Selector" @bind-IdCatalogo="IdCatalogo" @bind-IdCatalogo:after="..." />.
public abstract class PaginaConCatalogo : PaginaConSesion
{
    protected int IdCatalogo { get; set; } = SelectorCatalogo.SinCatalogo;

    protected SelectorCatalogo? Selector { get; set; }

    protected bool HayCatalogoElegido()
    {
        return IdCatalogo != SelectorCatalogo.SinCatalogo;
    }

    // Se llama cuando el servicio informa un error: si el catalogo se dio de baja con la pagina abierta, el
    // selector lo saca de la lista. La pagina asigna su mensaje de error despues, porque el aviso del selector
    // (que ya no hay catalogo elegido) hace que limpie los mensajes anteriores.
    protected async Task RecargarSelectorAsync()
    {
        if (Selector is not null)
        {
            await Selector.RecargarAsync();
        }
    }
}
