using Dominio;

namespace Data;

public class Memoria
{
    public List<Usuario> Usuarios { get; private set; } = new List<Usuario>();
    internal Catalogo? Catalogo { get; set; }
    internal List<Prestamo> Prestamos { get; } = new List<Prestamo>();
    internal List<Devolucion> Devoluciones { get; } = new List<Devolucion>();
}