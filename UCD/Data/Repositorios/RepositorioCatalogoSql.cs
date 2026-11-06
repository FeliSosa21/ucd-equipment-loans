using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Repositorios;

public class RepositorioCatalogoSql
{
    private readonly SqlContext _contexto;

    public RepositorioCatalogoSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public IReadOnlyList<Catalogo> ObtenerCatalogosActivos()
    {
        return _contexto.Catalogos.ToList();
    }

    public void AgregarCatalogo(Catalogo catalogo)
    {
        _contexto.Catalogos.Add(catalogo);
        _contexto.SaveChanges();
    }
}
