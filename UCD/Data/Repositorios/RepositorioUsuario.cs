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
}