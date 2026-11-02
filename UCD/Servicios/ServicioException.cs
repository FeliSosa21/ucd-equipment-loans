namespace Servicios;

public class ServicioException : Exception
{
    public string? Campo { get; }

    public ServicioException() : base()
    {
    }

    public ServicioException(string mensaje) : base(mensaje)
    {
    }

    public ServicioException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }

    public ServicioException(string campo, string mensaje) : base(mensaje)
    {
    }
}
