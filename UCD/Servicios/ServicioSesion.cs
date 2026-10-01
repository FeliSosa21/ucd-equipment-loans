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

    public ResultadoSesionDto IniciarSesion(string correo, string contrasena)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorCorreo(correo);

        if (usuario is null || !usuario.ContrasenaEsCorrecta(contrasena))
        {
            return IngresoFallido("Correo o contrasena incorrectos");
        }

        if (usuario.Estado == EstadoUsuario.Desactivado)
        {
            return IngresoFallido("El usuario esta desactivado");
        }

        return new ResultadoSesionDto { Exitoso = true, Sesion = CrearSesion(usuario) };
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

    public void CambiarContrasena(string correo, string actual, string nueva)
    {
        Usuario? usuario = _repositorio.ObtenerUsuarioPorCorreo(correo);

        if (usuario is null || !usuario.ContrasenaEsCorrecta(actual))
        {
            throw new ServicioException("La contrasena actual es incorrecta");
        }

        try
        {
            usuario.CambiarContrasena(nueva);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message, ex);
        }
    }

    private static ResultadoSesionDto IngresoFallido(string mensaje)
    {
        return new ResultadoSesionDto { Exitoso = false, Mensaje = mensaje };
    }

    private static UsuarioSesionDto CrearSesion(Usuario usuario)
    {
        return new UsuarioSesionDto
        {
            Dni = usuario.Dni,
            Correo = usuario.Correo,
            Nombre = usuario.Nombre,
            Tipo = usuario.Tipo.Nombre,
            DebeCambiarContrasena = usuario.DebeCambiarContrasena,
            PendienteDeActivacion = usuario.Tipo == TipoUsuario.SinPermisos,
            Permisos = PermisosDe(usuario.Tipo)
        };
    }

    private static List<string> PermisosDe(TipoUsuario tipo)
    {
        return Enum.GetValues<Permiso>()
            .Where(tipo.TienePermiso)
            .Select(permiso => permiso.ToString())
            .ToList();
    }
}
