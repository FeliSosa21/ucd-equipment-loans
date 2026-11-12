using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class NotificacionConfiguracion : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(notificacion => notificacion.Id);

        builder.Property(notificacion => notificacion.Tipo).HasConversion<string>();

        builder.HasOne(notificacion => notificacion.Prestamo)
            .WithMany()
            .HasForeignKey("PrestamoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
