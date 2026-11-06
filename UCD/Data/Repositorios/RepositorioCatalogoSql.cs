using Dominio;
using Microsoft.EntityFrameworkCore;

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
        ArgumentNullException.ThrowIfNull(catalogo);

        _contexto.Catalogos.Add(catalogo);
        _contexto.SaveChanges();
    }

    public Catalogo? ObtenerCatalogoActivoPorId(int id)
    {
        return _contexto.Catalogos.FirstOrDefault(catalogo => catalogo.Id == id);
    }
}
