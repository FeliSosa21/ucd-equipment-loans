using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Gestion de los seis tipos de usuario predefinidos (req. 2.1): el administrador los lista y modifica
// su descripcion, imagen y topes. No se crean ni eliminan tipos, ni se cambia su nombre o permisos (foro 2.1).
public class ServicioTiposUsuario
{
    private readonly RepositorioTipoUsuario _repositorioTipos;
    private readonly RepositorioUsuario _repositorioUsuarios;

    public ServicioTiposUsuario(RepositorioTipoUsuario repositorioTipos, RepositorioUsuario repositorioUsuarios)
    {
        _repositorioTipos = repositorioTipos;
        _repositorioUsuarios = repositorioUsuarios;
    }

    public List<TipoUsuarioDto> ListarTipos(string dniResponsable)
    {
        ValidarQueSeaAdministradorActivo(dniResponsable);
        return _repositorioTipos.ObtenerTodos().Select(ADto).ToList();
    }

    public void ModificarTipo(TipoUsuarioDto dto, string dniResponsable)
    {
        ValidarQueSeaAdministradorActivo(dniResponsable);
        TipoUsuario? tipo = _repositorioTipos.ObtenerTipoPorNombre(dto.Nombre);

        if (tipo is null)
        {
            throw new ServicioException("El tipo de usuario no existe");
        }

        TraduccionDeErrores.Ejecutar(() =>
        {
            tipo.ModificarDescripcion(dto.Descripcion);
            tipo.ModificarImagen(dto.Imagen);

            if (dto.MaxUnidadesPorCatalogo.HasValue && dto.MaxPrestamosConcurrentes.HasValue)
            {
                tipo.ModificarTopes(dto.MaxUnidadesPorCatalogo.Value, dto.MaxPrestamosConcurrentes.Value);
            }
        });
    }

    private void ValidarQueSeaAdministradorActivo(string dniResponsable)
    {
        Usuario? responsable = _repositorioUsuarios.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || responsable.Estado != EstadoUsuario.Activo
            || !responsable.TienePermiso(Permiso.GestionarUsuarios))
        {
            throw new ServicioException("Solo un administrador activo puede gestionar los tipos de usuario");
        }
    }

    private static TipoUsuarioDto ADto(TipoUsuario tipo)
    {
        return new TipoUsuarioDto
        {
            Nombre = tipo.Nombre,
            Descripcion = tipo.Descripcion,
            Imagen = tipo.Imagen,
            MaxUnidadesPorCatalogo = tipo.MaxUnidadesPorCatalogo,
            MaxPrestamosConcurrentes = tipo.MaxPrestamosConcurrentes
        };
    }
}
