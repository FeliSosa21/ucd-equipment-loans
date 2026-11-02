namespace Dominio;

public class Movimiento
{
    private RegistroDeAccion _registro;

    public int Id { get; private set; }
    public Prestamo Prestamo { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public Usuario? Evaluador { get; private set; }
    public UnidadFisica? Unidad { get; private init; }
    public DateTime FechaHora => _registro.FechaHora;
    public Usuario? Responsable => _registro.Responsable;
    public Usuario Solicitante => Prestamo.Solicitante;

    private Movimiento()
    {
        _registro = null!;
        Prestamo = null!;
    }

    private Movimiento(Prestamo prestamo, TipoMovimiento tipo, RegistroDeAccion registro)
    {
        ValidarPrestamoYRegistro(prestamo, registro);
        ValidarEvaluador(tipo, prestamo.Evaluador);

        Prestamo = prestamo;
        Tipo = tipo;
        Evaluador = prestamo.Evaluador;
        _registro = registro;
    }

    public static Movimiento DeSolicitud(Prestamo prestamo, RegistroDeAccion registro)
    {
        return new Movimiento(prestamo, TipoMovimiento.Solicitud, registro);
    }

    public static Movimiento DeRechazo(Prestamo prestamo, RegistroDeAccion registro)
    {
        return new Movimiento(prestamo, TipoMovimiento.Rechazo, registro);
    }

    public static Movimiento DeAprobacion(Prestamo prestamo, RegistroDeAccion registro, UnidadFisica unidad)
    {
        return new Movimiento(prestamo, TipoMovimiento.Aprobacion, registro) { Unidad = ValidarUnidad(unidad) };
    }

    public static Movimiento DePrestamo(Prestamo prestamo, RegistroDeAccion registro, UnidadFisica unidad)
    {
        return new Movimiento(prestamo, TipoMovimiento.Prestamo, registro) { Unidad = ValidarUnidad(unidad) };
    }

    public static Movimiento DeDevolucion(Prestamo prestamo, RegistroDeAccion registro, UnidadFisica unidad)
    {
        return new Movimiento(prestamo, TipoMovimiento.Devolucion, registro) { Unidad = ValidarUnidad(unidad) };
    }

    private static void ValidarPrestamoYRegistro(Prestamo prestamo, RegistroDeAccion registro)
    {
        if (prestamo is null)
        {
            throw new DominioException("El movimiento debe tener un prestamo");
        }

        if (registro is null)
        {
            throw new DominioException("El movimiento debe tener quien y cuando");
        }
    }

    private static void ValidarEvaluador(TipoMovimiento tipo, Usuario? evaluador)
    {
        if (tipo == TipoMovimiento.Aprobacion && evaluador is null)
        {
            throw new DominioException("La aprobacion debe tener un evaluador");
        }
    }

    private static UnidadFisica ValidarUnidad(UnidadFisica unidad)
    {
        if (unidad is null)
        {
            throw new DominioException("El movimiento debe tener una unidad");
        }

        return unidad;
    }
}
