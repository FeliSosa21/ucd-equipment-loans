using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

// Req. 2.2: el supervisor (o el administrador) crea, renombra y elimina catalogos.
// Las reglas las controla ServicioGestionCatalogos; la pagina solo muestra.
public partial class Catalogos
{
    private const string CampoNombre = nameof(CatalogoDto.Nombre);
    private const string IdNombre = "catalogo-nombre";
    private const string IdAyudaNombre = "catalogo-nombre-ayuda";
    private const int LargoMaximoNombre = 80;

    private readonly ErroresDeFormulario _errores = new ErroresDeFormulario(CampoNombre);
    private List<CatalogoDto> _catalogos = new List<CatalogoDto>();
    private bool _mostrarFormulario;
    private int? _idEditando;
    private string _nombre = string.Empty;
    private CatalogoDto? _aEliminar;
    private string? _exito;

    private string DescripcionDelNombre => _errores.IdDelError(CampoNombre, IdNombre) is string idError
        ? $"{idError} {IdAyudaNombre}"
        : IdAyudaNombre;

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarCatalogo))
        {
            return;
        }

        Recargar();
    }

    // ---------- Listado ----------

    private void Recargar()
    {
        try
        {
            _catalogos = ServicioGestion.Listar(DniUsuario);
        }
        catch (ServicioException ex)
        {
            _catalogos = new List<CatalogoDto>();
            _errores.Agregar(ex);
        }
    }

    // ---------- Alta y edicion ----------

    private void AbrirAlta()
    {
        CerrarTodo();
        _nombre = string.Empty;
        _idEditando = null;
        _mostrarFormulario = true;
    }

    private void AbrirEdicion(CatalogoDto catalogo)
    {
        CerrarTodo();
        _nombre = catalogo.Nombre;
        _idEditando = catalogo.Id;
        _mostrarFormulario = true;
    }

    private string TituloFormulario()
    {
        if (_idEditando is null)
        {
            return "Nuevo catálogo";
        }

        return "Cambiar el nombre del catálogo";
    }

    private void Guardar()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (!_mostrarFormulario)
        {
            return;
        }

        LimpiarMensajes();
        string nombre = _nombre.Trim();
        try
        {
            if (_idEditando is null)
            {
                ServicioGestion.Crear(_nombre, DniUsuario);
                _exito = $"Se creó el catálogo «{nombre}».";
            }
            else
            {
                ServicioGestion.Modificar(_idEditando.Value, _nombre, DniUsuario);
                _exito = $"El catálogo ahora se llama «{nombre}».";
            }

            _mostrarFormulario = false;
        }
        catch (ServicioException ex)
        {
            _errores.Agregar(ex);
        }

        Recargar();
    }

    private void CancelarFormulario()
    {
        _mostrarFormulario = false;
        LimpiarMensajes();
    }

    // ---------- Eliminar (con confirmacion: no se puede deshacer) ----------

    private void PedirEliminar(CatalogoDto catalogo)
    {
        CerrarTodo();
        _aEliminar = catalogo;
    }

    private void ConfirmarEliminar()
    {
        // Un doble clic manda dos envios: el segundo llega con la confirmacion ya cerrada.
        if (_aEliminar is null)
        {
            return;
        }

        CatalogoDto catalogo = _aEliminar;
        _aEliminar = null;
        try
        {
            ServicioGestion.Eliminar(catalogo.Id, DniUsuario);
            _exito = $"Se eliminó el catálogo «{catalogo.Nombre}». " +
                     "Sus tipos, unidades y préstamos quedan guardados.";
        }
        catch (ServicioException ex)
        {
            _errores.Agregar(ex);
        }

        Recargar();
    }

    private void CancelarEliminar()
    {
        _aEliminar = null;
    }

    // ---------- Comunes ----------

    // Solo un formulario o confirmacion abierto a la vez, y sin mensajes de la accion anterior.
    private void CerrarTodo()
    {
        _mostrarFormulario = false;
        _aEliminar = null;
        LimpiarMensajes();
    }

    private void LimpiarMensajes()
    {
        _errores.Limpiar();
        _exito = null;
    }
}
