namespace Dominio.Tests;

[TestClass]
public sealed class PrestamoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);
    private const int CantidadSolicitadaPorDefecto = 2;
    private const string MotivoDeRechazo = "El equipo esta reservado para un examen";

    [TestMethod]
    public void Constructor_DatosValidos_NaceSolicitado()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }

    [TestMethod]
    public void Constructor_DatosValidos_ConservaSolicitanteTipoCantidadYFecha()
    {
        Usuario solicitante = CrearSolicitante();
        TipoEquipamiento tipoEquipamiento = CrearTipoEquipamiento();
        const int otraCantidadSolicitada = 1;

        Prestamo prestamo = CrearPrestamo(
            solicitante: solicitante,
            tipoEquipamiento: tipoEquipamiento,
            cantidadSolicitada: otraCantidadSolicitada);

        Assert.AreSame(solicitante, prestamo.Solicitante);
        Assert.AreSame(tipoEquipamiento, prestamo.TipoEquipamiento);
        Assert.AreEqual(otraCantidadSolicitada, prestamo.CantidadSolicitada);
        Assert.AreEqual(FechaFija, prestamo.FechaHoraPrestamo);
    }

    [TestMethod]
    public void Constructor_DosPrestamos_TienenIdDistinto()
    {
        Prestamo primerPrestamo = CrearPrestamo();
        Prestamo segundoPrestamo = CrearPrestamo();

        Assert.AreNotEqual(primerPrestamo.Id, segundoPrestamo.Id);
    }

    [TestMethod]
    public void Constructor_DatosValidos_NaceSinUnidadesAsignadas()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.IsNotNull(prestamo.UnidadesAsignadas);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Constructor_SolicitanteNulo_LanzaExcepcion()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento();

        Assert.ThrowsException<DominioException>(() => new Prestamo(
            solicitante: null!,
            tipoEquipamiento: tipo,
            cantidadSolicitada: CantidadSolicitadaPorDefecto,
            fechaHoraPrestamo: FechaFija));
    }

    [TestMethod]
    public void Constructor_TipoEquipamientoNulo_LanzaExcepcion()
    {
        Usuario solicitante = CrearSolicitante();

        Assert.ThrowsException<DominioException>(() => new Prestamo(
            solicitante: solicitante,
            tipoEquipamiento: null!,
            cantidadSolicitada: CantidadSolicitadaPorDefecto,
            fechaHoraPrestamo: FechaFija));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_CantidadNoPositiva_LanzaExcepcion(int cantidadInvalida)
    {
        Assert.ThrowsException<DominioException>(
            () => CrearPrestamo(cantidadSolicitada: cantidadInvalida));
    }

    [TestMethod]
    public void Constructor_DatosValidos_NaceSinEvaluador()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.IsNull(prestamo.Evaluador);
    }

    [TestMethod]
    public void Constructor_DatosValidos_NaceSinMotivoDeRechazo()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.IsNull(prestamo.MotivoRechazo);
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_QuedaActivo()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_AsignaEsasUnidades()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        CollectionAssert.AreEquivalent(unidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void Activar_CantidadSolicitadaDeUnidadesDisponibles_UnidadesQuedanPrestadas()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        foreach (UnidadFisica unidad in unidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Prestada, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia estar prestada");
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(3)]
    public void Activar_CantidadDistintaALaSolicitada_LanzaExcepcion(int cantidadEntregada)
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: cantidadEntregada);

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(unidades));
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }

    [TestMethod]
    public void Activar_AlgunaUnidadNoDisponible_LanzaExcepcionYNoPrestaNinguna()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);
        UnidadFisica disponible = unidades[0];
        UnidadFisica prestada = unidades[1];

        prestada.Prestada();

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(unidades));
        Assert.AreEqual(EstadoUnidadFisica.Disponible, disponible.Estado);
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Activar_PrestamoYaActivo_LanzaExcepcion()
    {
        (Prestamo prestamo, List<UnidadFisica> primerasUnidades) = CrearPrestamoActivo();

        List<UnidadFisica> nuevasUnidades = CrearUnidadesDelTipo(
            tipo: prestamo.TipoEquipamiento,
            cantidad: CantidadSolicitadaPorDefecto,
            primerNumero: CantidadSolicitadaPorDefecto + 1);

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(nuevasUnidades));
        foreach (UnidadFisica unidad in nuevasUnidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        CollectionAssert.AreEquivalent(primerasUnidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void Activar_PrestamoDevuelto_LanzaExcepcion()
    {
        (Prestamo prestamo, List<UnidadFisica> primerasUnidades) = CrearPrestamoDevuelto();

        List<UnidadFisica> nuevasUnidades = CrearUnidadesDelTipo(
            tipo: prestamo.TipoEquipamiento,
            cantidad: CantidadSolicitadaPorDefecto,
            primerNumero: CantidadSolicitadaPorDefecto + 1);

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(nuevasUnidades));
        foreach (UnidadFisica unidad in nuevasUnidades)
        {
            Assert.AreEqual(
                EstadoUnidadFisica.Disponible,
                unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }
        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
        CollectionAssert.AreEquivalent(primerasUnidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void Activar_ListaNula_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(null!));
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Activar_UnidadRepetida_LanzaExcepcionYNoPrestaNinguna()
    {
        Prestamo prestamo = CrearPrestamo();
        UnidadFisica unidad = CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: 1)[0];
        List<UnidadFisica> mismaUnidadDosVeces = new List<UnidadFisica> { unidad, unidad };

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(mismaUnidadDosVeces));
        Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado);
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Activar_PrestamoRechazado_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoRechazado();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);

        Assert.ThrowsException<DominioException>(() => prestamo.Activar(unidades));
        foreach (UnidadFisica unidad in unidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }
        Assert.AreEqual(EstadoPrestamo.Rechazado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Aprobar_SolicitudDeTipoConAprobacion_QuedaAprobado()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();

        prestamo.Aprobar(evaluador, unidades);

        Assert.AreEqual(EstadoPrestamo.Aprobado, prestamo.Estado);
    }

    [TestMethod]
    public void Aprobar_SolicitudDeTipoConAprobacion_GuardaElEvaluador()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();

        prestamo.Aprobar(evaluador, unidades);

        Assert.AreSame(evaluador, prestamo.Evaluador);
    }

    [TestMethod]
    public void Aprobar_SolicitudDeTipoConAprobacion_AsignaYPrestaLasUnidades()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();

        prestamo.Aprobar(evaluador, unidades);

        CollectionAssert.AreEquivalent(unidades, prestamo.UnidadesAsignadas.ToList());

        foreach (UnidadFisica unidad in unidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Prestada, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia estar prestada");
        }
    }

    [TestMethod]
    public void Aprobar_PrestamoActivo_LanzaExcepcion()
    {
        (Prestamo prestamo, List<UnidadFisica> primerasUnidades) = CrearPrestamoActivo();

        List<UnidadFisica> nuevasUnidades = CrearUnidadesDelTipo(
            tipo: prestamo.TipoEquipamiento,
            cantidad: CantidadSolicitadaPorDefecto,
            primerNumero: CantidadSolicitadaPorDefecto + 1);
        Usuario evaluador = CrearEvaluador();

        Assert.ThrowsException<DominioException>(
            () => prestamo.Aprobar(evaluador, nuevasUnidades));

        foreach (UnidadFisica unidad in nuevasUnidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }

        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        Assert.IsNull(prestamo.Evaluador);
        CollectionAssert.AreEquivalent(primerasUnidades, prestamo.UnidadesAsignadas.ToList());
    }

    [TestMethod]
    public void Aprobar_AlgunaUnidadNoDisponible_LanzaExcepcionYNoPrestaNinguna()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();
        UnidadFisica disponible = unidades[0];
        UnidadFisica prestada = unidades[1];

        prestada.Prestada();

        Assert.ThrowsException<DominioException>(() => prestamo.Aprobar(evaluador, unidades));

        Assert.AreEqual(EstadoUnidadFisica.Disponible, disponible.Estado);
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
        Assert.IsNull(prestamo.Evaluador);
    }

    [TestMethod]
    public void Aprobar_EvaluadorNulo_LanzaExcepcion()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();

        Assert.ThrowsException<DominioException>(() => prestamo.Aprobar(null!, unidades));

        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);

        foreach (UnidadFisica unidad in unidades)
        {
            Assert.AreEqual(EstadoUnidadFisica.Disponible, unidad.Estado,
                $"La unidad {unidad.IdInstitucional} deberia seguir disponible");
        }
    }

    [TestMethod]
    public void Aprobar_UnidadDeOtroTipo_LanzaExcepcionYNoPrestaNinguna()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();
        UnidadFisica propia = unidades[0];
        UnidadFisica ajena = CrearUnidadDeOtroTipo();
        List<UnidadFisica> unaPropiaYUnaAjena = new List<UnidadFisica> { propia, ajena };

        Assert.ThrowsException<DominioException>(() => prestamo.Aprobar(evaluador, unaPropiaYUnaAjena));

        Assert.AreEqual(EstadoUnidadFisica.Disponible, propia.Estado);
        Assert.AreEqual(EstadoUnidadFisica.Disponible, ajena.Estado);
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.AreEqual(0, prestamo.UnidadesAsignadas.Count);
    }

    [TestMethod]
    public void Realizar_PrestamoAprobado_QuedaActivo()
    {
        (Prestamo prestamo, _) = CrearPrestamoAprobado();

        prestamo.Realizar();

        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void Realizar_PrestamoSolicitado_LanzaExcepcion()
    {
        (Prestamo prestamo, _) = CrearSolicitudDeTipoConAprobacion();

        Assert.ThrowsException<DominioException>(() => prestamo.Realizar());

        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoActivoSinUnidadesPrestadas_QuedaDevuelto()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();

        unidades[0].Disponible();
        unidades[1].AMantenimiento();

        prestamo.MarcarDevuelto();

        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoSolicitado_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamo();

        Assert.ThrowsException<DominioException>(() => prestamo.MarcarDevuelto());
        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoYaDevuelto_LanzaExcepcion()
    {
        (Prestamo prestamo, _) = CrearPrestamoDevuelto();

        Assert.ThrowsException<DominioException>(() => prestamo.MarcarDevuelto());
        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_AlgunaUnidadTodaviaPrestada_LanzaExcepcion()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();

        UnidadFisica devuelta = unidades[0];
        devuelta.Disponible();

        Assert.ThrowsException<DominioException>(() => prestamo.MarcarDevuelto());
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        Assert.AreEqual(EstadoUnidadFisica.Disponible, devuelta.Estado);
    }

    [TestMethod]
    public void MarcarDevuelto_PrestamoActivoSinUnidadesPrestadas_NoCambiaLasUnidades()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();

        UnidadFisica disponible = unidades[0];
        UnidadFisica enMantenimiento = unidades[1];

        disponible.Disponible();
        enMantenimiento.AMantenimiento();

        prestamo.MarcarDevuelto();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, disponible.Estado);
        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, enMantenimiento.Estado);
    }

    [TestMethod]
    public void Rechazar_SolicitudPendiente_QuedaRechazadoConEvaluadorYMotivo()
    {
        (Prestamo prestamo, _) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();

        prestamo.Rechazar(evaluador, MotivoDeRechazo);

        Assert.AreEqual(EstadoPrestamo.Rechazado, prestamo.Estado);
        Assert.AreSame(evaluador, prestamo.Evaluador);
        Assert.AreEqual(MotivoDeRechazo, prestamo.MotivoRechazo);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void Rechazar_MotivoVacio_LanzaExcepcion(string? motivoInvalido)
    {
        (Prestamo prestamo, _) = CrearSolicitudDeTipoConAprobacion();
        Usuario evaluador = CrearEvaluador();

        Assert.ThrowsException<DominioException>(
            () => prestamo.Rechazar(evaluador, motivoInvalido!));

        Assert.AreEqual(EstadoPrestamo.Solicitado, prestamo.Estado);
        Assert.IsNull(prestamo.MotivoRechazo);
        Assert.IsNull(prestamo.Evaluador);
    }

    [TestMethod]
    public void RechazarPorFaltaDeUnidades_PrestamoSolicitado_QuedaRechazado()
    {
        Prestamo prestamo = CrearPrestamo();

        prestamo.RechazarPorFaltaDeUnidades();

        Assert.AreEqual(EstadoPrestamo.Rechazado, prestamo.Estado);
    }

    [TestMethod]
    public void RechazarPorFaltaDeUnidades_PrestamoSolicitado_GuardaElMotivo()
    {
        Prestamo prestamo = CrearPrestamo();

        prestamo.RechazarPorFaltaDeUnidades();

        Assert.IsFalse(string.IsNullOrWhiteSpace(prestamo.MotivoRechazo),
            "El prestamo rechazado deberia tener un motivo");
    }

    [TestMethod]
    public void RechazarPorFaltaDeUnidades_PrestamoSolicitado_QuedaSinEvaluador()
    {
        Prestamo prestamo = CrearPrestamo();

        prestamo.RechazarPorFaltaDeUnidades();

        Assert.IsNull(prestamo.Evaluador);
    }

    [TestMethod]
    public void RechazarPorFaltaDeUnidades_PrestamoActivo_LanzaExcepcion()
    {
        (Prestamo prestamo, _) = CrearPrestamoActivo();

        Assert.ThrowsException<DominioException>(() => prestamo.RechazarPorFaltaDeUnidades());
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
        Assert.IsNull(prestamo.MotivoRechazo);
    }

    [TestMethod]
    public void EstaVigente_PrestamoSolicitado_DevuelveVerdadero()
    {
        Prestamo prestamo = CrearPrestamo();

        bool vigente = prestamo.EstaVigente();

        Assert.IsTrue(vigente);
    }

    [TestMethod]
    public void EstaVigente_PrestamoAprobado_DevuelveVerdadero()
    {
        (Prestamo prestamo, _) = CrearPrestamoAprobado();

        bool vigente = prestamo.EstaVigente();

        Assert.IsTrue(vigente);
    }

    [TestMethod]
    public void EstaVigente_PrestamoActivo_DevuelveVerdadero()
    {
        (Prestamo prestamo, _) = CrearPrestamoActivo();

        bool vigente = prestamo.EstaVigente();

        Assert.IsTrue(vigente);
    }

    [TestMethod]
    public void EstaVigente_PrestamoRechazado_DevuelveFalso()
    {
        Prestamo prestamo = CrearPrestamoRechazado();

        bool vigente = prestamo.EstaVigente();

        Assert.IsFalse(vigente);
    }

    [TestMethod]
    public void EstaVigente_PrestamoDevuelto_DevuelveFalso()
    {
        (Prestamo prestamo, _) = CrearPrestamoDevuelto();

        bool vigente = prestamo.EstaVigente();

        Assert.IsFalse(vigente);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: "12345672",
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(
            dni: "67890127",
            nombre: "Sofia",
            apellido: "Martinez",
            correo: "sofia.martinez@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Supervisor);

        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento(bool requiereAprobacion = false)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(
            nombre: "Proyector Epson PowerLite",
            descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5,
            requiereAprobacion: requiereAprobacion,
            categoria: Categoria.Audiovisual);

        return tipo;
    }

    private static Prestamo CrearPrestamo(
        Usuario? solicitante = null,
        TipoEquipamiento? tipoEquipamiento = null,
        int cantidadSolicitada = CantidadSolicitadaPorDefecto)
    {
        Prestamo prestamo = new Prestamo(
            solicitante: solicitante ?? CrearSolicitante(),
            tipoEquipamiento: tipoEquipamiento ?? CrearTipoEquipamiento(),
            cantidadSolicitada: cantidadSolicitada,
            fechaHoraPrestamo: FechaFija);

        return prestamo;
    }

    private static List<UnidadFisica> CrearUnidadesDelTipo(
        TipoEquipamiento tipo,
        int cantidad,
        int primerNumero = 1)
    {
        List<UnidadFisica> unidades = new List<UnidadFisica>();

        for (int i = 0; i < cantidad; i++)
        {
            int numero = primerNumero + i;

            UnidadFisica unidad = new UnidadFisica(
                idInstitucional: $"EQP-{numero:D4}",
                ubiFisica: "Laboratorio Prueba",
                numSerie: null);

            tipo.AgregarUnidad(unidad);
            unidades.Add(unidad);
        }

        return unidades;
    }

    private static UnidadFisica CrearUnidadDeOtroTipo()
    {
        TipoEquipamiento otroTipo = new TipoEquipamiento(
            nombre: "Parlante JBL Flip",
            descripcion: "Parlante portatil para eventos",
            duracionMaxPrestamo: 3,
            requiereAprobacion: false,
            categoria: Categoria.Audiovisual);

        UnidadFisica unidad = new UnidadFisica(
            idInstitucional: "EQP-0901",
            ubiFisica: "Deposito Central",
            numSerie: null);

        otroTipo.AgregarUnidad(unidad);

        return unidad;
    }

    private static (Prestamo prestamo, List<UnidadFisica> unidades) CrearSolicitudDeTipoConAprobacion()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento(requiereAprobacion: true);
        Prestamo prestamo = CrearPrestamo(tipoEquipamiento: tipo);
        List<UnidadFisica> unidades = CrearUnidadesDelTipo(tipo: tipo, cantidad: CantidadSolicitadaPorDefecto);

        return (prestamo, unidades);
    }

    private static (Prestamo prestamo, List<UnidadFisica> unidades) CrearPrestamoAprobado()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearSolicitudDeTipoConAprobacion();

        prestamo.Aprobar(CrearEvaluador(), unidades);

        return (prestamo, unidades);
    }

    private static (Prestamo prestamo, List<UnidadFisica> unidades) CrearPrestamoActivo()
    {
        Prestamo prestamo = CrearPrestamo();
        List<UnidadFisica> unidades =
            CrearUnidadesDelTipo(tipo: prestamo.TipoEquipamiento, cantidad: CantidadSolicitadaPorDefecto);

        prestamo.Activar(unidades);

        return (prestamo, unidades);
    }

    private static (Prestamo prestamo, List<UnidadFisica> unidades) CrearPrestamoDevuelto()
    {
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();

        foreach (UnidadFisica unidad in unidades)
        {
            unidad.Disponible();
        }

        prestamo.MarcarDevuelto();

        return (prestamo, unidades);
    }

    private static Prestamo CrearPrestamoRechazado()
    {
        Prestamo prestamo = CrearPrestamo();

        prestamo.RechazarPorFaltaDeUnidades();

        return prestamo;
    }
}