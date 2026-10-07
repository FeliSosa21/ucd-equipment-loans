using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioPrestamoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);
    private const int CantidadSolicitadaPorDefecto = 2;

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoAgregado_DevuelveElMismoPrestamo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorio.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorio.ObtenerPrestamoPorId(id: prestamo.Id);

        Assert.AreSame(prestamo, resultado);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_IdInexistente_DevuelveNulo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorio.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorio.ObtenerPrestamoPorId(id: Guid.NewGuid());

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerPrestamosActivos_UnoActivoYUnoSolicitado_DevuelveSoloElActivo()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Prestamo solicitado = CrearPrestamo();
        (Prestamo activo, _) = CrearPrestamoActivo();
        repositorio.AgregarPrestamo(solicitado);
        repositorio.AgregarPrestamo(activo);

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosActivos();

        CollectionAssert.AreEquivalent(new List<Prestamo> { activo }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPrestamosActivos_PrestamoDevueltoDespuesDeAgregarlo_NoLoIncluye()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        (Prestamo prestamo, List<UnidadFisica> unidades) = CrearPrestamoActivo();
        repositorio.AgregarPrestamo(prestamo);
        foreach (UnidadFisica unidad in unidades)
        {
            unidad.Disponible();
        }
        prestamo.MarcarDevuelto();

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosActivos();

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerPrestamosPendientes_PrestamosEnVariosEstados_DevuelveSoloLosSolicitados()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        (Prestamo primerPendiente, _) = CrearSolicitudDeTipoConAprobacion();
        (Prestamo segundoPendiente, _) = CrearSolicitudDeTipoConAprobacion();
        (Prestamo aprobado, _) = CrearPrestamoAprobado();
        (Prestamo activo, _) = CrearPrestamoActivo();
        Prestamo rechazado = CrearPrestamoRechazado();

        repositorio.AgregarPrestamo(primerPendiente);
        repositorio.AgregarPrestamo(aprobado);
        repositorio.AgregarPrestamo(activo);
        repositorio.AgregarPrestamo(rechazado);
        repositorio.AgregarPrestamo(segundoPendiente);

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosPendientes();

        CollectionAssert.AreEquivalent(
            new List<Prestamo> { primerPendiente, segundoPendiente }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPrestamosPendientes_SinPendientes_DevuelveListaVacia()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        (Prestamo aprobado, _) = CrearPrestamoAprobado();
        (Prestamo activo, _) = CrearPrestamoActivo();

        repositorio.AgregarPrestamo(aprobado);
        repositorio.AgregarPrestamo(activo);

        IReadOnlyList<Prestamo> resultado = repositorio.ObtenerPrestamosPendientes();

        Assert.IsNotNull(resultado);
        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_OtroRepositorioSobreLaMismaMemoria_LoEncuentra()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorioQueGuarda = new RepositorioPrestamo(memoria);
        RepositorioPrestamo repositorioQueBusca = new RepositorioPrestamo(memoria);
        Prestamo prestamo = CrearPrestamo();
        repositorioQueGuarda.AgregarPrestamo(prestamo);

        Prestamo? resultado = repositorioQueBusca.ObtenerPrestamoPorId(id: prestamo.Id);

        Assert.AreSame(prestamo, resultado);
    }

    [TestMethod]
    public void ObtenerPrestamosDeSolicitante_DosSolicitantes_DevuelveSoloLosDelSolicitante()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Usuario solicitante = CrearSolicitante();
        Usuario otroSolicitante = CrearSolicitante("45678905");
        Prestamo primeroDelSolicitante = CrearPrestamo(solicitante: solicitante);
        Prestamo segundoDelSolicitante = CrearPrestamo(solicitante: solicitante);
        Prestamo delOtroSolicitante = CrearPrestamo(solicitante: otroSolicitante);

        repositorio.AgregarPrestamo(primeroDelSolicitante);
        repositorio.AgregarPrestamo(delOtroSolicitante);
        repositorio.AgregarPrestamo(segundoDelSolicitante);

        IReadOnlyList<Prestamo> resultado =
            repositorio.ObtenerPrestamosDeSolicitante(dni: solicitante.Dni);

        CollectionAssert.AreEquivalent(
            new List<Prestamo> { primeroDelSolicitante, segundoDelSolicitante },
            resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPrestamosDeSolicitante_SinPrestamos_DevuelveListaVacia()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Usuario solicitante = CrearSolicitante();

        IReadOnlyList<Prestamo> resultado =
            repositorio.ObtenerPrestamosDeSolicitante(dni: solicitante.Dni);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ObtenerPrestamosDeSolicitante_PrestamoRechazado_LoIncluye()
    {
        Memoria memoria = new Memoria();
        RepositorioPrestamo repositorio = new RepositorioPrestamo(memoria);
        Usuario solicitante = CrearSolicitante();
        Prestamo rechazado = CrearPrestamo(solicitante: solicitante);
        rechazado.RechazarPorFaltaDeUnidades();
        repositorio.AgregarPrestamo(rechazado);

        IReadOnlyList<Prestamo> resultado =
            repositorio.ObtenerPrestamosDeSolicitante(dni: solicitante.Dni);

        CollectionAssert.Contains(resultado.ToList(), rechazado);
    }

    private static Usuario CrearSolicitante(string dni = "12345672")
    {
        Usuario usuario = new Usuario(
            dni: dni,
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

    private static Prestamo CrearPrestamoRechazado()
    {
        Prestamo prestamo = CrearPrestamo();

        prestamo.RechazarPorFaltaDeUnidades();

        return prestamo;
    }
}