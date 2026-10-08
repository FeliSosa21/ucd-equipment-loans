using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioUsuario
{
    private readonly RepositorioUsuario _repositorio;

    public ServicioUsuario(RepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public void Registrar(UsuarioDto dto)
    {
        Usuario usuario = CrearUsuario(dto);

        if (_repositorio.ObtenerUsuarioPorDni(usuario.Dni) != null)
        {
            throw new ServicioException("El DNI ya existe");
        }

        if (_repositorio.ObtenerUsuarioPorCorreo(usuario.Correo) != null)
        {
            throw new ServicioException("El correo ya existe");
        }

        _repositorio.AgregarUsuario(usuario);
    }

    private static Usuario CrearUsuario(UsuarioDto dto)
    {
        try
        {
            return new Usuario(
                dni: dto.Dni,
                nombre: dto.Nombre,
                apellido: dto.Apellido,
                correo: dto.Correo,
                contrasena: dto.Contrasena);
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Message);
        }
    }
}