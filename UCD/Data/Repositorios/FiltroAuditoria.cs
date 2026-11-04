namespace Data.Repositorios;

public class FiltroAuditoria
{
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }
    public string? DniUsuario { get; init; }
}
