namespace Dominio;

public class Prestamo
{
    private readonly List<UnidadFisica> _unidadesFisicasAsignadas = new List<UnidadFisica>();
    
    public Guid Id { get; private set; } = Guid.NewGuid();
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; private set; }
    public TipoEquipamiento TipoEquipamiento { get; private set; }
    public int CantidadSolicitada { get; private set; }
    public DateTime FechaHoraPrestamo { get; private set; }
    public IReadOnlyList<UnidadFisica> UnidadesFisicasAsignadas => _unidadesFisicasAsignadas.AsReadOnly();

    public void Activar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        return;
    }
    
    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada, DateTime fechaHoraPrestamo)
    {
        Solicitante = solicitante;
        TipoEquipamiento = tipoEquipamiento;
        CantidadSolicitada = cantidadSolicitada;
        FechaHoraPrestamo = fechaHoraPrestamo;
    }
}