using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class ConfiguracionUnidadFisica : IEntityTypeConfiguration<UnidadFisica>
{
    public void Configure(EntityTypeBuilder<UnidadFisica> builder)
    {
        builder.ToTable("UnidadesFisica");

        builder.HasKey(unidad => unidad.Id);

        builder.Property(unidad => unidad.IdInstitucional).IsRequired().HasMaxLength(UnidadFisica.LargoIdInstitucional);

        builder.HasIndex(unidad => unidad.IdInstitucional).IsUnique();

        builder.Property(unidad => unidad.UbiFisica).IsRequired().HasMaxLength(UnidadFisica.LargoMaximoUbicacion);

        builder.Property(unidad => unidad.NumSerie).HasMaxLength(UnidadFisica.LargoMaxNumSerie);

        builder.Property(unidad => unidad.Observaciones).HasMaxLength(UnidadFisica.LargoMaxObservaciones);

        builder.Property(unidad => unidad.Estado).HasConversion<string>();
    }
}