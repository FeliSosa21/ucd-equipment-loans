using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class DevolucionConfiguracion : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("Devoluciones");

        builder.HasKey(devolucion => devolucion.Id);

        builder.HasOne(devolucion => devolucion.Prestamo)
            .WithMany()
            .HasForeignKey("PrestamoId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne<RegistroDeAccion>("_registro", registro =>
        {
            registro.Property(accion => accion.FechaHora).HasColumnName("FechaHora").IsRequired();

            registro.Property<int?>("ResponsableId").HasColumnName("RegistradaPorId");

            registro.HasOne(accion => accion.Responsable)
                .WithMany()
                .HasForeignKey("ResponsableId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation("_registro").IsRequired();

        builder.OwnsMany<UnidadDevuelta>("_unidades", unidades =>
        {
            unidades.ToTable("UnidadesDevueltas");
            unidades.WithOwner().HasForeignKey("DevolucionId");
            unidades.Property<int>("Id");
            unidades.HasKey("Id");

            unidades.Property(unidad => unidad.Condicion).HasConversion<string>();
            unidades.Property(unidad => unidad.Observaciones).HasMaxLength(UnidadDevuelta.LargoMaximoObservaciones);

            unidades.HasOne(unidad => unidad.Unidad)
                .WithMany()
                .HasForeignKey("UnidadFisicaId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Ignore(devolucion => devolucion.RegistradaPor);
        builder.Ignore(devolucion => devolucion.FechaHoraDevolucion);
        builder.Ignore(devolucion => devolucion.Unidades);
    }
}
