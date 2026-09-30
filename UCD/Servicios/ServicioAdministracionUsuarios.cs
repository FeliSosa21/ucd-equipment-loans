using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de cuentas: el administrador crea, lista, asigna tipo, modifica, desactiva y reactiva;
// cualquier usuario puede modificar sus propios datos (la UI controla quien edita a quien).
public class ServicioAdministracionUsuarios
{
    public const string ContrasenaPredeterminada = "Ucd12345";

    private readonly RepositorioUsuario _repositorio;
    private readonly ServicioUsuario _servicioUsuario;

    public ServicioAdministracionUsuarios(RepositorioUsuario repositorio, ServicioUsuario servicioUsuario)
    {
        _repositorio = repositorio;
        _servicioUsuario = servicioUsuario;
    }

    public void CrearUsuario(UsuarioDto dto, string nombreTipo)
    {
        TipoUsuario tipo = ObtenerTipo(nombreTipo);

        _servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = dto.Dni,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            Contrasena = ContrasenaPredeterminada
        });

        Usuario creado = _repositorio.ObtenerUsuarioPorCorreo(dto.Correo)!;
        creado.AsignarTipo(tipo);
        creado.ExigirCambioDeContrasena();
    }

    public List<UsuarioResumenDto> ListarUsuarios()
    {
        return _repositorio.ObtenerTodos().Select(CrearResumen).ToList();
    }

    public List<string> NombresDeTipos()
    {
        return TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList();
    }

    public void AsignarTipo(string dni, string nombreTipo, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede cambiar su propio tipo");
        Usuario usuario = ObtenerUsuario(dni);
        TipoUsuario tipo = ObtenerTipo(nombreTipo);

        if (tipo != TipoUsuario.Administrador)
        {
            ValidarQueQuedeOtroAdministradorActivo(usuario);
        }

        usuario.AsignarTipo(tipo);
    }

    public UsuarioResumenDto ObtenerResumen(string dni)
    {
        return CrearResumen(ObtenerUsuario(dni));
    }

    public void ModificarDatos(string dni, string nombre, string apellido, string correo)
    {
        Usuario usuario = ObtenerUsuario(dni);
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
            throw new ServicioException(ex.Message);
        }
    }

    public void Desactivar(string dni, string dniResponsable)
    {
        ValidarQueNoSeaElMismo(dni, dniResponsable, "Un administrador no puede desactivarse a si mismo");
        Usuario usuario = ObtenerUsuario(dni);
        ValidarQueQuedeOtroAdministradorActivo(usuario);
        usuario.Desactivar();
    }

    public void Reactivar(string dni)
    {
        ObtenerUsuario(dni).Reactivar();
    }

    private static void ValidarQueNoSeaElMismo(string dni, string dniResponsable, string mensaje)
    {
        if (dni == dniResponsable)
        {
            throw new ServicioException(mensaje);
        }
    }

    private void ValidarQueQuedeOtroAdministradorActivo(Usuario usuario)
    {
        if (!EsAdministradorActivo(usuario))
        {
            return;
        }

        bool hayOtro = _repositorio.ObtenerTodos().Any(otro => otro != usuario && EsAdministradorActivo(otro));

        if (!hayOtro)
        {
            throw new ServicioException("Debe quedar al menos un administrador activo");
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
