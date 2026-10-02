namespace Dominio;

// Devolucion total de un prestamo activo: todas sus unidades en un mismo acto, cada una con su condicion.
public class Devolucion
{
    private readonly List<UnidadDevuelta> _unidades;

    public Prestamo Prestamo { get; }
    public Usuario RegistradaPor { get; }
    public DateTime FechaHoraDevolucion { get; }
    public IReadOnlyList<UnidadDevuelta> Unidades => _unidades.AsReadOnly();

    public Devolucion(Prestamo prestamo, Usuario registradaPor, DateTime fechaHoraDevolucion,
        IReadOnlyList<UnidadDevuelta> unidades)
    {
        ValidarPrestamo(prestamo);
        ValidarUsuario(registradaPor);
        ValidarFecha(fechaHoraDevolucion, prestamo.FechaHoraPrestamo);
        ValidarUnidades(unidades, prestamo);

        Prestamo = prestamo;
        RegistradaPor = registradaPor;
        FechaHoraDevolucion = fechaHoraDevolucion;
        _unidades = unidades.ToList();
    }

    public void Aplicar()
    {
        ValidarPrestamo(Prestamo);

        foreach (UnidadDevuelta unidadDevuelta in _unidades)
        {
            unidadDevuelta.ActualizarEstadoDeLaUnidad();
        }

        Prestamo.MarcarDevuelto();
    }

    private static void ValidarPrestamo(Prestamo prestamo)
    {
        if (prestamo is null)
        {
            throw new DominioException("La devolucion debe tener el prestamo devuelto");
        }

        if (prestamo.Estado != EstadoPrestamo.Activo)
        {
            throw new DominioException("Solo se puede devolver un prestamo activo");
        }
    }

    private static void ValidarUsuario(Usuario registradaPor)
    {
        if (registradaPor is null)
        {
            throw new DominioException("La devolucion debe tener el usuario que la registra");
        }
    }

    private static void ValidarFecha(DateTime fechaHoraDevolucion, DateTime inicioPrestamo)
    {
        if (fechaHoraDevolucion < inicioPrestamo)
        {
            throw new DominioException("La fecha de devolucion no puede ser anterior al inicio del prestamo");
        }
    }

    private static void ValidarUnidades(IReadOnlyList<UnidadDevuelta> unidades, Prestamo prestamo)
    {
        if (unidades is null)
        {
            throw new DominioException("La devolucion debe incluir las unidades del prestamo");
        }

        List<UnidadFisica> devueltas = unidades.Select(unidadDevuelta => unidadDevuelta.Unidad).ToList();

        if (devueltas.Distinct().Count() != devueltas.Count)
        {
            throw new DominioException("Una unidad no puede devolverse dos veces");
        }

        bool sonLasDelPrestamo = devueltas.Count == prestamo.UnidadesAsignadas.Count &&
                                 prestamo.UnidadesAsignadas.All(devueltas.Contains);

        if (!sonLasDelPrestamo)
        {
            throw new DominioException("La devolucion debe incluir todas las unidades del prestamo y solo esas");
        }
    }
}
