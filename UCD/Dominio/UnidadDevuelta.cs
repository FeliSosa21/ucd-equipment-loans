namespace Dominio;

// Una unidad de un prestamo al momento de devolverla: su condicion y, si hace falta, observaciones.
public class UnidadDevuelta
{
    private const int LargoMaximoObservaciones = 300;

    public UnidadFisica Unidad { get; }
    public CondicionDevolucion Condicion { get; }
    public string? Observaciones { get; }

    public UnidadDevuelta(UnidadFisica unidad, CondicionDevolucion condicion, string? observaciones)
    {
        ValidarUnidad(unidad);
        ValidarObservaciones(observaciones);

        Unidad = unidad;
        Condicion = condicion;
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
    }

    public void ActualizarEstadoDeLaUnidad()
    {
        if (Condicion == CondicionDevolucion.SinObservaciones)
        {
            Unidad.Disponible();
            return;
        }

        Unidad.AMantenimiento();
    }

    private static void ValidarUnidad(UnidadFisica unidad)
    {
        if (unidad is null)
        {
            throw new DominioException("La unidad devuelta no puede ser nula");
        }
    }

    private static void ValidarObservaciones(string? observaciones)
    {
        if (observaciones != null && observaciones.Length > LargoMaximoObservaciones)
        {
            throw new DominioException(
                $"Las observaciones no pueden tener mas de {LargoMaximoObservaciones} caracteres");
        }
    }
}
