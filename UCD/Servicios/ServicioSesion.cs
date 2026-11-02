using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioSesion
{
    private readonly RepositorioUsuario _repositorio;

    public ServicioSesion(RepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public UsuarioSesionDto IniciarSesion(string correo, string contrasena)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorCorreo(correo);

        if (usuario is null || !usuario.ContrasenaEsCorrecta(contrasena))
        {
            throw new ServicioException("Correo o contrasena incorrectos");
        }

        if (usuario.Estado == EstadoUsuario.Desactivado)
        {
            throw new ServicioException("El usuario esta desactivado");
        }

        return CrearSesion(usuario);
    }

    public UsuarioSesionDto RenovarSesion(string dni)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorDni(dni);

        if (usuario is null || usuario.Estado == EstadoUsuario.Desactivado)
        {
            throw new ServicioException("La sesion ya no es valida");
        }

        return CrearSesion(usuario);
    }

    // Se identifica por DNI (el correo se puede modificar) y solo un usuario activo puede cambiarla.
    public void CambiarContrasena(string dni, string actual, string nueva)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorDni(dni);

        if (usuario is null || usuario.Estado == EstadoUsuario.Desactivado)
        {
            throw new ServicioException("La sesion ya no es valida");
        }

        if (!usuario.ContrasenaEsCorrecta(actual))
        {
            throw new ServicioException("La contrasena actual es incorrecta");
        }

        TraduccionDeErrores.Ejecutar(() => usuario.CambiarContrasena(nueva));
    }

    private static UsuarioSesionDto CrearSesion(Usuario usuario)
    {
        List<string> permisos = PermisosDe(usuario);

        return new UsuarioSesionDto
        {
            Dni = usuario.Dni,
            Correo = usuario.Correo,
            Nombre = usuario.Nombre,
            Tipo = usuario.Tipo.Nombre,
            DebeCambiarContrasena = usuario.DebeCambiarContrasena,
            PendienteDeActivacion = permisos.Count == 0,
            Permisos = permisos
        };
    }

    private static List<string> PermisosDe(Usuario usuario)
    {
        return Enum.GetValues<Permiso>()
            .Where(usuario.TienePermiso)
            .Select(permiso => permiso.ToString())
            .ToList();
    }
}
