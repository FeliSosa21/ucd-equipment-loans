using Dominio;

namespace Data.Repositorios;

public class RepositorioNotificacionSql
{
    private readonly SqlContext _contexto;

    public RepositorioNotificacionSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarNotificacion(Notificacion notificacion)
    {
    }
}
