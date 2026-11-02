namespace Dominio;

public class RegistroAuditoria
{
    private const int LargoMaximoDetalle = 200;

    private readonly RegistroDeAccion _registro;

    public DateTime FechaHora => _registro.FechaHora;
    public Usuario? Responsable => _registro.Responsable;
    public AccionAuditada Accion { get; private set; }
    public string Detalle { get; private set; } = string.Empty;

    public RegistroAuditoria(RegistroDeAccion registro, AccionAuditada accion, string detalle)
    {
        string detalleRecortado = detalle?.Trim() ?? string.Empty;

        ValidarRegistro(registro);
        ValidarAccion(accion);
        ValidarDetalle(detalleRecortado);

        _registro = registro;
        Accion = accion;
        Detalle = detalleRecortado;
    }

    public bool EsDelSistema()
    {
        return Responsable is null;
    }

    private static void ValidarRegistro(RegistroDeAccion registro)
    {
        if (registro is null)
        {
            throw new DominioException("El registro de auditoria debe tener quien y cuando");
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
