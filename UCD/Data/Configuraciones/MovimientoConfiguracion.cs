using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class MovimientoConfiguracion : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("Movimientos");

        builder.HasKey(movimiento => movimiento.Id);

        builder.Property(movimiento => movimiento.Tipo).HasConversion<string>();

        builder.HasOne(movimiento => movimiento.Prestamo)
            .WithMany()
            .HasForeignKey("PrestamoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movimiento => movimiento.Evaluador)
            .WithMany()
            .HasForeignKey("EvaluadorId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movimiento => movimiento.Unidad)
            .WithMany()
            .HasForeignKey("UnidadId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne<RegistroDeAccion>("_registro", registro =>
        {
            registro.Property(accion => accion.FechaHora).HasColumnName("FechaHora").IsRequired();

            registro.HasOne(accion => accion.Responsable)
                .WithMany()
                .HasForeignKey("ResponsableId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation("_registro").IsRequired();

        builder.Ignore(movimiento => movimiento.FechaHora);
        builder.Ignore(movimiento => movimiento.Responsable);
        builder.Ignore(movimiento => movimiento.Solicitante);
    }
}
