using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de cuentas: el administrador crea, lista, asigna tipo, modifica, desactiva y reactiva;
// cualquier usuario activo puede ver y modificar sus propios datos. Cada operacion valida al responsable
// (administrador activo, o el mismo usuario activo), asi nadie desactivado con la pantalla abierta puede seguir
// operando. Como un administrador no puede actuar sobre si mismo, siempre queda al menos uno activo.
// NombresDeTipos no valida al responsable: solo devuelve los nombres de los tipos guardados, sin datos de usuarios.
public class ServicioAdministracionUsuarios
{
    public const string ContrasenaPredeterminada = "Ucd12345";
    public static readonly string NombreTipoAdministrador = TipoUsuario.Administrador().Nombre;
    public static readonly string NombreTipoPendiente = TipoUsuario.SinPermisos().Nombre;
    private const string MensajeSoloAdministrador = "Solo un administrador activo puede gestionar usuarios";

    private readonly RepositorioUsuarioSql _repositorio;
    private readonly ServicioUsuario _servicioUsuario;
    private readonly RepositorioTipoUsuarioSql _repositorioTipos;
    private readonly RegistradorDeAuditoria _auditoria;

    public ServicioAdministracionUsuarios(RepositorioUsuarioSql repositorio, ServicioUsuario servicioUsuario,
        RepositorioTipoUsuarioSql repositorioTipos, RegistradorDeAuditoria auditoria)
    {
        _repositorio = repositorio;
        _servicioUsuario = servicioUsuario;
        _repositorioTipos = repositorioTipos;
        _auditoria = auditoria;
    }

    public void CrearUsuario(UsuarioDto dto, string nombreTipo, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Usuario administrador = ObtenerAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        TipoUsuario tipo = ObtenerTipo(nombreTipo);

        Usuario creado = _servicioUsuario.Agregar(new UsuarioDto
        {
            Dni = dto.Dni,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            Contrasena = ContrasenaPredeterminada
        });

        creado.AsignarTipo(tipo);
        creado.ExigirCambioDeContrasena();
        _repositorio.ActualizarUsuario(creado);
        _auditoria.RegistrarDeUsuario(AccionAuditada.AltaDeUsuario, administrador,
            $"Alta de {creado.NombreCompletoConDni} como {tipo.Nombre}");
    }

    public List<UsuarioResumenDto> ListarUsuarios(FiltroUsuariosDto filtro, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);

