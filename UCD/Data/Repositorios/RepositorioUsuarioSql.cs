using Dominio;

namespace Data.Repositorios;

public class RepositorioUsuarioSql
{
    private readonly SqlContext _contexto;
    
    public RepositorioUsuarioSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarUsuario(Usuario usuario)
    {
        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();
    }

    public Usuario? ObtenerUsuarioPorDni(string dni)
    {
        return _contexto.Usuarios.FirstOrDefault(usuario => usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        string buscado = correo.ToLower();
        return _contexto.Usuarios.FirstOrDefault(usuario => usuario.Correo.ToLower() == buscado);
    }

    public IReadOnlyList<Usuario> ObtenerPaginaDeUsuarios(string? filtro, int pagina, int tamanoPagina)
    {
        return FiltrarPor(filtro)
            .OrderBy(usuario => usuario.Apellido)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }
    private IQueryable<Usuario> FiltrarPor(string? filtro)
    {
        IQueryable<Usuario> usuarios = _contexto.Usuarios;

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
    
    public int ContarUsuarios(string? filtro)
    {
        return FiltrarPor(filtro).Count();
    }
}