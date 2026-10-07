using DTOs;
using UI.Sesion;
using Servicios;

namespace UI.Components.Pages;

public partial class Catalogo
{
    private const string EstadoActivo = "Activo";
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string? _error;
    private string _categoria = string.Empty;
    private string _estado = string.Empty;
    private int? _minimoDisponibles;
    private string? _tipoSeleccionado;
    private List<UnidadFisicaDto> _unidades = new List<UnidadFisicaDto>();

    // Clave: nombre del enum que espera el servicio. Valor: texto que ve el usuario.
    private static readonly Dictionary<string, string> Categorias = new Dictionary<string, string>
    {
        { "Computacion", "Computación" },
        { "Audiovisual", "Audiovisual" },
        { "Laboratorio", "Laboratorio" },
        { "Electronica", "Electrónica" },
        { "Accesorios", "Accesorios" }
    };

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
        // Un filtro vacio (todo en null) trae todos los tipos: el servicio no acepta un filtro null.
        FiltroCatalogoDto filtro = new FiltroCatalogoDto{ Categoria = ValorONulo(_categoria), Estado = ValorONulo(_estado), MinimoDisponibles = _minimoDisponibles };

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

    private void LimpiarFiltros()
    {
        _categoria = string.Empty;
        _estado = string.Empty;
        _minimoDisponibles = null;
        Consultar();
    }

    private bool HayFiltrosAplicados()
    {
        if (_categoria != string.Empty || _estado != string.Empty)
        {
            return true;
        }

        return _minimoDisponibles is not null;
    }

    private void VerUnidades(string nombreTipo)
    {
        _error = null;

        try
        {
            _unidades = ServicioCatalogo.ObtenerUnidadesDisponibles(nombreTipo);
            _tipoSeleccionado = nombreTipo;

        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _tipoSeleccionado = null;
            _unidades = new List<UnidadFisicaDto>();
        }
    }

    private static string TextoDeSerie(string? numSerie)
    {
        if (string.IsNullOrEmpty(numSerie))
        {
            return "—";
        }

        return numSerie;
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

    private static string? ValorONulo(string valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return null;
        }

        return valor;
    }

    private static string TextoDeCategoria(string categoria)
    {
        if (Categorias.ContainsKey(categoria))
        {
            return Categorias[categoria];
        }

        return categoria;
    }
}