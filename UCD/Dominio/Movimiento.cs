namespace Dominio;

// Registro historico del req. 10: un movimiento por unidad para prestamo, aprobacion y devolucion; solicitud y rechazo, sin unidad. La aprobacion lleva evaluador. No se modifica ni se elimina.
public class Movimiento
{
    public DateTime FechaHora { get; }
    public TipoMovimiento Tipo { get; }
    public Usuario Solicitante { get; }
    public Usuario? Evaluador { get; }
    public UnidadFisica? Unidad { get; }
    public Prestamo? Prestamo { get; private init; }
    public Usuario? Responsable { get; private init; }

    public static Movimiento DeSolicitud(Prestamo prestamo, RegistroDeAccion registro)
    {
        return new Movimiento(registro.FechaHora, TipoMovimiento.Solicitud, prestamo.Solicitante,
            prestamo.Evaluador, unidad: null)
        {
            Prestamo = prestamo,
            Responsable = registro.Responsable
        };
    }

    public Movimiento(DateTime fechaHora, TipoMovimiento tipo, Usuario solicitante, Usuario? evaluador,
        UnidadFisica? unidad)
    {
        ValidarTipo(tipo);
        ValidarSolicitante(solicitante);
        ValidarUnidad(tipo, unidad);
        ValidarEvaluador(tipo, evaluador);

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
        bool requiereUnidad = tipo == TipoMovimiento.Prestamo || tipo == TipoMovimiento.Devolucion
            || tipo == TipoMovimiento.Aprobacion;

        if (requiereUnidad && unidad is null)
        {
            throw new DominioException("El movimiento debe tener una unidad");
        }

        if (!requiereUnidad && unidad is not null)
        {
            throw new DominioException("La solicitud y el rechazo no llevan unidad");
        }
    }

    private static void ValidarEvaluador(TipoMovimiento tipo, Usuario? evaluador)
    {
        if (tipo == TipoMovimiento.Aprobacion && evaluador is null)
        {
            throw new DominioException("La aprobacion debe tener un evaluador");
        }
    }
}
