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
        return _repositorioTipos.ObtenerTodos().Select(ADto).ToList();
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
