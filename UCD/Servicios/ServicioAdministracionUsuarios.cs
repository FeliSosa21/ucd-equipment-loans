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

        _servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = dto.Dni,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            Contrasena = ContrasenaPredeterminada
        });

        // Se busca por correo porque el registro puede normalizar el DNI (cedulas de 7 digitos).
        Usuario? creado = _repositorio.ObtenerUsuarioPorCorreo(dto.Correo);

        if (creado is null)
        {
            throw new ServicioException("No se pudo crear el usuario");
        }

        creado.AsignarTipo(tipo);
        creado.ExigirCambioDeContrasena();
    }

    public List<UsuarioResumenDto> ListarUsuarios(string dniResponsable)
    {
        ValidarQueSeaAdministradorActivo(dniResponsable, MensajeSoloAdministrador);
        return _repositorio.ObtenerTodos().Select(CrearResumen).ToList();
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
        string nombre = Recortar(datos.Nombre);
        string apellido = Recortar(datos.Apellido);
        string correo = Recortar(datos.Correo);
        Usuario? duenoDelCorreo = _repositorio.ObtenerUsuarioPorCorreo(correo);

        if (duenoDelCorreo != null && duenoDelCorreo != usuario)
        {
            throw new ServicioException("El correo ya existe");
        }

        try
        {
            usuario.ModificarDatos(nombre, apellido, correo);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
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

        if (responsable is null || !EsAdministradorActivo(responsable))
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

    private static bool EsAdministradorActivo(Usuario usuario)
    {
        return usuario.Tipo == TipoUsuario.Administrador && usuario.Estado == EstadoUsuario.Activo;
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

    // Los espacios de alrededor no cuentan para el largo minimo ni se guardan.
    private static string Recortar(string? valor)
    {
        return valor?.Trim() ?? string.Empty;
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
