using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

public class RepositorioUsuarioSql
{
    private readonly SqlContext _contexto;

    public RepositorioUsuarioSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    private IQueryable<Usuario> UsuariosConTipo =>
        _contexto.Usuarios.Include(usuario => usuario.Tipo);

    public void AgregarUsuario(Usuario usuario)
    {
        ValidarQueNoExista(usuario);

        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();
    }

    public Usuario? ObtenerUsuarioPorDni(string dni)
    {
        return UsuariosConTipo.FirstOrDefault(usuario => usuario.Dni == dni);
    }

    public Usuario? ObtenerUsuarioPorCorreo(string correo)
    {
        string correoEnMinusculas = correo.ToLower();

        return UsuariosConTipo
            .FirstOrDefault(usuario => usuario.Correo.ToLower() == correoEnMinusculas);
    }

    public IReadOnlyList<Usuario> ObtenerUsuarios(FiltroUsuarios filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<Usuario>();
        }

        IQueryable<Usuario> usuarios = FiltrarPorTexto(UsuariosConTipo, filtro.Texto);
        usuarios = FiltrarPorTipo(usuarios, filtro.Tipo);

        return usuarios
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Dni)
            .ToList();
    }

    public void ActualizarUsuario(Usuario usuario)
    {
        _contexto.SaveChanges();
    }

    private IQueryable<Usuario> FiltrarPorTexto(IQueryable<Usuario> usuarios, string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return usuarios;
        }

        string textoEnMinusculas = texto.Trim().ToLower();

        return usuarios.Where(usuario => usuario.Nombre.ToLower().Contains(textoEnMinusculas)
                                         || usuario.Apellido.ToLower().Contains(textoEnMinusculas)
                                         || usuario.Dni.Contains(textoEnMinusculas)
                                         || usuario.Correo.ToLower().Contains(textoEnMinusculas));
    }

    private IQueryable<Usuario> FiltrarPorTipo(IQueryable<Usuario> usuarios, TipoUsuario? tipo)
    {
        if (tipo is null)
        {
            return usuarios;
        }

        return usuarios.Where(usuario => usuario.Tipo == tipo);
    }

    private void ValidarQueNoExista(Usuario usuario)
    {
        if (_contexto.Usuarios.Any(existente => existente.Dni == usuario.Dni))
        {
            throw new InvalidOperationException($"Ya existe un usuario con el DNI {usuario.Dni}");
        }

        string correoEnMinusculas = usuario.Correo.ToLower();

        if (_contexto.Usuarios.Any(existente => existente.Correo.ToLower() == correoEnMinusculas))
        {
            throw new InvalidOperationException($"Ya existe un usuario con el correo {usuario.Correo}");
        }
    }
}
