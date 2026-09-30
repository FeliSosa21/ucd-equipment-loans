namespace Dominio;

public class Prestamo
{
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; private set; }
    public TipoEquipamiento TipoEquipamiento { get; private set; }
    public int CantidadSolicitada { get; private set; }
    public DateTime FechaHoraPrestamo { get; private set; }
    
    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada, DateTime fechaHoraPrestamo)
    {
        Solicitante = solicitante;
        TipoEquipamiento = tipoEquipamiento;
        CantidadSolicitada = cantidadSolicitada;
        FechaHoraPrestamo = fechaHoraPrestamo;
    }
}