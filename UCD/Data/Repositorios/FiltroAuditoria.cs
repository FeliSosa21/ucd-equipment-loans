namespace Data.Repositorios;

public class FiltroAuditoria
{
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }
    public string? DniUsuario { get; init; }

    public bool TieneCriterios()
    {
        return Desde is not null || Hasta is not null || DniUsuario is not null;
    }
}
