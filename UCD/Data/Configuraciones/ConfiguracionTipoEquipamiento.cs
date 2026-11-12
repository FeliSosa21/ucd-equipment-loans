using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class ConfiguracionTipoEquipamiento : IEntityTypeConfiguration<TipoEquipamiento>
{
    public void Configure(EntityTypeBuilder<TipoEquipamiento> builder)
    {
        builder.ToTable("TiposEquipamiento");

        builder.HasKey(tipo => tipo.Id);

        builder.Property(tipo => tipo.Nombre).IsRequired().HasMaxLength(TipoEquipamiento.LargoMaximoNombre);

        builder.Property(tipo => tipo.Descripcion).IsRequired().HasMaxLength(TipoEquipamiento.LargoMaximoDescripcion);

        builder.Property(tipo => tipo.Estado).HasConversion<string>();

        builder.Property(tipo => tipo.Categoria).HasConversion<string>();

        builder.Ignore(tipo => tipo.EstaActivo);

        builder.Property<int>("CatalogoId");

        builder.HasIndex("CatalogoId", nameof(TipoEquipamiento.Nombre)).IsUnique();

        builder.HasMany(tipo => tipo.Unidades)
            .WithOne()
            .HasForeignKey("TipoEquipamientoId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(tipo => tipo.Unidades)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}