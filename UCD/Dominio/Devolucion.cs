namespace Dominio;

public class Devolucion
{
    public DateTime FechaHoraDevolucion { get; private set; }

    public Devolucion(DateTime fechaHoraDevolucion)
    {
        FechaHoraDevolucion = fechaHoraDevolucion;
    }
}