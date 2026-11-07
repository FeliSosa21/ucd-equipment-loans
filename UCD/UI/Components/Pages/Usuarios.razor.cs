using DTOs;
using Servicios;
using UI.Components.Shared;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Usuarios
{
    private List<UsuarioResumenDto> _usuarios = new List<UsuarioResumenDto>();
    private List<string> _tipos = new List<string>();
    private string _busqueda = string.Empty;
    private string _tipoFiltro = string.Empty;
    private string? _error;
    private readonly ErroresDeFormulario _erroresFormulario = new ErroresDeFormulario(
        CampoDni, nameof(UsuarioDto.Nombre), nameof(UsuarioDto.Apellido), nameof(UsuarioDto.Correo));

    private const string CampoDni = nameof(UsuarioDto.Dni);
    private string? _exito;
    private bool _mostrarAlta;
    private UsuarioDto _nuevo = new UsuarioDto();
    private string _tipoNuevo = string.Empty;
    private DatosPersonalesDto? _editando;
    private Confirmacion? _confirmacion;
    private int _recargas;

    // Accion delicada que espera que el administrador la confirme antes de ejecutarse.
    private sealed record Confirmacion(string Pregunta, string TextoBoton, Action Accion);

    private bool HayCriterios => !string.IsNullOrWhiteSpace(_busqueda) || !string.IsNullOrEmpty(_tipoFiltro);

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
        _tipoFiltro = ServicioAdministracionUsuarios.NombreTipoPendiente;
        Recargar();
    }

    private void AbrirAlta()
    {
        _mostrarAlta = true;
        _editando = null;
        _confirmacion = null;
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

        if (Ejecutar(() => ServicioAdministracion.CrearUsuario(datos, _tipoNuevo, DniUsuario), _erroresFormulario))
        {
            _exito = $"Se creó la cuenta de {datos.Nombre} {datos.Apellido}.";
            _nuevo = new UsuarioDto();
            _mostrarAlta = false;
        }
    }

    private void AbrirEdicion(UsuarioResumenDto usuario)
    {
        _mostrarAlta = false;
        _confirmacion = null;
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

        if (Ejecutar(() => ServicioAdministracion.ModificarDatos(editado, DniUsuario), _erroresFormulario))
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

    // Dar permisos de Administrador es lo unico que se confirma: le da acceso a todo el sistema.
    private void AsignarTipo(UsuarioResumenDto usuario, string? tipo)
    {
        if (tipo == ServicioAdministracionUsuarios.NombreTipoAdministrador)
        {
            PedirConfirmacion($"¿Querés que {usuario.Nombre} {usuario.Apellido} sea Administrador? " +
                              "Va a poder hacer cualquier operación, incluso gestionar usuarios.",
                "Sí, hacerlo Administrador", () => EjecutarAsignarTipo(usuario, tipo));
            return;
        }

        EjecutarAsignarTipo(usuario, tipo);
    }

    private void EjecutarAsignarTipo(UsuarioResumenDto usuario, string? tipo)
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
            PedirConfirmacion($"¿Querés desactivar la cuenta de {usuario.Nombre} {usuario.Apellido}? " +
                              "No va a poder ingresar hasta que la reactives.",
                "Sí, desactivar", () => Desactivar(usuario));
            return;
        }

        if (Ejecutar(() => ServicioAdministracion.Reactivar(usuario.Dni, DniUsuario)))
        {
            _exito = $"Se reactivó la cuenta de {usuario.Nombre} {usuario.Apellido}.";
        }
    }

    private void Desactivar(UsuarioResumenDto usuario)
    {
        if (Ejecutar(() => ServicioAdministracion.Desactivar(usuario.Dni, DniUsuario)))
        {
            _exito = $"Se desactivó la cuenta de {usuario.Nombre} {usuario.Apellido}.";
        }
    }

    private void PedirConfirmacion(string pregunta, string textoBoton, Action accion)
    {
        _mostrarAlta = false;
        _editando = null;
        LimpiarMensajes();
        _confirmacion = new Confirmacion(pregunta, textoBoton, accion);
    }

    private void Confirmar()
    {
        // Un doble clic manda dos envios: el segundo llega con la confirmacion ya cerrada.
        if (_confirmacion is null)
        {
            return;
        }

        Action accion = _confirmacion.Accion;
        _confirmacion = null;
        accion();
    }

    // Al cancelar se recarga la tabla para que el desplegable vuelva a mostrar el tipo real.
    private void CancelarConfirmacion()
    {
        _confirmacion = null;
        Recargar();
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
        _erroresFormulario.Limpiar();
    }

    // Devuelve si la accion se pudo hacer. Si no, el error de un campo va junto a ese campo del formulario
    // abierto, y el resto queda en _error.
    private bool Ejecutar(Action accion, ErroresDeFormulario? erroresDelFormulario = null)
    {
        LimpiarMensajes();
        bool exito = true;
        try
        {
            accion();
        }
        catch (ServicioException ex) when (erroresDelFormulario is not null && erroresDelFormulario.Muestra(ex.Campo))
        {
            erroresDelFormulario.Agregar(ex);
            exito = false;
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
        FiltroUsuariosDto filtro = new FiltroUsuariosDto { Texto = _busqueda, NombreTipo = _tipoFiltro };

        try
        {
            _usuarios = ServicioAdministracion.ListarUsuarios(filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _usuarios = new List<UsuarioResumenDto>();
            _error = ex.Message;
        }

        _recargas++;
    }
}
