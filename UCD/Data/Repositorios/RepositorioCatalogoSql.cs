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
        return _contexto.Catalogos
            .Where(catalogo => catalogo.Activo)
            .ToList();
    }

    public void AgregarCatalogo(Catalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);

        _contexto.Catalogos.Add(catalogo);
        _contexto.SaveChanges();
    }

    public Catalogo? ObtenerCatalogoActivoPorId(int id)
    {
        return _contexto.Catalogos
            .Include(catalogo => catalogo.Tipos)
            .ThenInclude(equipamiento => equipamiento.Unidades)
            .FirstOrDefault(catalogo => catalogo.Id == id && catalogo.Activo);
    }
}
