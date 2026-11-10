namespace Dominio;

public class Notificacion
{
    public int Id { get; private set; }
    public Prestamo Prestamo { get; private set; }
    public TipoNotificacion Tipo { get; private set; }
    public DateTime FechaHora { get; private set; }
    public bool EstaLeida { get; private set; }

    private Notificacion()
    {
        Prestamo = null!;
    }

    public Notificacion(Prestamo prestamo, TipoNotificacion tipo, DateTime fechaHora)
    {
        ValidarPrestamo(prestamo);

        Prestamo = prestamo;
        Tipo = tipo;
        FechaHora = fechaHora;
    }

    private static void ValidarPrestamo(Prestamo prestamo)
    {
        if (prestamo is null)
        {
            throw new DominioException("La notificacion debe tener un prestamo");
        }
    }
}
