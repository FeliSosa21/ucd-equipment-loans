using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class RegistroAuditoriaConfiguracion : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");

        builder.HasKey(registro => registro.Id);

        builder.Property(registro => registro.Accion).HasConversion<string>();

        builder.Property(registro => registro.Detalle)
            .HasMaxLength(RegistroAuditoria.LargoMaximoDetalle)
            .IsRequired();

        builder.OwnsOne<RegistroDeAccion>(NombresDeMapeo.CampoRegistro, registro =>
        {
            registro.Property(accion => accion.FechaHora).HasColumnName("FechaHora").IsRequired();

            registro.Property<int?>("ResponsableId").HasColumnName("ResponsableId");

            registro.HasOne(accion => accion.Responsable)
                .WithMany()
                .HasForeignKey("ResponsableId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation(NombresDeMapeo.CampoRegistro).IsRequired();

        builder.Ignore(registro => registro.FechaHora);
        builder.Ignore(registro => registro.Responsable);
    }
}
