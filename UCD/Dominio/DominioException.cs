namespace Dominio;

public class DominioException : Exception
{
    public DominioException() : base()
    {
    }

    public DominioException(string mensaje) : base(mensaje)
    {
    }

    public DominioException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }
}