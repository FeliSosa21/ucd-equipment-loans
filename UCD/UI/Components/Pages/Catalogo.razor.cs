using DTOs;
using UI.Sesion;
using Servicios;
using UI.Presentacion;

namespace UI.Components.Pages;

public partial class Catalogo
{
    private List<TipoEquipamientoDto> _tipos = new List<TipoEquipamientoDto>();
    private string? _error;
    private string _categoria = string.Empty;
    private string _estado = string.Empty;
    private int? _minimoDisponibles;
    private bool _puedeFiltrarPorEstado;
    private string? _tipoSeleccionado;
    private List<UnidadFisicaDto> _unidades = new List<UnidadFisicaDto>();

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarCatalogo))
        {
            return;
        }

        // Foro: Docente y Estudiante solo ven los tipos activos; filtrar por estado es de quien gestiona el catalogo.
        _puedeFiltrarPorEstado = SesionActual.Usuario!.Permisos.Contains(Permisos.GestionarCatalogo);
        Consultar();
    }

    private void Consultar()
    {
        _error = null;
        // Un filtro vacio (todo en null) trae todos los tipos: el servicio no acepta un filtro null.
        FiltroCatalogoDto filtro = new FiltroCatalogoDto
        {
            Categoria = ValoresDeFiltro.ValorONulo(_categoria),
            Estado = EstadoAFiltrar(),
            MinimoDisponibles = _minimoDisponibles
        };

        try
        {
            _tipos = ServicioCatalogo.ConsultarTipos(ObtenerIdCatalogoProvisorio(), filtro, DniUsuario);
            CerrarUnidadesSiElTipoNoEstaEnLaLista();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            _tipos = new List<TipoEquipamientoDto>();
            CerrarUnidades();
        }
    }

    // Provisorio hasta que exista el selector de catalogo (T4): el primer catalogo activo.
    private int ObtenerIdCatalogoProvisorio()
    {
        List<CatalogoDto> catalogos = ServicioCatalogo.ListarCatalogos(DniUsuario);
        if (catalogos.Count == 0)
        {
            throw new ServicioException("No hay catalogos disponibles");
        }

        return catalogos[0].Id;
    }

    private string? EstadoAFiltrar()
    {
        if (_puedeFiltrarPorEstado)
        {
            return ValoresDeFiltro.ValorONulo(_estado);
        }

        return EstadosCatalogo.TipoActivo;
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
            _unidades = ServicioCatalogo.ObtenerUnidadesDisponibles(ObtenerIdCatalogoProvisorio(), nombreTipo, DniUsuario);
            _tipoSeleccionado = nombreTipo;
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            CerrarUnidades();
        }
    }

    private void CerrarUnidades()
    {
        _tipoSeleccionado = null;
        _unidades = new List<UnidadFisicaDto>();
    }

    private void CerrarUnidadesSiElTipoNoEstaEnLaLista()
    {
        if (_tipoSeleccionado is null)
        {
            return;
        }

        if (!_tipos.Any(tipo => tipo.Nombre == _tipoSeleccionado))
        {
            CerrarUnidades();
        }
    }

    private string ClaseDeFila(string nombreTipo)
    {
        if (nombreTipo == _tipoSeleccionado)
        {
            return "seleccionada";
        }

        return string.Empty;
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
        if (estado == EstadosCatalogo.TipoActivo)
        {
            return "badge-ok";
        }

        return "badge-neutro";
    }

    private static string ClaseDeDisponibles(int cantidad)
    {
        if (cantidad > 0)
        {
            return "numero hay-disponibles";
        }

        return "numero sin-disponibles";
    }
}