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
        Usuario usuario = _repositorio.ObtenerUsuarioPorCorreo(correo)!;

        if (!usuario.ContrasenaEsCorrecta(contrasena))
        {
            throw new DominioException("Correo o contrasena incorrectos");
        }

        return new UsuarioSesionDto { Correo = usuario.Correo, Nombre = usuario.Nombre };
    }
}
