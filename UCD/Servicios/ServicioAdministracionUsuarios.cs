using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Lo que hace el administrador con las cuentas: listar, asignar tipo, desactivar y reactivar.
public class ServicioAdministracionUsuarios
{
    private readonly RepositorioUsuario _repositorio;

    public ServicioAdministracionUsuarios(RepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public List<UsuarioResumenDto> ListarUsuarios()
    {
        return _repositorio.ObtenerTodos().Select(CrearResumen).ToList();
    }

    public List<string> NombresDeTipos()
    {
        return TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList();
    }

    public void AsignarTipo(string dni, string nombreTipo)
    {
        Usuario usuario = ObtenerUsuario(dni);
        TipoUsuario? tipo = TipoUsuario.Todos().FirstOrDefault(t => t.Nombre == nombreTipo);

        if (tipo is null)
        {
            throw new ServicioException("El tipo de usuario no existe");
        }

        usuario.AsignarTipo(tipo);
    }

    public void Desactivar(string dni)
    {
        ObtenerUsuario(dni).Desactivar();
    }

    public void Reactivar(string dni)
    {
        ObtenerUsuario(dni).Reactivar();
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
