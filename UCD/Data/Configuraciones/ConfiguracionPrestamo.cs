using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class ConfiguracionPrestamo : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> builder)
    {
        builder.ToTable("Prestamos");

        builder.HasKey(prestamo => prestamo.Id);

        builder.Property(prestamo => prestamo.Id).ValueGeneratedNever();

        builder.Property(prestamo => prestamo.Estado).HasConversion<string>();

        builder.Property(prestamo => prestamo.MotivoRechazo).HasMaxLength(Prestamo.LargoMaximoMotivoRechazo);

        builder.HasOne(prestamo => prestamo.Solicitante)
            .WithMany()
            .HasForeignKey("SolicitanteId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(prestamo => prestamo.Evaluador)
            .WithMany()
            .HasForeignKey("EvaluadorId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(prestamo => prestamo.TipoEquipamiento)
            .WithMany()
            .HasForeignKey("TipoEquipamientoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(prestamo => prestamo.UnidadesAsignadas).
            WithMany().
            UsingEntity(
                "UnidadesAsignadas",
                asignacion => asignacion.HasOne(typeof(UnidadFisica)).WithMany()
                    .HasForeignKey("UnidadFisicaId").OnDelete(DeleteBehavior.Restrict),
                asignacion => asignacion.HasOne(typeof(Prestamo)).WithMany()
                    .HasForeignKey("PrestamoId").OnDelete(DeleteBehavior.Restrict),
                tabla => tabla.HasKey("PrestamoId", "UnidadFisicaId"));

        builder.Navigation(prestamo => prestamo.UnidadesAsignadas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}