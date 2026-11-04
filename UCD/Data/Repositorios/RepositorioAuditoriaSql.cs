using Dominio;

namespace Data.Repositorios;

public class RepositorioAuditoriaSql
{
    private readonly SqlContext _contexto;

    public RepositorioAuditoriaSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarRegistro(RegistroAuditoria registro)
    {
    }
}
