using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

// Req. 4: el supervisor (o el administrador) registra, modifica, discontinua, reactiva y busca tipos de equipamiento,
// y cambia el nombre del catalogo. Las reglas las controla ServicioTiposEquipamiento; la pagina solo muestra.
public partial class TiposEquipamiento
{
    private const int DuracionInicial = 1;

    private List<TipoEquipamientoDetalleDto> _tipos = new List<TipoEquipamientoDetalleDto>();
    private string _busqueda = string.Empty;
    private string _categoria = string.Empty;
    private string _nombreCatalogo = string.Empty;
    private string _nombreCatalogoEditado = string.Empty;
    private bool _editandoNombreCatalogo;
    private bool _mostrarFormulario;
    private string? _nombreOriginal;
    private DatosTipoEquipamientoDto _datos = new DatosTipoEquipamientoDto();
    private TipoEquipamientoDetalleDto? _aDiscontinuar;
    private string? _error;
    private string? _exito;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarCatalogo))
        {
            return;
        }

        Recargar();
    }

    // ---------- Listado y filtros ----------

    private void Recargar()
    {
        try
        {
            _nombreCatalogo = ServicioTipos.ObtenerNombreCatalogo();
            _tipos = ServicioTipos.Listar(ValoresDeFiltro.ValorONulo(_busqueda),
                ValoresDeFiltro.ValorONulo(_categoria));
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
                ServicioTipos.Registrar(_datos, DniUsuario);
                _exito = $"Se registró el tipo «{nombre}».";
            }
            else
            {
                ServicioTipos.Modificar(_nombreOriginal, _datos, DniUsuario);
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
            ServicioTipos.Discontinuar(nombre, DniUsuario);
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
            ServicioTipos.Reactivar(tipo.Nombre, DniUsuario);
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

    // ---------- Nombre del catalogo ----------

    private void AbrirEdicionNombreCatalogo()
    {
        CerrarTodo();
        _nombreCatalogoEditado = _nombreCatalogo;
        _editandoNombreCatalogo = true;
    }

    private bool NombreCatalogoCompleto()
    {
        return !string.IsNullOrWhiteSpace(_nombreCatalogoEditado);
    }

    private void GuardarNombreCatalogo()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (!_editandoNombreCatalogo)
        {
            return;
        }

        LimpiarMensajes();
        try
        {
            ServicioTipos.ModificarNombreCatalogo(_nombreCatalogoEditado, DniUsuario);
            _exito = "Se cambió el nombre del catálogo.";
            _editandoNombreCatalogo = false;
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    private void CancelarNombreCatalogo()
    {
        _editandoNombreCatalogo = false;
        LimpiarMensajes();
    }

    // ---------- Comunes ----------

    // Solo un formulario o confirmacion abierto a la vez, y sin mensajes de la accion anterior.
    private void CerrarTodo()
    {
        _mostrarFormulario = false;
        _aDiscontinuar = null;
        _editandoNombreCatalogo = false;
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
