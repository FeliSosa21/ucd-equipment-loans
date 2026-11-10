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
        ValidarTipo(tipo);

        Prestamo = prestamo;
        Tipo = tipo;
        FechaHora = fechaHora;
    }

    public void MarcarComoLeida()
    {
        EstaLeida = true;
    }

    private static void ValidarPrestamo(Prestamo prestamo)
    {
        if (prestamo is null)
        {
            throw new DominioException("La notificacion debe tener un prestamo");
        }
    }

    private static void ValidarTipo(TipoNotificacion tipo)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new DominioException("La notificacion debe tener un tipo valido");
        }
    }
}
