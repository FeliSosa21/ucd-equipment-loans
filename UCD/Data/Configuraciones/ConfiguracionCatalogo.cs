using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class ConfiguracionCatalogo : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        builder.ToTable("Catalogos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(Catalogo.LargoMaximoNombre);

        builder.HasMany(c => c.Tipos)
            .WithOne()
            .HasForeignKey("CatalogoId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Tipos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}