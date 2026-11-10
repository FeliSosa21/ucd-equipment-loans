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
        _contexto.Notificaciones.Add(notificacion);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<Notificacion> ObtenerNoLeidasDeUsuario(string dni)
    {
        return _contexto.Notificaciones
            .Where(notificacion => notificacion.Prestamo.Solicitante.Dni == dni)
            .ToList();
    }
}
