namespace Dominio;

// Devolucion total de un prestamo activo: todas sus unidades en un mismo acto, cada una con su condicion.
public class Devolucion
{
    private readonly List<UnidadDevuelta> _unidades;
    private readonly RegistroDeAccion _registro;

    public Prestamo Prestamo { get; }
    public Usuario RegistradaPor => _registro.Responsable!;
    public DateTime FechaHoraDevolucion => _registro.FechaHora;
    public IReadOnlyList<UnidadDevuelta> Unidades => _unidades.AsReadOnly();

    public Devolucion(Prestamo prestamo, RegistroDeAccion registro, IReadOnlyList<UnidadDevuelta> unidades)
    {
        ValidarPrestamo(prestamo);
        ValidarUsuario(registro?.Responsable);
        ValidarFecha(registro!.FechaHora, prestamo.FechaHoraPrestamo);
        ValidarUnidades(unidades, prestamo);

        Prestamo = prestamo;
        _registro = registro;
        _unidades = unidades.ToList();
    }

    public void Aplicar()
    {
        ValidarPrestamo(Prestamo);
        ValidarQueTodasEstenPrestadas();

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

    private static void ValidarUsuario(Usuario? registradaPor)
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

        if (unidades.Any(unidadDevuelta => unidadDevuelta is null))
        {
            throw new DominioException("La devolucion no puede incluir unidades nulas");
        }

        List<UnidadFisica> devueltas = unidades.Select(unidadDevuelta => unidadDevuelta.Unidad).ToList();
        ValidarQueNoHayaRepetidas(devueltas);
        ValidarQueSeanLasDelPrestamo(devueltas, prestamo);
    }

    // Se valida antes de cambiar cualquier unidad, para no dejar la devolucion aplicada a medias.
    private void ValidarQueTodasEstenPrestadas()
    {
        if (_unidades.Any(unidadDevuelta => unidadDevuelta.Unidad.Estado != EstadoUnidadFisica.Prestada))
        {
            throw new DominioException("Todas las unidades deben estar prestadas para registrar la devolucion");
        }
    }

    private static void ValidarQueNoHayaRepetidas(List<UnidadFisica> devueltas)
    {
        if (devueltas.Distinct().Count() != devueltas.Count)
        {
            throw new DominioException("Una unidad no puede devolverse dos veces");
        }
    }

    private static void ValidarQueSeanLasDelPrestamo(List<UnidadFisica> devueltas, Prestamo prestamo)
    {
        bool sonLasDelPrestamo = devueltas.Count == prestamo.UnidadesAsignadas.Count &&
                                 prestamo.UnidadesAsignadas.All(devueltas.Contains);

        if (!sonLasDelPrestamo)
        {
            throw new DominioException("La devolucion debe incluir todas las unidades del prestamo y solo esas");
        }
    }
}
