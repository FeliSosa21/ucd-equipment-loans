using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

internal sealed class SqlContextQueFallaAlGuardar : SqlContext
{
    public SqlContextQueFallaAlGuardar(DbContextOptions<SqlContext> opciones) : base(opciones)
    {
    }

    public override int SaveChanges()
    {
        throw new DbUpdateException("Se viola un indice unico");
    }
}