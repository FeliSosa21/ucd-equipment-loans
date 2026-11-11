using Data;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

// InMemory no aplica los indices unicos: este contexto simula el rechazo de la base al guardar.
internal sealed class ContextoQueFallaAlGuardar : SqlContext
{
    public ContextoQueFallaAlGuardar(DbContextOptions<SqlContext> opciones) : base(opciones)
    {
    }

    public override int SaveChanges()
    {
        throw new DbUpdateException("Se viola un indice unico");
    }
}
