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

    // Lo consulta la pagina (por el @ref) para no pedir que elija cuando no hay nada para elegir.
    public bool HayCatalogos => _catalogos.Count > 0;

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

    // Con un solo catalogo se elige solo; con varios, vuelve el que el usuario eligio antes si sigue activo.
    // Si el elegido ya no esta entre los activos, se avisa que no hay ninguno.
    private async Task AjustarSeleccionAsync()
    {
        if (_catalogos.Count == 1 && IdCatalogo != _catalogos[0].Id)
        {
            await ElegirAsync(_catalogos[0].Id);
            return;
        }

        if (IdCatalogo != SinCatalogo && !EstaActivo(IdCatalogo))
        {
            await ElegirAsync(SinCatalogo);
            return;
        }

        int? recordado = SesionActual.IdCatalogoElegido;
        if (IdCatalogo == SinCatalogo && recordado is int id && EstaActivo(id))
        {
            await ElegirAsync(id);
        }
    }

    private async Task AlElegir(ChangeEventArgs evento)
    {
        await ElegirAsync(int.Parse(evento.Value!.ToString()!));
    }

    private async Task ElegirAsync(int idCatalogo)
    {
        if (idCatalogo == SinCatalogo)
        {
            SesionActual.IdCatalogoElegido = null;
        }
        else
        {
            SesionActual.IdCatalogoElegido = idCatalogo;
        }

        await IdCatalogoChanged.InvokeAsync(idCatalogo);
    }

    private bool EstaActivo(int idCatalogo)
    {
        return _catalogos.Any(catalogo => catalogo.Id == idCatalogo);
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
