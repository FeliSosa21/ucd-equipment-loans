using Dominio;

namespace Data.Repositorios;

public class RepositorioUsuario
{
    private readonly Memoria _memoria;

    public RepositorioUsuario(Memoria memoria)
    {
        _memoria = memoria;
    }

    private IEnumerable<Usuario> UsuariosVigentes =>
        _memoria.Usuarios.Where(usuario => !usuario.EstaEliminado);

    public void AgregarUsuario(Usuario usuario)
    {
        _memoria.Usuarios.Add(usuario);
    }

    public Usuario? ObtenerUsuarioPorDni(string dni)
    {
        return UsuariosVigentes.FirstOrDefault(usuario => usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        return UsuariosVigentes.FirstOrDefault(usuario =>
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

        IEnumerable<Usuario> usuarios = FiltrarPorTexto(UsuariosVigentes, filtro.TextoNormalizado);
        usuarios = FiltrarPorTipo(usuarios, filtro.Tipo);

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

    private IEnumerable<Usuario> FiltrarPorTexto(IEnumerable<Usuario> usuarios, string? textoNormalizado)
    {
        if (textoNormalizado is null)
        {
            return usuarios;
        }

        return usuarios.Where(usuario =>
            usuario.Nombre.Contains(textoNormalizado, StringComparison.OrdinalIgnoreCase)
            || usuario.Apellido.Contains(textoNormalizado, StringComparison.OrdinalIgnoreCase)
            || usuario.Dni.Contains(textoNormalizado)
            || usuario.Correo.Contains(textoNormalizado, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<Usuario> FiltrarPorTipo(IEnumerable<Usuario> usuarios, TipoUsuario? tipo)
    {
        if (tipo is null)
        {
            return usuarios;
        }

        return usuarios.Where(usuario => usuario.Tipo == tipo);
    }

    // Busca el texto en nombre, apellido, cedula o correo; sin texto no filtra.
    private IEnumerable<Usuario> FiltrarPor(string? filtro)
    {
        string busqueda = filtro?.Trim() ?? string.Empty;

        return _memoria.Usuarios.Where(usuario => $"{usuario.Nombre} {usuario.Apellido} {usuario.Dni} {usuario.Correo}"
            .Contains(busqueda, StringComparison.OrdinalIgnoreCase));
    }
}
