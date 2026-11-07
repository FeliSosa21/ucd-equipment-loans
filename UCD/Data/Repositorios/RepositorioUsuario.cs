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
        return _memoria.Usuarios.FirstOrDefault(usuario => !usuario.EstaEliminado && usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        return _memoria.Usuarios.FirstOrDefault(usuario =>
            string.Equals(usuario.Correo, correo, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<Usuario> ObtenerPaginaDeUsuarios(string? filtro, int pagina, int tamanoPagina)
    {
        return FiltrarPor(filtro)
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Dni)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }

    public int ContarUsuarios(string? filtro)
    {
        return FiltrarPor(filtro).Count();
    }

    public IReadOnlyList<Usuario> ObtenerUsuarios(FiltroUsuarios filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<Usuario>();
        }

        IEnumerable<Usuario> usuarios = _memoria.Usuarios;
        string? texto = filtro.TextoNormalizado;

        if (texto is not null)
        {
            usuarios = usuarios.Where(usuario => usuario.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                                 || usuario.Apellido.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                                 || usuario.Dni.Contains(texto)
                                                 || usuario.Correo.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (filtro.Tipo is not null)
        {
            usuarios = usuarios.Where(usuario => usuario.Tipo == filtro.Tipo);
        }

        return usuarios
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Dni)
            .ToList();
    }

    public void EliminarUsuario(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        usuario.Eliminar();
    }

    // Busca el texto en nombre, apellido, cedula o correo; sin texto no filtra.
    private IEnumerable<Usuario> FiltrarPor(string? filtro)
    {
        string busqueda = filtro?.Trim() ?? string.Empty;

        return _memoria.Usuarios.Where(usuario => $"{usuario.Nombre} {usuario.Apellido} {usuario.Dni} {usuario.Correo}"
            .Contains(busqueda, StringComparison.OrdinalIgnoreCase));
    }
}
