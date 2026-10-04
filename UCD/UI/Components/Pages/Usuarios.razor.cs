using DTOs;
using Servicios;
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
    private UsuarioResumenDto? _editando;
    private int _recargas;

    private IEnumerable<UsuarioResumenDto> UsuariosFiltrados =>
        _usuarios.Where(usuario => string.IsNullOrWhiteSpace(_busqueda) ||
                                   $"{usuario.Nombre} {usuario.Apellido} {usuario.Dni} {usuario.Correo}"
                                       .Contains(_busqueda.Trim(), StringComparison.OrdinalIgnoreCase));

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

    private void AlternarAlta()
    {
        _mostrarAlta = !_mostrarAlta;
        _editando = null;
    }

    private void CrearUsuario()
    {
        UsuarioDto datos = new UsuarioDto
        {
            Dni = _nuevo.Dni.Trim(), Nombre = _nuevo.Nombre.Trim(), Apellido = _nuevo.Apellido.Trim(),
            Correo = _nuevo.Correo.Trim()
        };
        Ejecutar(() => ServicioAdministracion.CrearUsuario(datos, _tipoNuevo, SesionActual.Usuario!.Dni));

        if (_error is null)
        {
            _exito = $"Se creó la cuenta de {_nuevo.Nombre} {_nuevo.Apellido}.";
            _nuevo = new UsuarioDto();
            _mostrarAlta = false;
        }
    }

    private void AbrirEdicion(UsuarioResumenDto usuario)
    {
        _mostrarAlta = false;
        _editando = new UsuarioResumenDto
        {
            Dni = usuario.Dni, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Correo = usuario.Correo
        };
    }

    private void GuardarEdicion()
    {
        UsuarioResumenDto editado = _editando!;
        editado.Nombre = editado.Nombre.Trim();
        editado.Apellido = editado.Apellido.Trim();
        editado.Correo = editado.Correo.Trim();
        Ejecutar(() => ServicioAdministracion.ModificarDatos(editado, SesionActual.Usuario!.Dni));

        if (_error is null)
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
        Ejecutar(() => ServicioAdministracion.AsignarTipo(usuario.Dni, tipo ?? string.Empty,
            SesionActual.Usuario!.Dni));
    }

    private void CambiarEstado(UsuarioResumenDto usuario)
    {
        if (usuario.Activo)
        {
            Ejecutar(() => ServicioAdministracion.Desactivar(usuario.Dni, SesionActual.Usuario!.Dni));
            return;
        }

        Ejecutar(() => ServicioAdministracion.Reactivar(usuario.Dni, SesionActual.Usuario!.Dni));
    }

    private void Ejecutar(Action accion)
    {
        _error = null;
        _exito = null;
        try
        {
            accion();
        }
        catch (ServicioException ex)
        {
            _error = ex.Message;
        }

        Recargar();
    }

    // Cambiar la clave de la tabla la vuelve a dibujar entera: si el servicio rechaza un cambio de tipo,
    // el desplegable vuelve a mostrar el tipo real y no el que se habia elegido.
    private void Recargar()
    {
        _usuarios = ServicioAdministracion.ListarUsuarios();
        _recargas++;
    }
}
