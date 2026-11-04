using Dominio;

namespace Data.Repositorios;

public class RepositorioTipoUsuarioSql
{
    private readonly SqlContext _contexto;

    public RepositorioTipoUsuarioSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarTipo(TipoUsuario tipo)
    {
        _contexto.Add(tipo);
        _contexto.SaveChanges();
    }

    public TipoUsuario? ObtenerTipoPorNombre(string nombre)
    {
        return null;
    }
}
