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
        Agregar(dto);
    }

    internal Usuario Agregar(UsuarioDto dto)
    {
        Usuario usuario = CrearUsuario(dto);

        if (_repositorio.ObtenerUsuarioPorDni(usuario.Dni) != null)
        {
            throw new ServicioException(nameof(Usuario.Dni), "El DNI ya existe");
        }

        if (_repositorio.ObtenerUsuarioPorCorreo(usuario.Correo) != null)
        {
            throw new ServicioException(nameof(Usuario.Correo), "El correo ya existe");
        }

        _repositorio.AgregarUsuario(usuario);
        return usuario;
    }

    private static Usuario CrearUsuario(UsuarioDto dto)
    {
        return TraduccionDeErrores.Ejecutar(() => new Usuario(
            dni: dto.Dni,
            nombre: dto.Nombre,
            apellido: dto.Apellido,
            correo: dto.Correo,
            contrasena: dto.Contrasena));
    }
}