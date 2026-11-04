using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioTipoUsuarioSql
{
    private const string CampoPresentacion = "presentacion";

    private readonly SqlContext _contexto;

    public RepositorioTipoUsuarioSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarTipo(TipoUsuario tipo)
    {
        _contexto.TiposDeUsuario.Add(tipo);
        _contexto.SaveChanges();
    }

    public TipoUsuario? ObtenerTipoPorNombre(string nombre)
    {
        return _contexto.TiposDeUsuario
            .FirstOrDefault(tipo => EF.Property<PresentacionTipoUsuario>(tipo, CampoPresentacion).Nombre == nombre);
    }

    public IReadOnlyList<TipoUsuario> ObtenerTodos()
    {
        return new List<TipoUsuario>();
    }
}
