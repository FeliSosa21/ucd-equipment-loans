using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioUsuario
{
    private readonly RepositorioUsuario _repositorio;
    private readonly RepositorioTipoUsuario _repositorioTipos;
    private readonly RegistradorDeAuditoria _auditoria;

    public ServicioUsuario(RepositorioUsuario repositorio, RepositorioTipoUsuario repositorioTipos,
        RegistradorDeAuditoria auditoria)
    {
        _repositorio = repositorio;
        _repositorioTipos = repositorioTipos;
        _auditoria = auditoria;
    }

    public void Registrar(UsuarioDto dto)
    {
        Usuario usuario = Agregar(dto);
        _auditoria.RegistrarDeUsuario(AccionAuditada.RegistroDeUsuario, usuario,
            $"Se registro {RegistradorDeAuditoria.Describir(usuario)}");
    }

    internal Usuario Agregar(UsuarioDto dto)
    {
        Usuario usuario = CrearUsuario(dto);
        usuario.AsignarTipo(_repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.SinPermisos().Nombre)!);

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