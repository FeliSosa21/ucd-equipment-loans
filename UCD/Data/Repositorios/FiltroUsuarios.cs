namespace Data.Repositorios;
using Dominio;

public class FiltroUsuarios
{
    public string? Texto { get; init; }
    public TipoUsuario? Tipo { get; init; }

    public bool TieneCriterios()
    {
        return !string.IsNullOrWhiteSpace(Texto) || Tipo is not null;
    }
}