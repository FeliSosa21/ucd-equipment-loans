using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

// Req. 4: el funcionario, el supervisor o el administrador registra unidades fisicas, modifica su informacion,
// cambia su estado y registra su baja. Las reglas las controla ServicioUnidades; la pagina solo muestra.
public partial class Unidades
{
    private List<UnidadDetalleDto> _unidades = new List<UnidadDetalleDto>();
    private List<TipoEquipamientoDetalleDto> _tipos = new List<TipoEquipamientoDetalleDto>();
    private string _tipoFiltro = string.Empty;
    private string _estadoFiltro = string.Empty;
    private bool _mostrarFormulario;
    private string? _idEditando;
    private string _tipoEditando = string.Empty;
    private DatosUnidadDto _datos = new DatosUnidadDto();
    private UnidadDetalleDto? _aDarDeBaja;
    private int _idCatalogo;
    private string? _error;
    private string? _exito;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUnidades))
        {
            return;
        }

        ResolverCatalogoProvisorio();
        Recargar();
    }

    // Provisorio hasta que exista el selector de catalogo (T4): el primer catalogo activo.
    private void ResolverCatalogoProvisorio()
    {
        try
        {
            List<CatalogoDto> catalogos = ServicioCatalogo.ListarCatalogos(DniUsuario);
            if (catalogos.Count > 0)
            {
                _idCatalogo = catalogos[0].Id;
            }
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }
    }


    // ---------- Listado y filtros ----------

    private void Recargar()
    {
        try
        {
            _tipos = ServicioTipos.Listar(_idCatalogo, new FiltroTiposDto(), DniUsuario);
            FiltroUnidadesDto filtro = new FiltroUnidadesDto
            {
                NombreTipo = ValoresDeFiltro.ValorONulo(_tipoFiltro),
                Estado = ValoresDeFiltro.ValorONulo(_estadoFiltro)
            };
            _unidades = ServicioUnidades.Listar(_idCatalogo, filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _unidades = new List<UnidadDetalleDto>();
            _error = ex.Message;
        }
    }

    // Al cambiar un filtro no queda en pantalla el mensaje de la accion anterior.
    private void Filtrar()
    {
        LimpiarMensajes();
        Recargar();
    }

    private void LimpiarFiltros()
    {
        _tipoFiltro = string.Empty;
        _estadoFiltro = string.Empty;
        Filtrar();
    }

    private bool HayFiltrosAplicados()
    {
        return _tipoFiltro != string.Empty || _estadoFiltro != string.Empty;
    }

    // Solo se pueden registrar unidades de tipos activos (letra).
    private List<TipoEquipamientoDetalleDto> TiposActivos()
    {
        List<TipoEquipamientoDetalleDto> activos = new List<TipoEquipamientoDetalleDto>();
        foreach (TipoEquipamientoDetalleDto tipo in _tipos)
        {
            if (tipo.Estado == EstadosCatalogo.TipoActivo)
            {
                activos.Add(tipo);
            }
        }

        return activos;
    }

    // ---------- Alta y edicion ----------

    private void AbrirAlta()
    {
        CerrarTodo();
        _datos = new DatosUnidadDto();
        _idEditando = null;
        _mostrarFormulario = true;
    }

    private void AbrirEdicion(UnidadDetalleDto unidad)
    {
        CerrarTodo();
        _datos = new DatosUnidadDto
        {
            IdInstitucional = unidad.IdInstitucional,
            UbicacionFisica = unidad.UbicacionFisica,
            NumeroSerie = unidad.NumeroSerie,
            Observaciones = unidad.Observaciones
        };
        _idEditando = unidad.IdInstitucional;
        _tipoEditando = unidad.Tipo;
        _mostrarFormulario = true;
    }

    private bool EsAlta()
    {
        return _idEditando is null;
    }

    // Prevencion de errores: el boton se habilita recien cuando estan los datos obligatorios.
    private bool FormularioCompleto()
    {
        if (string.IsNullOrWhiteSpace(_datos.UbicacionFisica))
        {
            return false;
        }

        if (EsAlta())
        {
            return _datos.NombreTipo != string.Empty && !string.IsNullOrWhiteSpace(_datos.IdInstitucional);
        }

        return true;
    }

    private void Guardar()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (!_mostrarFormulario)
        {
            return;
        }

        LimpiarMensajes();
        string id = _datos.IdInstitucional.Trim();
        try
        {
            if (EsAlta())
            {
                ServicioUnidades.Registrar(_idCatalogo, _datos, DniUsuario);
                _exito = $"Se registró la unidad {id}.";
            }
            else
            {
                ServicioUnidades.Modificar(_idCatalogo, _datos, DniUsuario);
                _exito = $"Se guardaron los cambios de la unidad {id}.";
            }

            _mostrarFormulario = false;
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    private void CancelarFormulario()
    {
        _mostrarFormulario = false;
        LimpiarMensajes();
    }

    // ---------- Cambios de estado ----------

    private void EnviarAMantenimiento(UnidadDetalleDto unidad)
    {
        // Un doble clic manda dos envios: el segundo llega con la unidad ya en mantenimiento.
        if (!SigueEnEstado(unidad, EstadosCatalogo.UnidadDisponible))
        {
            return;
        }

        CerrarTodo();
        try
        {
            ServicioUnidades.EnviarAMantenimiento(_idCatalogo, unidad.IdInstitucional, DniUsuario);
            _exito = $"La unidad {unidad.IdInstitucional} pasó a mantenimiento.";
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    private void VolverADisponible(UnidadDetalleDto unidad)
    {
        // Un doble clic manda dos envios: el segundo llega con la unidad ya disponible.
        if (!SigueEnEstado(unidad, EstadosCatalogo.UnidadEnMantenimiento))
        {
            return;
        }

        CerrarTodo();
        try
        {
            ServicioUnidades.VolverADisponible(_idCatalogo, unidad.IdInstitucional, DniUsuario);
            _exito = $"La unidad {unidad.IdInstitucional} volvió a estar disponible.";
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    // ---------- Baja (con confirmacion: no se puede deshacer) ----------

    private void PedirBaja(UnidadDetalleDto unidad)
    {
        CerrarTodo();
        _aDarDeBaja = unidad;
    }

    private void ConfirmarBaja()
    {
        // Un doble clic manda dos envios: el segundo llega con la confirmacion ya cerrada.
        if (_aDarDeBaja is null)
        {
            return;
        }

        string id = _aDarDeBaja.IdInstitucional;
        _aDarDeBaja = null;
        try
        {
            ServicioUnidades.DarDeBaja(_idCatalogo, id, DniUsuario);
            _exito = $"Se dio de baja la unidad {id}.";
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    private void CancelarBaja()
    {
        _aDarDeBaja = null;
    }

    // ---------- Comunes ----------

    // Solo un formulario o confirmacion abierto a la vez, y sin mensajes de la accion anterior.
    private void CerrarTodo()
    {
        _mostrarFormulario = false;
        _aDarDeBaja = null;
        LimpiarMensajes();
    }

    private void LimpiarMensajes()
    {
        _error = null;
        _exito = null;
    }

    // La fila del clic puede ser de un listado viejo: se compara con el estado del listado actual.
    private bool SigueEnEstado(UnidadDetalleDto unidad, string estado)
    {
        foreach (UnidadDetalleDto actual in _unidades)
        {
            if (actual.IdInstitucional == unidad.IdInstitucional)
            {
                return actual.Estado == estado;
            }
        }

        return false;
    }

    private static bool PuedeEditarse(UnidadDetalleDto unidad)
    {
        return unidad.Estado != EstadosCatalogo.UnidadDadaDeBaja;
    }

    private static bool PuedeIrAMantenimiento(UnidadDetalleDto unidad)
    {
        return unidad.Estado == EstadosCatalogo.UnidadDisponible;
    }

    private static bool PuedeVolverADisponible(UnidadDetalleDto unidad)
    {
        return unidad.Estado == EstadosCatalogo.UnidadEnMantenimiento;
    }

    // Una unidad prestada no se da de baja directamente (letra); una dada de baja ya no cambia.
    private static bool PuedeDarseDeBaja(UnidadDetalleDto unidad)
    {
        return unidad.Estado == EstadosCatalogo.UnidadDisponible ||
               unidad.Estado == EstadosCatalogo.UnidadEnMantenimiento;
    }

    private static bool EstaPrestada(UnidadDetalleDto unidad)
    {
        return unidad.Estado == EstadosCatalogo.UnidadPrestada;
    }

    private static string ClaseDeEstado(UnidadDetalleDto unidad)
    {
        if (unidad.Estado == EstadosCatalogo.UnidadDisponible)
        {
            return "badge badge-ok";
        }

        if (unidad.Estado == EstadosCatalogo.UnidadEnMantenimiento)
        {
            return "badge badge-alerta";
        }

        if (unidad.Estado == EstadosCatalogo.UnidadPrestada)
        {
            return "badge badge-neutro";
        }

        return "badge badge-error";
    }

    private static string TextoOpcional(string? texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return "—";
        }

        return texto;
    }
}
