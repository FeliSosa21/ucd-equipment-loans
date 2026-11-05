using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioUsuarioSql
{
    private readonly SqlContext _contexto;

    public RepositorioUsuarioSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    private IQueryable<Usuario> UsuariosConTipo =>
        _contexto.Usuarios.Include(usuario => usuario.Tipo);

    public void AgregarUsuario(Usuario usuario)
    {
        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();
    }

    public Usuario? ObtenerUsuarioPorDni(string dni)
    {
        return UsuariosConTipo.FirstOrDefault(usuario => usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        string buscado = correo.ToLower();

        return UsuariosConTipo
            .FirstOrDefault(usuario => usuario.Correo.ToLower() == buscado);
    }

    private IQueryable<Usuario> FiltrarPor(string? filtro)
    {
        IQueryable<Usuario> usuarios = UsuariosConTipo;

        if (filtro is not null)
        {
            string buscado = filtro.ToLower();
            usuarios = usuarios.Where(usuario => usuario.Nombre.ToLower().Contains(buscado)
                                                 || usuario.Apellido.ToLower().Contains(buscado)
                                                 || usuario.Dni.Contains(buscado)
                                                 || usuario.Correo.ToLower().Contains(buscado));
        }
        return usuarios;
    }

    public IReadOnlyList<Usuario> ObtenerUsuarios(FiltroUsuarios filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<Usuario>();
        }
        return FiltrarPor(filtro.Texto)
            .OrderBy(usuario=> usuario.Apellido)
            .ToList();
    }
    

    public void ActualizarUsuario(Usuario usuario)
    {
        _contexto.SaveChanges();
    }
}
