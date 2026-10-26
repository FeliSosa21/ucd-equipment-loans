namespace Dominio;

public class RegistroAuditoria
{
    public DateTime FechaHora { get; private set; }
    public Usuario? Responsable { get; private set; }
    public AccionAuditada Accion { get; private set; }
    public string Detalle { get; private set; } = string.Empty;

    public RegistroAuditoria(DateTime fechaHora, Usuario? responsable, AccionAuditada accion, string detalle)
    {
        FechaHora = fechaHora;
        Responsable = responsable;
        Accion = accion;
        Detalle = detalle;
    }

    public bool EsDelSistema()
    {
        return Responsable is null;
    }
}
