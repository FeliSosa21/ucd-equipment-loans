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

}