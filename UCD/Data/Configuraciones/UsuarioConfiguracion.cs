using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(usuario => usuario.Id);
        builder.HasIndex(usuario => usuario.Dni).IsUnique();
        builder.HasIndex(usuario => usuario.Correo).IsUnique();

        builder.Property<string>("Contrasena");

        builder.Property(usuario => usuario.Tipo)
            .HasConversion(
                tipo => tipo.Nombre,
                nombre => TipoUsuario.Todos().First(tipo => tipo.Nombre == nombre));
    }
}