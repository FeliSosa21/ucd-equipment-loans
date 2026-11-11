namespace Dominio;

public class Prestamo
{
    private const string MotivoFaltaDeUnidades = "No hay unidades suficientes para el tipo de equipamiento";
    public const int LargoMaximoMotivoRechazo = 300;
    private readonly List<UnidadFisica> _unidadesAsignadas = new List<UnidadFisica>();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public EstadoPrestamo Estado { get; private set; } = EstadoPrestamo.Solicitado;
    public Usuario Solicitante { get; private set; }
    public TipoEquipamiento TipoEquipamiento { get; private set; }
    public int CantidadSolicitada { get; private set; }
    public DateTime FechaHoraPrestamo { get; private set; }
    public DateTime FechaEsperadaDevolucion { get; private set; }
    public Usuario? Evaluador { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public IReadOnlyList<UnidadFisica> UnidadesAsignadas => _unidadesAsignadas.AsReadOnly();

    private Prestamo()
    {
        Solicitante = null!;
        TipoEquipamiento = null!;
    }

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
        FechaEsperadaDevolucion = CalendarioLaboral.SumarDiasHabiles(
            desde: fechaHoraPrestamo,
            diasHabiles: tipoEquipamiento.DuracionMaxPrestamo);
    }

    public void Activar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueEsteSolicitado();
        ValidarQueNoRequieraAprobacion();
        AsignarYActivar(unidadesFisicas);
    }

    public void Aprobar(Usuario evaluador, IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueEsteSolicitado();
        ValidarEvaluador(evaluador);
        AsignarYActivar(unidadesFisicas);

        Evaluador = evaluador;
    }

    public void Rechazar(Usuario evaluador, string motivo)
    {
        ValidarQueEsteSolicitado();
        ValidarEvaluador(evaluador);
        ValidarMotivo(motivo);

        Estado = EstadoPrestamo.Rechazado;
        Evaluador = evaluador;
        MotivoRechazo = motivo;
    }

    public void RechazarPorFaltaDeUnidades()
    {
        ValidarQueEsteSolicitado();

        Estado = EstadoPrestamo.Rechazado;
        MotivoRechazo = MotivoFaltaDeUnidades;
    }

    public void ActivarORechazarPorFaltaDeUnidades()
    {
        ValidarQueEsteSolicitado();
        ValidarQueNoRequieraAprobacion();

        List<UnidadFisica> unidades = PrimerasUnidadesDisponibles();

        if (unidades.Count < CantidadSolicitada)
        {
            RechazarPorFaltaDeUnidades();
        }
        else
        {
            Activar(unidades);
        }
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
               || Estado == EstadoPrestamo.Activo;
    }

    public bool EstaVencido(DateTime ahora)
    {
        return Estado == EstadoPrestamo.Activo && FechaEsperadaDevolucion < ahora.Date;
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

    private void ValidarQueNoRequieraAprobacion()
    {
        if (TipoEquipamiento.RequiereAprobacion)
        {
            throw new DominioException("El prestamo requiere aprobacion antes de activarse");
        }
    }

    private static void ValidarEvaluador(Usuario evaluador)
    {
        if (evaluador is null)
        {
            throw new DominioException("El prestamo debe tener un evaluador");
        }
    }

    private void ValidarUnidades(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarQueLaListaExista(unidadesFisicas);
        ValidarCantidad(unidadesFisicas);
        ValidarQueNoHayaUnidadesRepetidas(unidadesFisicas);
        ValidarQueSeanDelTipo(unidadesFisicas);
        ValidarQueEstenDisponibles(unidadesFisicas);
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
        foreach (UnidadFisica unidadFisica in unidadesFisicas)
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

    private List<UnidadFisica> PrimerasUnidadesDisponibles()
    {
        return TipoEquipamiento.UnidadesDisponibles().Take(CantidadSolicitada).ToList();
    }

    private void AsignarYActivar(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        ValidarUnidades(unidadesFisicas);
        AsignarUnidades(unidadesFisicas);

        Estado = EstadoPrestamo.Activo;
    }

    private void AsignarUnidades(IReadOnlyList<UnidadFisica> unidadesFisicas)
    {
        foreach (UnidadFisica unidadFisica in unidadesFisicas)
        {
            unidadFisica.Prestada();
        }

        _unidadesAsignadas.AddRange(unidadesFisicas);
    }

    private static void ValidarMotivo(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DominioException("El motivo de rechazo no puede ser vacio");
        }

        if (motivo.Length > LargoMaximoMotivoRechazo)
        {
            throw new DominioException($"El motivo de rechazo no puede superar los {LargoMaximoMotivoRechazo} caracteres");
        }
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