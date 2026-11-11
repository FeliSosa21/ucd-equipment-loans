using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

// Copia del de Servicios.Tests, que es internal en otro proyecto: existe para probar la regla 5 del repositorio.
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
