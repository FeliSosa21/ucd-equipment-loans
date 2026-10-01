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
    
    
    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada, DateTime fechaHoraPrestamo)
    {
        Solicitante = solicitante;
        TipoEquipamiento = tipoEquipamiento;
        CantidadSolicitada = cantidadSolicitada;
        FechaHoraPrestamo = fechaHoraPrestamo;
    }

    public void Activar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueEsteSolicitado();
        ValidarCantidad(unidadesFisicas);
        ValidarQueEstenDisponibles(unidadesFisicas);
        AsignarUnidades(unidadesFisicas);
        Estado = EstadoPrestamo.Activo;
    }

    public void MarcarDevuelto()
    {
        Estado = EstadoPrestamo.Devuelto;
    }

    private void ValidarQueEsteSolicitado()
    {
        if (Estado == EstadoPrestamo.Activo)
        {
            throw new DominioException("Solo se puede activar un prestamo con estado: solicitado");
        }
    }

    private void ValidarCantidad(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        if (unidadesFisicas.Count != CantidadSolicitada)
        {
            throw new DominioException(
                $"Se esperaban {CantidadSolicitada} unidades y se recibieron {unidadesFisicas.Count}");
        }
    }
    private static void ValidarQueEstenDisponibles(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (var unidad in unidadesFisicas)
        {
            if (!unidad.EstaDisponible())
            {
                throw new DominioException($"la unidad {unidad.IdInstitucional} no esta disponible");
            }
        }
    }
    private void AsignarUnidades(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (var unidadFisica in unidadesFisicas)
        {
            unidadFisica.Prestada();
        }

        _unidadesAsignadas.AddRange(unidadesFisicas);
    }
}