namespace Dominio;

public class Prestamo
{
    private readonly List<UnidadFisica> _unidadesAsignadas = new List<UnidadFisica>();
    
    public Guid Id { get; } = Guid.NewGuid();
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; }
    public TipoEquipamiento TipoEquipamiento { get; }
    public int CantidadSolicitada { get; }
    public DateTime FechaHoraPrestamo { get; private set; }
    public Usuario? Evaluador { get; private set; }
    public IReadOnlyList<UnidadFisica> UnidadesAsignadas => _unidadesAsignadas.AsReadOnly();
    
    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada, DateTime fechaHoraPrestamo)
    {
        if (solicitante == null)
        {
            throw new DominioException("El prestamo debe tener un solicitante");
        }

        if (tipoEquipamiento == null)
        {
            throw new DominioException("El  prestamo debe tener un tipo equipamiento");
        }

        if (cantidadSolicitada <= 0)
        {
            throw new DominioException("La cantidad solicitada debe ser mayor a cero");
        }
        
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
        ValidarQueEsteActivo();
        ValidarQueNingunaUnidadSigaPrestada();
        
        Estado = EstadoPrestamo.Devuelto;
    }
    
    private void ValidarQueEsteSolicitado()
    {
        if (Estado != EstadoPrestamo.Solicitado)
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
        foreach (UnidadFisica unidad in unidadesFisicas)
        {
            if (!unidad.EstaDisponible())
            {
                throw new DominioException($"La unidad {unidad.IdInstitucional} no esta disponible");
            }
        }
    }
    
    private void AsignarUnidades(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (UnidadFisica unidadFisica in unidadesFisicas)
        {
            unidadFisica.Prestada();
        }
        
        _unidadesAsignadas.AddRange(unidadesFisicas);
    }
    
    private void ValidarQueEsteActivo()
    {
        if (Estado != EstadoPrestamo.Activo)
        {
            throw new DominioException("Solo se puede devolver un prestamo activo");
        }
    }
    
    private void ValidarQueNingunaUnidadSigaPrestada()
    {
        foreach (UnidadFisica unidadFisica in _unidadesAsignadas)
        {
            if (unidadFisica.Estado == EstadoUnidadFisica.Prestada)
            {
                throw new DominioException($"La unidad {unidadFisica.IdInstitucional} todavia esta prestada");
            }
        }
    }
}