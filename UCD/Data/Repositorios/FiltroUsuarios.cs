using Dominio;

namespace Data.Repositorios;

public class FiltroUsuarios
{
    public string? Texto { get; init; }
    public TipoUsuario? Tipo { get; init; }

    public string? TextoNormalizado =>
        string.IsNullOrWhiteSpace(Texto) ? null : Texto.Trim().ToLower();

    public bool TieneCriterios()
    {
        return TextoNormalizado is not null || Tipo is not null;
    }
}
