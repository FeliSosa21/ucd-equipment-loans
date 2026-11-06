using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Repositorios;

public class RepositorioCatalogoSql
{
    private readonly SqlContext _contexto;

    public RepositorioCatalogoSql(SqlContext context)
    {
        _contexto = context;
    }

    public IReadOnlyList<Catalogo> ObtenerCatalogosActivos()
    {
        return _contexto.Catalogos.ToList();
    }
}
