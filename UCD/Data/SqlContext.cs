using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class SqlContext : DbContext
{
    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }
    public DbSet<TipoUsuario> TiposDeUsuario { get; set; }
    public DbSet<Devolucion> Devoluciones { get; set; }

    public SqlContext(DbContextOptions<SqlContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlContext).Assembly);
    }
}
