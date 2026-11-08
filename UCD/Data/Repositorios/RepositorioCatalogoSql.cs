using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioCatalogoSql
{
    private readonly SqlContext _contexto;

    private IQueryable<Catalogo> CatalogosActivos =>
        _contexto.Catalogos.Where(catalogo => catalogo.Activo);

    public RepositorioCatalogoSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public IReadOnlyList<Catalogo> ObtenerCatalogosActivos()
    {
        return CatalogosActivos
            .OrderBy(catalogo => catalogo.Nombre)
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
        return CatalogosActivos
            .Include(catalogo => catalogo.Tipos)
            .ThenInclude(tipo => tipo.Unidades)
            .FirstOrDefault(catalogo => catalogo.Id == id);
    }

    public bool ExisteActivoConNombre(string nombre, int? idExcluido)
    {
        ArgumentNullException.ThrowIfNull(nombre);

        string nombreBuscado = nombre.Trim().ToLower();

        return CatalogosActivos
            .Any(catalogo => catalogo.Nombre.Trim().ToLower() == nombreBuscado
                             && catalogo.Id != idExcluido);
    }

    public bool ExisteUnidadConIdInstitucional(string idInstitucional)
    {
        ArgumentNullException.ThrowIfNull(idInstitucional);

        return _contexto.Catalogos
            .SelectMany(catalogo => catalogo.Tipos)
            .SelectMany(tipo => tipo.Unidades)
            .Any(unidad => unidad.IdInstitucional == idInstitucional);
    }

    public void ActualizarCatalogo(Catalogo catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);

        _contexto.SaveChanges();
    }

    public TipoEquipamiento? ObtenerTipoPorNombre(int idCatalogo, string nombre)
    {
        ArgumentNullException.ThrowIfNull(nombre);

        string nombreBuscado = nombre.Trim().ToLower();

        return CatalogosActivos
            .Where(catalogo => catalogo.Id == idCatalogo)
            .SelectMany(catalogo => catalogo.Tipos)
            .Include(tipo => tipo.Unidades)
            .FirstOrDefault(tipo => tipo.Nombre.Trim().ToLower() == nombreBuscado);
    }

    public IReadOnlyList<Catalogo> ObtenerCatalogosParaExportar()
    {
        return _contexto.Catalogos
            .Include(catalogo => catalogo.Tipos)
            .ToList();
    }
}
