using Dominio;

namespace Data;

public class Memoria
{
    internal List<Usuario> Usuarios { get; } = new List<Usuario>();
    internal List<TipoUsuario> TiposDeUsuario { get; } = new List<TipoUsuario>();
    internal Catalogo? Catalogo { get; set; }
    internal List<Prestamo> Prestamos { get; } = new List<Prestamo>();
    internal List<Devolucion> Devoluciones { get; } = new List<Devolucion>();
    internal List<Movimiento> Movimientos { get; } = new List<Movimiento>();
}