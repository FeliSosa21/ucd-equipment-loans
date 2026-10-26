namespace Dominio;

public class RegistroAuditoria
{
    public DateTime FechaHora { get; private set; }
    public Usuario? Responsable { get; private set; }
    public AccionAuditada Accion { get; private set; }
    public string Detalle { get; private set; } = string.Empty;

    public RegistroAuditoria(DateTime fechaHora, Usuario? responsable, AccionAuditada accion, string detalle)
    {
        ValidarFechaHora(fechaHora);
        ValidarAccion(accion);

        FechaHora = fechaHora;
        Responsable = responsable;
        Accion = accion;
        Detalle = detalle;
    }

    public bool EsDelSistema()
    {
        return Responsable is null;
    }

    private static void ValidarFechaHora(DateTime fechaHora)
    {
        if (fechaHora == default)
        {
            throw new DominioException("El registro de auditoria debe tener fecha y hora");
        }
    }

    private static void ValidarAccion(AccionAuditada accion)
    {
        if (!Enum.IsDefined(accion))
        {
            throw new DominioException("El registro de auditoria debe tener una accion valida");
        }
    }
}
