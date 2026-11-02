using Dominio;

namespace Data.Repositorios;

public class RepositorioUsuario
{
    private readonly Memoria _memoria;

    public RepositorioUsuario(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarUsuario(Usuario usuario)
    {
        _memoria.Usuarios.Add(usuario);
    }

    public Usuario? ObtenerUsuarioPorDni(string dni)
    {
        return _memoria.Usuarios.FirstOrDefault(usuario => usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        return _memoria.Usuarios.FirstOrDefault(usuario =>
            string.Equals(usuario.Correo, correo, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<Usuario> ObtenerUsuariosQueCoinciden(string? texto, int maximo)
    {
        string busqueda = texto?.Trim() ?? string.Empty;

        return _memoria.Usuarios
            .Where(usuario => $"{usuario.Nombre} {usuario.Apellido} {usuario.Dni} {usuario.Correo}"
                .Contains(busqueda, StringComparison.OrdinalIgnoreCase))
            .Take(maximo)
            .ToList();
    }
}
