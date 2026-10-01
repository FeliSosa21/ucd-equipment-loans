namespace Dominio;

public class Prestamo
{
    private readonly List<UnidadFisica> _unidadesAsignadas = new List<UnidadFisica>();
    
    public Guid Id { get; private set; } = Guid.NewGuid();
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; private set; }
    public TipoEquipamiento TipoEquipamiento { get; private set; }
    public int CantidadSolicitada { get; private set; }
    public DateTime FechaHoraPrestamo { get; private set; }
    public IReadOnlyList<UnidadFisica> UnidadesAsignadas => _unidadesAsignadas.AsReadOnly();

    public void Activar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        if (unidadesFisicas.Count != CantidadSolicitada)
        {
            throw new DominioException(
                $"Se esperaban {CantidadSolicitada} cantidad de unidades y se recibieron {unidadesFisicas.Count}");
        }
        foreach (var unidadFisica in unidadesFisicas)
        {
            unidadFisica.Prestada();
        }
        _unidadesAsignadas.AddRange(unidadesFisicas);
        Estado = EstadoPrestamo.Activo;
    }
    
    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada, DateTime fechaHoraPrestamo)
    {
        Solicitante = solicitante;
        TipoEquipamiento = tipoEquipamiento;
        CantidadSolicitada = cantidadSolicitada;
        FechaHoraPrestamo = fechaHoraPrestamo;
    }
}