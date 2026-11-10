using Data;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

// InMemory no simula los errores de la base: este contexto rechaza el guardado si hay una TEntidad nueva pendiente,
// para probar que una falla a mitad de una operacion no deja nada a medias.
internal sealed class ContextoQueFallaSiHayPendiente<TEntidad> : SqlContext where TEntidad : class
{
    public ContextoQueFallaSiHayPendiente(DbContextOptions<SqlContext> opciones) : base(opciones)
    {
    }

    public override int SaveChanges()
    {
        if (ChangeTracker.Entries<TEntidad>().Any(entrada => entrada.State == EntityState.Added))
        {
            throw new DbUpdateException($"Se rechaza el guardado de {typeof(TEntidad).Name}");
        }

        return base.SaveChanges();
    }
}
