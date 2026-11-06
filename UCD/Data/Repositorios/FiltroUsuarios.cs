using Dominio;

namespace Data.Repositorios;

public class FiltroUsuarios
{
    public string? Texto { get; init; }
    public TipoUsuario? Tipo { get; init; }
    public string? TextoNormalizado => Texto?.Trim().ToLower();
    public bool TieneCriterios()
    {
        return !string.IsNullOrWhiteSpace(Texto) || Tipo is not null;
    }
}
