using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class TipoUsuarioConfiguracion : IEntityTypeConfiguration<TipoUsuario>
{
    public void Configure(EntityTypeBuilder<TipoUsuario> builder)
    {
        builder.ToTable("TiposDeUsuario");

        builder.HasKey(tipo => tipo.Id);

        builder.OwnsOne<PresentacionTipoUsuario>("presentacion", presentacion =>
        {
            presentacion.Property(datos => datos.Nombre).HasColumnName("Nombre").IsRequired();
            presentacion.Property(datos => datos.Descripcion).HasColumnName("Descripcion").IsRequired();
            presentacion.Property(datos => datos.Imagen).HasColumnName("Imagen").IsRequired();
        });

        builder.Navigation("presentacion").IsRequired();
    }
}
