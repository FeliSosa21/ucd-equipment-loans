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
        if (_repositorio.ObtenerUsuarioPorDni(dto.Dni) != null)
        {
            throw new DominioException("El DNI ya existe");
        }

        if (_repositorio.ObtenerUsuarioPorCorreo(dto.Correo) != null)
        {
            throw new DominioException("El correo ya existe");
        }

        Usuario usuario = new Usuario(
            dni: dto.Dni,
            nombre: dto.Nombre,
            apellido: dto.Apellido,
            correo: dto.Correo,
            contrasena: dto.Contrasena);
        _repositorio.AgregarUsuario(usuario);
    }
}