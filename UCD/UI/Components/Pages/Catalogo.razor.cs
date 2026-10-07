using DTOs;
using UI.Sesion;
using Servicios;

namespace UI.Components.Pages;

public partial class Catalogo
{
    private const string EstadoActivo = "Activo";
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string? _error;
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
        _error = null;
        FiltroCatalogoDto filtro = new FiltroCatalogoDto();

        try
        {
            _tipos = ServicioCatalogo.ConsultarTipos(filtro);
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _tipos = new List<TipoEquipamientoDto>();
        }
    }
    private string TextoResultados()
    {
        if (_tipos.Count == 1)
        {
            return "1 tipo encontrado";
        }

        return _tipos.Count + " tipos encontrados";
    }

    private static string ClaseDeEstado(string estado)
    {
        if (estado == EstadoActivo)
        {
            return "badge-ok";
        }

        return "badge-neutro";
    }
}