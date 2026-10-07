using DTOs;
using Servicios;
using UI.Components.Shared;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Usuarios
{
    private List<UsuarioResumenDto> _usuarios = new List<UsuarioResumenDto>();
    private List<string> _tipos = new List<string>();
    private string _busqueda = string.Empty;
    private string? _error;
    private string? _exito;
    private bool _mostrarAlta;
    private UsuarioDto _nuevo = new UsuarioDto();
    private string _tipoNuevo = string.Empty;
    private DatosPersonalesDto? _editando;
    private int _recargas;

    private IEnumerable<UsuarioResumenDto> UsuariosFiltrados =>
        _usuarios.Where(usuario => string.IsNullOrWhiteSpace(_busqueda) ||
                                   $"{usuario.Nombre} {usuario.Apellido} {usuario.Dni} {usuario.Correo}"
                                       .Contains(_busqueda.Trim(), StringComparison.OrdinalIgnoreCase));

    private bool AltaCompleta =>
        !string.IsNullOrWhiteSpace(_nuevo.Dni) &&
        CamposDatosPersonales.EstanCompletos(_nuevo.Nombre, _nuevo.Apellido, _nuevo.Correo);

    private bool EdicionCompleta =>
        _editando is not null &&
        CamposDatosPersonales.EstanCompletos(_editando.Nombre, _editando.Apellido, _editando.Correo);

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUsuarios))
        {
            return;
        }

        _tipos = ServicioAdministracion.NombresDeTipos();
        _tipoNuevo = _tipos.First();
        Recargar();
    }

    private void AbrirAlta()
    {
        _mostrarAlta = true;
        _editando = null;
        LimpiarMensajes();
    }

    private void CancelarAlta()
    {
        _mostrarAlta = false;
        LimpiarMensajes();
    }

    private void CrearUsuario()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (!_mostrarAlta)
        {
            return;
        }

        UsuarioDto datos = new UsuarioDto
        {
            Dni = _nuevo.Dni.Trim(), Nombre = _nuevo.Nombre.Trim(), Apellido = _nuevo.Apellido.Trim(),
            Correo = _nuevo.Correo.Trim()
        };

        if (Ejecutar(() => ServicioAdministracion.CrearUsuario(datos, _tipoNuevo, DniUsuario)))
        {
            _exito = $"Se creó la cuenta de {datos.Nombre} {datos.Apellido}.";
            _nuevo = new UsuarioDto();
            _mostrarAlta = false;
        }
    }

    private void AbrirEdicion(UsuarioResumenDto usuario)
    {
        _mostrarAlta = false;
        LimpiarMensajes();
        _editando = new DatosPersonalesDto
        {
            Dni = usuario.Dni, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Correo = usuario.Correo
        };
    }

    private void GuardarEdicion()
    {
        // Un doble clic manda dos envios: el segundo llega con el formulario ya cerrado.
        if (_editando is null)
        {
            return;
        }

        DatosPersonalesDto editado = _editando;
        editado.Nombre = editado.Nombre.Trim();
        editado.Apellido = editado.Apellido.Trim();
        editado.Correo = editado.Correo.Trim();

        if (Ejecutar(() => ServicioAdministracion.ModificarDatos(editado, DniUsuario)))
        {
            if (EsElMismoDni(editado.Dni))
            {
                SesionActual.ActualizarDatos(editado.Nombre, editado.Correo);
            }

            _exito = $"Se guardaron los datos de {editado.Nombre} {editado.Apellido}.";
            _editando = null;
        }
    }

    private bool EsElMismoDni(string dni) => dni == SesionActual.Usuario?.Dni;

    private bool EsElMismo(UsuarioResumenDto usuario) => EsElMismoDni(usuario.Dni);

    private void AsignarTipo(UsuarioResumenDto usuario, string? tipo)
    {
        if (Ejecutar(() => ServicioAdministracion.AsignarTipo(usuario.Dni, tipo ?? string.Empty, DniUsuario)))
        {
            _exito = $"{usuario.Nombre} {usuario.Apellido} ahora es {tipo}.";
        }
    }

    private void CambiarEstado(UsuarioResumenDto usuario)
    {
        if (usuario.Activo)
        {
            if (Ejecutar(() => ServicioAdministracion.Desactivar(usuario.Dni, DniUsuario)))
            {
                _exito = $"Se desactivó la cuenta de {usuario.Nombre} {usuario.Apellido}.";
            }

            return;
        }

        if (Ejecutar(() => ServicioAdministracion.Reactivar(usuario.Dni, DniUsuario)))
        {
            _exito = $"Se reactivó la cuenta de {usuario.Nombre} {usuario.Apellido}.";
        }
    }

    private void CancelarEdicion()
    {
        _editando = null;
        LimpiarMensajes();
    }

    // Al abrir o cerrar un formulario se borran los mensajes de la accion anterior.
    private void LimpiarMensajes()
    {
        _error = null;
        _exito = null;
    }

    // Devuelve si la accion se pudo hacer; si no, deja el mensaje del servicio en _error.
    private bool Ejecutar(Action accion)
    {
        LimpiarMensajes();
        bool exito = true;
        try
        {
            accion();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
            exito = false;
        }

        Recargar();
        return exito;
    }

    // Cambiar la clave de la tabla la vuelve a dibujar entera: si el servicio rechaza un cambio de tipo,
    // el desplegable vuelve a mostrar el tipo real y no el que se habia elegido.
    // Si quien mira la lista dejo de ser administrador, el servicio la niega y se muestra el motivo.
    private void Recargar()
    {
        try
        {
            _usuarios = ServicioAdministracion.ListarUsuarios(DniUsuario);
        }
        catch (ServicioException ex)
        {
            _usuarios = new List<UsuarioResumenDto>();
            _error = ex.Message;
        }

        _recargas++;
    }
}
