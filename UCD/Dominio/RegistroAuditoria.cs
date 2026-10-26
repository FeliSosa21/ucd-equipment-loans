namespace Dominio;

public class RegistroAuditoria
{
    private const int LargoMaximoDetalle = 200;

    public DateTime FechaHora { get; private set; }
    public Usuario? Responsable { get; private set; }
    public AccionAuditada Accion { get; private set; }
    public string Detalle { get; private set; } = string.Empty;

    public RegistroAuditoria(DateTime fechaHora, Usuario? responsable, AccionAuditada accion, string detalle)
    {
        string detalleRecortado = detalle?.Trim() ?? string.Empty;

        ValidarFechaHora(fechaHora);
        ValidarAccion(accion);
        ValidarDetalle(detalleRecortado);

        FechaHora = fechaHora;
        Responsable = responsable;
        Accion = accion;
        Detalle = detalleRecortado;
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

    private static void ValidarDetalle(string detalle)
    {
        if (string.IsNullOrWhiteSpace(detalle))
        {
            throw new DominioException("El detalle no puede estar vacio");
        }

        if (detalle.Length > LargoMaximoDetalle)
        {
            throw new DominioException($"El detalle debe tener como maximo {LargoMaximoDetalle} caracteres");
        }
    }
}
