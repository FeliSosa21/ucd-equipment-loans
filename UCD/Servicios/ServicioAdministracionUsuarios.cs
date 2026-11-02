using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de cuentas: el administrador crea, lista, asigna tipo, modifica, desactiva y reactiva;
// cualquier usuario activo puede ver y modificar sus propios datos. Cada operacion valida al responsable
// (administrador activo, o el mismo usuario activo), asi nadie desactivado con la pantalla abierta puede seguir
// operando. Como un administrador no puede actuar sobre si mismo, siempre queda al menos uno activo.
// NombresDeTipos no valida al responsable: solo devuelve los nombres fijos de los tipos, sin datos de usuarios.
public class ServicioAdministracionUsuarios
{
    public const string ContrasenaPredeterminada = "Ucd12345";
    public static readonly string NombreTipoAdministrador = TipoUsuario.Administrador.Nombre;
    public const int TamanoPagina = 50;
    private const string MensajeSoloAdministrador = "Solo un administrador activo puede gestionar usuarios";

    private readonly RepositorioUsuario _repositorio;
    private readonly ServicioUsuario _servicioUsuario;

    public ServicioAdministracionUsuarios(RepositorioUsuario repositorio, ServicioUsuario servicioUsuario)
    {
        _repositorio = repositorio;
        _servicioUsuario = servicioUsuario;
    }

    public void CrearUsuario(UsuarioDto dto, string nombreTipo, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
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
    }

    public PaginaDto<UsuarioResumenDto> ListarUsuarios(string dniResponsable, string? busqueda, int pagina)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pagina, 1);
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        int total = _repositorio.ContarUsuarios(busqueda);

        return new PaginaDto<UsuarioResumenDto>
        {
            Elementos = _repositorio.ObtenerPaginaDeUsuarios(busqueda, pagina, TamanoPagina).Select(CrearResumen).ToList(),
            Pagina = pagina,
            HayPaginaAnterior = pagina > 1,
            HayPaginaSiguiente = pagina * TamanoPagina < total
        };
    }

    public List<string> NombresDeTipos()
    {
        return TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList();
    }

    public void AsignarTipo(string dni, string nombreTipo, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede cambiar su propio tipo");
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        Usuario usuario = ObtenerUsuario(dni);
        usuario.AsignarTipo(ObtenerTipo(nombreTipo));
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
    }

    public void Desactivar(string dni, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede desactivarse a si mismo");
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        ObtenerUsuario(dni).Desactivar();
    }

    public void Reactivar(string dni, string dniResponsable)
    {
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        ObtenerUsuario(dni).Reactivar();
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
        Usuario? responsable = _repositorio.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || !PuedeGestionarUsuarios(responsable))
        {
            throw new ServicioException(mensaje);
        }
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

    private static bool PuedeGestionarUsuarios(Usuario usuario)
    {
        return usuario.Estado == EstadoUsuario.Activo && usuario.TienePermiso(Permiso.GestionarUsuarios);
    }

    private static TipoUsuario ObtenerTipo(string nombreTipo)
    {
        TipoUsuario? tipo = TipoUsuario.Todos().FirstOrDefault(t => t.Nombre == nombreTipo);

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
