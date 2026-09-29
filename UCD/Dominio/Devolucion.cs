namespace Dominio;

public class Devolucion
{
    private const int LargoMaxObservaciones = 300;

    public DateTime FechaHoraDevolucion { get; private set; }
    public CondicionDevolucion Condicion { get; private set; }
    public string? Observaciones { get; private set; }
    public Usuario RegistradaPor { get; private set; }

    public Devolucion(DateTime fechaHoraDevolucion, DateTime inicioPrestamo, CondicionDevolucion condicion,
        string? observaciones, Usuario registradaPor)
    {
        ValidarFecha(fechaHoraDevolucion, inicioPrestamo);
        ValidarObservaciones(observaciones);
        ValidarUsuario(registradaPor);

        FechaHoraDevolucion = fechaHoraDevolucion;
        Condicion = condicion;
        Observaciones = observaciones;
        RegistradaPor = registradaPor;
    }

    public void ActualizarUnidad(UnidadFisica unidad)
    {
        if (Condicion == CondicionDevolucion.SinObservaciones)
        {
            unidad.Disponible();
            return;
        }

        unidad.AMantenimiento();
    }

    private static void ValidarFecha(DateTime fechaHoraDevolucion, DateTime inicioPrestamo)
    {
        if (fechaHoraDevolucion < inicioPrestamo)
        {
            throw new DominioException("La fecha de devolucion no puede ser anterior al inicio del prestamo");
        }
    }

    private static void ValidarObservaciones(string? observaciones)
    {
        if (observaciones != null && observaciones.Length > LargoMaxObservaciones)
        {
            throw new DominioException("Las observaciones no pueden tener mas de 300 caracteres");
        }
    }

    private static void ValidarUsuario(Usuario registradaPor)
    {
        if (registradaPor is null)
        {
            throw new DominioException("La devolucion debe tener el usuario que la registra");
        }
    }
}
