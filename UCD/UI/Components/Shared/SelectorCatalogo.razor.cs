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
        CargarCatalogos();
        if (_catalogos.Count == 1)
        {
            await IdCatalogoChanged.InvokeAsync(_catalogos[0].Id);
        }
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
