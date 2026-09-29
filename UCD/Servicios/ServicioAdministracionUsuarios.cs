using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Lo que hace el administrador con las cuentas: crear, listar, asignar tipo, desactivar y reactivar.
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

    public void AsignarTipo(string dni, string nombreTipo)
    {
        Usuario usuario = ObtenerUsuario(dni);
        usuario.AsignarTipo(ObtenerTipo(nombreTipo));
    }

    public void Desactivar(string dni)
    {
        ObtenerUsuario(dni).Desactivar();
    }

    public void Reactivar(string dni)
    {
        ObtenerUsuario(dni).Reactivar();
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
