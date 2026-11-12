using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class TipoUsuarioConfiguracion : IEntityTypeConfiguration<TipoUsuario>
{
    private const char SeparadorPermisos = ',';
    private const int LargoMaximoNombre = 50;

    public void Configure(EntityTypeBuilder<TipoUsuario> builder)
    {
        builder.ToTable("TiposDeUsuario");

        builder.HasKey(tipo => tipo.Id);

        builder.OwnsOne<PresentacionTipoUsuario>("presentacion", presentacion =>
        {
            presentacion.Property(datos => datos.Nombre).HasColumnName("Nombre").IsRequired()
                .HasMaxLength(LargoMaximoNombre);
            presentacion.Property(datos => datos.Descripcion).HasColumnName("Descripcion").IsRequired()
                .HasMaxLength(PresentacionTipoUsuario.LargoMaximoDescripcion);
            presentacion.Property(datos => datos.Imagen).HasColumnName("Imagen").IsRequired();
            presentacion.HasIndex(datos => datos.Nombre).IsUnique();
        });

        builder.Navigation("presentacion").IsRequired();

        builder.OwnsOne<TopesDePrestamo>("topes", topes =>
        {
            topes.Property(limites => limites.MaxUnidadesPorCatalogo).HasColumnName("MaxUnidadesPorCatalogo");
            topes.Property(limites => limites.MaxPrestamosConcurrentes).HasColumnName("MaxPrestamosConcurrentes");
        });

        builder.Property<HashSet<Permiso>>("permisos")
            .HasColumnName("Permisos")
            .HasConversion(permisos => ATexto(permisos), texto => APermisos(texto))
            .Metadata.SetValueComparer(new ValueComparer<HashSet<Permiso>>(
                (unos, otros) => unos!.SetEquals(otros!),
                permisos => permisos.Aggregate(0, (hash, permiso) => hash ^ permiso.GetHashCode()),
                permisos => new HashSet<Permiso>(permisos)));
    }

    private static string ATexto(HashSet<Permiso> permisos)
    {
        return string.Join(SeparadorPermisos, permisos);
    }

    private static HashSet<Permiso> APermisos(string texto)
    {
        return texto.Split(SeparadorPermisos, StringSplitOptions.RemoveEmptyEntries)
            .Select(nombre => Enum.Parse<Permiso>(nombre))
            .ToHashSet();
    }
}
