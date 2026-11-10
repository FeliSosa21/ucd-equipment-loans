using DTOs;
using Microsoft.AspNetCore.Components;
using Servicios;
using UI.Sesion;

namespace UI.Components.Shared;

public partial class SelectorCatalogo
{
    // Valor de IdCatalogo mientras no hay un catalogo elegido (los ids de la base empiezan en 1).
    public const int SinCatalogo = 0;

    private const string IdDelSelect = "selector-catalogo";

    private List<CatalogoDto> _catalogos = new List<CatalogoDto>();
    private string? _error;

    [Inject] private ServicioCatalogo ServicioCatalogo { get; set; } = null!;
    [Inject] private SesionActual SesionActual { get; set; } = null!;

    [Parameter] public int IdCatalogo { get; set; } = SinCatalogo;
    [Parameter] public EventCallback<int> IdCatalogoChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!SesionActual.EstaIniciada)
        {
            return;
        }

        CargarCatalogos();
        await AjustarSeleccionAsync();
    }

    // Lo llama la pagina (por una referencia @ref) cuando el servicio le informa que el catalogo ya no esta activo.
    public async Task RecargarAsync()
    {
        CargarCatalogos();
        await AjustarSeleccionAsync();
    }

    private void CargarCatalogos()
    {
        _error = null;
        try
        {
            _catalogos = ServicioCatalogo.ListarCatalogos(SesionActual.Usuario!.Dni);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _catalogos = new List<CatalogoDto>();
        }
    }

    // Con un solo catalogo se elige solo; si el elegido ya no esta entre los activos, se avisa que no hay ninguno.
    private async Task AjustarSeleccionAsync()
    {
        if (_catalogos.Count == 1 && IdCatalogo != _catalogos[0].Id)
        {
            await IdCatalogoChanged.InvokeAsync(_catalogos[0].Id);
            return;
        }

        if (IdCatalogo != SinCatalogo && !_catalogos.Any(catalogo => catalogo.Id == IdCatalogo))
        {
            await IdCatalogoChanged.InvokeAsync(SinCatalogo);
        }
    }

    private async Task AlElegir(ChangeEventArgs evento)
    {
        await IdCatalogoChanged.InvokeAsync(int.Parse(evento.Value!.ToString()!));
    }

    private string? NombreElegido()
    {
        return _catalogos.FirstOrDefault(catalogo => catalogo.Id == IdCatalogo)?.Nombre;
    }

    private bool PuedeGestionarCatalogos()
    {
        return SesionActual.Usuario!.Permisos.Contains(Permisos.GestionarCatalogo);
    }
}
