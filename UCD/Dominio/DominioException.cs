namespace Dominio;

public class DominioException : Exception
{
    public string? Campo { get; }

    public DominioException() : base()
    {
    }

    public DominioException(string mensaje) : base(mensaje)
    {
    }

    public DominioException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }

    public DominioException(string campo, string mensaje) : base(mensaje)
    {
    }
}