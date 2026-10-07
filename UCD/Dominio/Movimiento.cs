namespace Dominio;

// Registro historico del req. 10: un movimiento por unidad para prestamo, aprobacion y devolucion; solicitud y rechazo, sin unidad. No se modifica ni se elimina.
public class Movimiento
{
    public DateTime FechaHora { get; }
    public TipoMovimiento Tipo { get; }
    public Usuario Solicitante { get; }
    public Usuario? Evaluador { get; }
    public UnidadFisica? Unidad { get; }

    public Movimiento(DateTime fechaHora, TipoMovimiento tipo, Usuario solicitante, Usuario? evaluador,
        UnidadFisica? unidad)
    {
        ValidarTipo(tipo);
        ValidarSolicitante(solicitante);
        ValidarUnidad(tipo, unidad);

        FechaHora = fechaHora;
        Tipo = tipo;
        Solicitante = solicitante;
        Evaluador = evaluador;
        Unidad = unidad;
    }

    private static void ValidarTipo(TipoMovimiento tipo)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new DominioException("El movimiento debe tener un tipo valido");
        }
    }

    private static void ValidarSolicitante(Usuario solicitante)
    {
        if (solicitante is null)
        {
            throw new DominioException("El movimiento debe tener un solicitante");
        }
    }

    private static void ValidarUnidad(TipoMovimiento tipo, UnidadFisica? unidad)
    {
        bool esDeUnidad = tipo == TipoMovimiento.Prestamo || tipo == TipoMovimiento.Devolucion
            || tipo == TipoMovimiento.Aprobacion;

        if (esDeUnidad && unidad is null)
        {
            throw new DominioException("El movimiento debe tener una unidad");
        }
    }
}
