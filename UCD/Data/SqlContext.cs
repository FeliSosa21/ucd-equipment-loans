using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class SqlContext : DbContext
{
    public DbSet<Catalogo> Catalogos { get; set; }

    public SqlContext(DbContextOptions<SqlContext> options) : base(options)
    {
        if (Database.IsRelational())
        {
            Database.Migrate();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlContext).Assembly);
    }
}

