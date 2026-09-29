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

        return new UsuarioSesionDto
        {
            Correo = usuario.Correo,
            Nombre = usuario.Nombre,
            Tipo = usuario.Tipo.Nombre,
            DebeCambiarContrasena = usuario.DebeCambiarContrasena
        };
    }
}
