using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        
        builder.HasKey(usuario => usuario.Id);
        builder.HasIndex(usuario => usuario.Dni).IsUnique();
        builder.HasIndex(usuario => usuario.Correo).IsUnique();

        builder.Property(usuario => usuario.Dni).IsRequired().HasMaxLength(Usuario.LargoDni);
        builder.Property(usuario => usuario.Nombre).IsRequired().HasMaxLength(Usuario.LargoMaximoNombreOApellido);
        builder.Property(usuario => usuario.Apellido).IsRequired().HasMaxLength(Usuario.LargoMaximoNombreOApellido);
        builder.Property(usuario => usuario.Correo).IsRequired();
        
        builder.Property(usuario => usuario.Estado).HasConversion<string>();
        
        builder.Property<string>("Contrasena").IsRequired();
        
        builder.Ignore(usuario => usuario.NombreCompleto);
        
        builder.HasOne(usuario => usuario.Tipo)
            .WithMany()
            .HasForeignKey("TipoUsuarioId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}