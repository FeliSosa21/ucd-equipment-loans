using Dominio;

namespace Data.Repositorios;

public class RepositorioTipoUsuario
{
    private readonly Memoria _memoria;

    public RepositorioTipoUsuario(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarTipo(TipoUsuario tipo)
    {
        _memoria.TiposDeUsuario.Add(tipo);
    }

    public TipoUsuario? ObtenerTipoPorNombre(string nombre)
    {
        return _memoria.TiposDeUsuario.FirstOrDefault(tipo => tipo.Nombre == nombre);
    }

    public IReadOnlyList<TipoUsuario> ObtenerTodos()
    {
        return _memoria.TiposDeUsuario.ToList().AsReadOnly();
    }
}
