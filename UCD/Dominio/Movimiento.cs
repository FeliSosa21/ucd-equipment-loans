namespace Dominio;

// Registro historico del req. 10: uno por cada unidad prestada o devuelta; no se modifica ni se elimina.
public class Movimiento
{
    public DateTime FechaHora { get; }
    public TipoMovimiento Tipo { get; }
    public Usuario Solicitante { get; }
    public Usuario? Evaluador { get; }
    public UnidadFisica Unidad { get; }

    public Movimiento(DateTime fechaHora, TipoMovimiento tipo, Usuario solicitante, Usuario? evaluador,
        UnidadFisica unidad)
    {
        FechaHora = fechaHora;
        Solicitante = solicitante;
        Evaluador = evaluador;
        Unidad = unidad;
    }
}
