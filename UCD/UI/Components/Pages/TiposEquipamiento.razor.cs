using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

// Req. 4: el supervisor (o el administrador) registra, modifica, discontinua, reactiva y busca tipos de equipamiento.
// Las reglas las controla ServicioTiposEquipamiento; la pagina solo muestra.
public partial class TiposEquipamiento
{
    private const int DuracionInicial = 1;

    private List<TipoEquipamientoDetalleDto> _tipos = new List<TipoEquipamientoDetalleDto>();
    private string _busqueda = string.Empty;
    private string _categoria = string.Empty;
    private bool _mostrarFormulario;
    private string? _nombreOriginal;
    private DatosTipoEquipamientoDto _datos = new DatosTipoEquipamientoDto();
    private TipoEquipamientoDetalleDto? _aDiscontinuar;
    private int _idCatalogo;
    private string? _error;
    private string? _exito;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarCatalogo))
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
            FiltroTiposDto filtro = new FiltroTiposDto
            {
                Nombre = ValoresDeFiltro.ValorONulo(_busqueda),
                Categoria = ValoresDeFiltro.ValorONulo(_categoria)
            };
            _tipos = ServicioTipos.Listar(_idCatalogo, filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _tipos = new List<TipoEquipamientoDetalleDto>();
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
        _busqueda = string.Empty;
        _categoria = string.Empty;
        Filtrar();
    }

    private bool HayFiltrosAplicados()
    {
        return _busqueda != string.Empty || _categoria != string.Empty;
    }

    // ---------- Alta y edicion ----------

    private void AbrirAlta()
    {
        CerrarTodo();
        _datos = new DatosTipoEquipamientoDto { DuracionMaxPrestamo = DuracionInicial };
        _nombreOriginal = null;
        _mostrarFormulario = true;
    }

    private void AbrirEdicion(TipoEquipamientoDetalleDto tipo)
    {
        CerrarTodo();
        _datos = new DatosTipoEquipamientoDto
        {
            Nombre = tipo.Nombre,
            Descripcion = tipo.Descripcion,
            DuracionMaxPrestamo = tipo.DuracionMaxPrestamo,
            RequiereAprobacion = tipo.RequiereAprobacion,
            Categoria = tipo.Categoria
        };
        _nombreOriginal = tipo.Nombre;
        _mostrarFormulario = true;
    }

    private string TituloFormulario()
    {
        if (_nombreOriginal is null)
        {
            return "Nuevo tipo de equipamiento";
        }

        return $"Editar «{_nombreOriginal}»";
    }

    // Prevencion de errores: el boton se habilita recien cuando estan los datos obligatorios.
    private bool FormularioCompleto()
    {
        return !string.IsNullOrWhiteSpace(_datos.Nombre) && _datos.Categoria != string.Empty &&
               _datos.DuracionMaxPrestamo > 0;
    }

    private void Guardar()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (!_mostrarFormulario)
        {
            return;
        }

        LimpiarMensajes();
        string nombre = _datos.Nombre.Trim();
        try
        {
            if (_nombreOriginal is null)
            {
                ServicioTipos.Registrar(_idCatalogo, _datos, DniUsuario);
                _exito = $"Se registró el tipo «{nombre}».";
            }
            else
            {
                ModificacionTipoEquipamientoDto modificacion = new ModificacionTipoEquipamientoDto
                {
                    NombreActual = _nombreOriginal,
                    Datos = _datos
                };
                ServicioTipos.Modificar(_idCatalogo, modificacion, DniUsuario);
                _exito = $"Se guardaron los cambios de «{nombre}».";
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

    // ---------- Discontinuar (con confirmacion) ----------

    private void PedirDiscontinuar(TipoEquipamientoDetalleDto tipo)
    {
        CerrarTodo();
        _aDiscontinuar = tipo;
    }

    private void ConfirmarDiscontinuar()
    {
        // Un doble clic manda dos envios: el segundo llega con la confirmacion ya cerrada.
        if (_aDiscontinuar is null)
        {
            return;
        }

        string nombre = _aDiscontinuar.Nombre;
        _aDiscontinuar = null;
        try
        {
            ServicioTipos.Discontinuar(_idCatalogo, nombre, DniUsuario);
            _exito = $"Se discontinuó el tipo «{nombre}».";
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    private void CancelarDiscontinuar()
    {
        _aDiscontinuar = null;
    }

    // ---------- Reactivar (foro: un tipo discontinuado puede volver a activarse) ----------

    private void Reactivar(TipoEquipamientoDetalleDto tipo)
    {
        // Un doble clic manda dos envios: el segundo llega con el tipo ya activo.
        if (!SigueDiscontinuado(tipo))
        {
            return;
        }

        CerrarTodo();
        try
        {
            ServicioTipos.Reactivar(_idCatalogo, tipo.Nombre, DniUsuario);
            _exito = $"Se reactivó el tipo «{tipo.Nombre}».";
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    // La fila del clic puede ser de un listado viejo: se compara con el estado del listado actual.
    private bool SigueDiscontinuado(TipoEquipamientoDetalleDto tipo)
    {
        foreach (TipoEquipamientoDetalleDto actual in _tipos)
        {
            if (actual.Nombre == tipo.Nombre)
            {
                return actual.Estado == EstadosCatalogo.TipoDiscontinuado;
            }
        }

        return false;
    }

    // ---------- Comunes ----------

    // Solo un formulario o confirmacion abierto a la vez, y sin mensajes de la accion anterior.
    private void CerrarTodo()
    {
        _mostrarFormulario = false;
        _aDiscontinuar = null;
        LimpiarMensajes();
    }

    private void LimpiarMensajes()
    {
        _error = null;
        _exito = null;
    }

    private static bool EstaActivo(TipoEquipamientoDetalleDto tipo)
    {
        return tipo.Estado == EstadosCatalogo.TipoActivo;
    }

    private static string ClaseDeEstado(TipoEquipamientoDetalleDto tipo)
    {
        if (EstaActivo(tipo))
        {
            return "badge badge-ok";
        }

        return "badge badge-neutro";
    }

    private static string TextoAprobacion(TipoEquipamientoDetalleDto tipo)
    {
        if (tipo.RequiereAprobacion)
        {
            return "Sí";
        }

        return "No";
    }
}
