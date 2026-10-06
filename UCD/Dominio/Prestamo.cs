namespace Dominio;

public class Prestamo
{
    private const string MotivoFaltaDeUnidades = "No hay unidades suficientes para el tipo de equipamiento";
    private readonly List<UnidadFisica> _unidadesAsignadas = new List<UnidadFisica>();

    public Guid Id { get; } = Guid.NewGuid();
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; }
    public TipoEquipamiento TipoEquipamiento { get; }
    public int CantidadSolicitada { get; }
    public DateTime FechaHoraPrestamo { get; }
    public Usuario? Evaluador { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public IReadOnlyList<UnidadFisica> UnidadesAsignadas => _unidadesAsignadas.AsReadOnly();

    public Prestamo(Usuario solicitante, TipoEquipamiento tipoEquipamiento, int cantidadSolicitada,
        DateTime fechaHoraPrestamo)
    {
        ValidarSolicitante(solicitante);
        ValidarTipoEquipamiento(tipoEquipamiento);
        ValidarCantidadSolicitada(cantidadSolicitada);

        Solicitante = solicitante;
        TipoEquipamiento = tipoEquipamiento;
        CantidadSolicitada = cantidadSolicitada;
        FechaHoraPrestamo = fechaHoraPrestamo;
    }

    public void Activar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueEsteSolicitado();
        ValidarQueLaListaExista(unidadesFisicas);
        ValidarCantidad(unidadesFisicas);
        ValidarQueNoHayaUnidadesRepetidas(unidadesFisicas);
        ValidarQueEstenDisponibles(unidadesFisicas);
        AsignarUnidades(unidadesFisicas);

        Estado = EstadoPrestamo.Activo;
    }

    public void Aprobar(Usuario evaluador, IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueEsteSolicitado();
        ValidarEvaluador(evaluador);
        ValidarQueLaListaExista(unidadesFisicas);
        ValidarCantidad(unidadesFisicas);
        ValidarQueNoHayaUnidadesRepetidas(unidadesFisicas);
        ValidarQueSeanDelTipo(unidadesFisicas);
        ValidarQueEstenDisponibles(unidadesFisicas);
        AsignarUnidades(unidadesFisicas);

        Estado = EstadoPrestamo.Aprobado;
        Evaluador = evaluador;
    }

    public void Realizar()
    {
    }

    public void RechazarPorFaltaDeUnidades()
    {
        ValidarQueEsteSolicitado();

        Estado = EstadoPrestamo.Rechazado;
        MotivoRechazo = MotivoFaltaDeUnidades;
    }

    public void MarcarDevuelto()
    {
        ValidarQueEsteActivo();
        ValidarQueNingunaUnidadSigaPrestada();

        Estado = EstadoPrestamo.Devuelto;
    }

    public bool EstaVigente()
    {
        return Estado == EstadoPrestamo.Solicitado
               || Estado == EstadoPrestamo.Activo
               || Estado == EstadoPrestamo.Aprobado;
    }

    private static void ValidarSolicitante(Usuario solicitante)
    {
        if (solicitante is null)
        {
            throw new DominioException("El prestamo debe tener un solicitante");
        }
    }

    private static void ValidarTipoEquipamiento(TipoEquipamiento tipoEquipamiento)
    {
        if (tipoEquipamiento is null)
        {
            throw new DominioException("El prestamo debe tener un tipo de equipamiento");
        }
    }

    private static void ValidarCantidadSolicitada(int cantidadSolicitada)
    {
        if (cantidadSolicitada <= 0)
        {
            throw new DominioException("La cantidad solicitada debe ser mayor a cero");
        }
    }

    private void ValidarQueEsteSolicitado()
    {
        if (Estado != EstadoPrestamo.Solicitado)
        {
            throw new DominioException("El prestamo debe estar solicitado");
        }
    }

    private static void ValidarEvaluador(Usuario evaluador)
    {
        if (evaluador is null)
        {
            throw new DominioException("El prestamo debe tener un evaluador");
        }
    }

    private static void ValidarQueLaListaExista(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        if (unidadesFisicas is null)
        {
            throw new DominioException("El prestamo debe tener una lista de unidades");
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

    private static void ValidarQueNoHayaUnidadesRepetidas(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        if (unidadesFisicas.Distinct().Count() != unidadesFisicas.Count)
        {
            throw new DominioException("Una unidad no puede prestarse dos veces");
        }
    }

    private void ValidarQueSeanDelTipo(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (var unidadFisica in unidadesFisicas)
        {
            if (!TipoEquipamiento.Unidades.Contains(unidadFisica))
            {
                throw new DominioException(
                    $"La unidad {unidadFisica.IdInstitucional} no pertenece al tipo de equipamiento del prestamo");
            }
        }
    }

    private static void ValidarQueEstenDisponibles(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (UnidadFisica unidadFisica in unidadesFisicas)
        {
            if (!unidadFisica.EstaDisponible())
            {
                throw new DominioException($"La unidad {unidadFisica.IdInstitucional} no esta disponible");
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