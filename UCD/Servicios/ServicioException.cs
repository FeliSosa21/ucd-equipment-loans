namespace Servicios;

public class ServicioException : Exception
{
    public ServicioException() : base()
    {
    }

    public ServicioException(string mensaje) : base(mensaje)
    {
    }

    public ServicioException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }
}