        return _repositorio.ObtenerUsuarios(CrearFiltro(filtro)).Select(CrearResumen).ToList();
    }

    public bool TieneCriterios(FiltroUsuariosDto filtro)
    {
        return CrearFiltro(filtro).TieneCriterios();
    }

    public List<string> NombresDeTipos()
    {
        return _repositorioTipos.ObtenerTodos().Select(tipo => tipo.Nombre).ToList();
    }

    public void AsignarTipo(string dni, string nombreTipo, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede cambiar su propio tipo");
        Usuario administrador = ObtenerAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        Usuario usuario = ObtenerUsuario(dni);
        string tipoAnterior = usuario.Tipo.Nombre;
        TipoUsuario tipoNuevo = ObtenerTipo(nombreTipo);

        if (usuario.TieneTipo(tipoNuevo))
        {
            return;
        }

        usuario.AsignarTipo(tipoNuevo);
        _repositorio.ActualizarUsuario(usuario);
        _auditoria.RegistrarDeUsuario(AccionAuditada.CambioDeTipoDeUsuario, administrador,
            $"{usuario.NombreCompletoConDni} pasó de {tipoAnterior} a {tipoNuevo.Nombre}");
    }

    public UsuarioResumenDto ObtenerResumen(string dni, string dniResponsable)
    {
        ValidarQuePuedeModificar(dni, dniResponsable);
        return CrearResumen(ObtenerUsuario(dni));
    }

    public void ModificarDatos(DatosPersonalesDto datos, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ValidarQuePuedeModificar(datos.Dni, dniResponsable);
        Usuario usuario = ObtenerUsuario(datos.Dni);
        Usuario? duenoDelCorreo = _repositorio.ObtenerUsuarioPorCorreo(Usuario.NormalizarCorreo(datos.Correo));

        if (duenoDelCorreo != null && duenoDelCorreo != usuario)
        {
            throw new ServicioException(nameof(Usuario.Correo), "El correo ya existe");
        }

        TraduccionDeErrores.Ejecutar(() => usuario.ModificarDatos(datos.Nombre, datos.Apellido, datos.Correo));
        _repositorio.ActualizarUsuario(usuario);
    }

    public void Desactivar(string dni, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede desactivarse a si mismo");
        Usuario administrador = ObtenerAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        Usuario usuario = ObtenerUsuario(dni);

        if (!usuario.EstaActivo())
        {
            return;
        }

        usuario.Desactivar();
        _auditoria.RegistrarDeUsuario(AccionAuditada.DesactivacionDeUsuario, administrador,
            $"Se desactivó la cuenta de {usuario.NombreCompletoConDni}");
    }

    public void Reactivar(string dni, string dniResponsable)
    {
        Usuario administrador = ObtenerAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        Usuario usuario = ObtenerUsuario(dni);

        if (usuario.EstaActivo())
        {
            return;
        }

        TraduccionDeErrores.Ejecutar(() => usuario.Reactivar());
        _auditoria.RegistrarDeUsuario(AccionAuditada.ReactivacionDeUsuario, administrador,
            $"Se reactivó la cuenta de {usuario.NombreCompletoConDni}");
    }

    // El mismo usuario puede ver y modificar sus datos mientras siga activo; a otro, solo un administrador.
    private void ValidarQuePuedeModificar(string dni, string dniResponsable)
    {
        if (dni == dniResponsable)
        {
            ValidarQueEsteActivo(dniResponsable);
            return;
        }

        ValidarQueSeaAdministradorActivo(dniResponsable,
            "Solo un administrador puede modificar los datos de otro usuario");
    }

    private void ValidarQueSeaAdministradorActivo(string dniResponsable, string mensaje)
    {
        ObtenerAdministradorActivo(dniResponsable, mensaje);
    }

    private Usuario ObtenerAdministradorActivo(string dniResponsable, string mensaje)
    {
        Usuario? responsable = _repositorio.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || !responsable.PuedeGestionarUsuarios())
        {
            throw new ServicioException(mensaje);
        }

        return responsable;
    }

    private void ValidarQueEsteActivo(string dni)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorDni(dni);

        if (usuario is null || usuario.Estado == EstadoUsuario.Desactivado)
        {
            throw new ServicioException("La sesion ya no es valida");
        }
    }

    private static void ValidarQueNoSeaElMismo(string dni, string dniResponsable, string mensaje)
    {
        if (dni == dniResponsable)
        {
            throw new ServicioException(mensaje);
        }
    }

    private FiltroUsuarios CrearFiltro(FiltroUsuariosDto filtro)
    {
        return new FiltroUsuarios
        {
            Texto = filtro.Texto,
            Tipo = string.IsNullOrWhiteSpace(filtro.NombreTipo) ? null : ObtenerTipo(filtro.NombreTipo)
        };
    }

    private TipoUsuario ObtenerTipo(string nombreTipo)
    {
        TipoUsuario? tipo = _repositorioTipos.ObtenerTipoPorNombre(nombreTipo);

        if (tipo is null)
        {
            throw new ServicioException("El tipo de usuario no existe");
        }

        return tipo;
    }

    private Usuario ObtenerUsuario(string dni)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorDni(dni);

        if (usuario is null)
        {
            throw new ServicioException("El usuario no existe");
        }

        return usuario;
    }

    private static UsuarioResumenDto CrearResumen(Usuario usuario)
    {
        return new UsuarioResumenDto
        {
            Dni = usuario.Dni,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Tipo = usuario.Tipo.Nombre,
            Activo = usuario.Estado == EstadoUsuario.Activo
        };
    }
}
