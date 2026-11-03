using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class ConfiguracionPrestamo : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> builder)
    {
        builder.HasMany(prestamo => prestamo.UnidadesAsignadas)
            .WithMany();

        builder.Navigation(prestamo => prestamo.UnidadesAsignadas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}