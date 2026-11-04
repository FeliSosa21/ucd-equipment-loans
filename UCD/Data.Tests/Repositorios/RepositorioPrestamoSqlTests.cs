using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioPrestamoSqlTests
{
    private const string DniSolicitante = "52083412";
    private const string DniEvaluador = "67890127";
    private const string NombreTipo = "Notebook Lenovo ThinkPad";
    private const int CantidadSolicitada = 2;
    private const string IdPrimeraUnidad = "EQP-0001";
    private const string IdSegundaUnidad = "EQP-0002";
    private const string UbicacionUnidad = "Laboratorio Prueba";
    private const string MotivoDelRechazo = "Equipo reservado para un evento";
    private static readonly DateTime FechaFija = new(2026, 11, 3, 10, 0, 0);

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    }

    [TestMethod]
    public void AgregarPrestamo_PrestamoValido_OtroContextoLoEncuentra()
    {
        Prestamo prestamo = CrearPrestamo();

        GuardarConOtroContexto(prestamo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo? leido = contextoQueLee.Prestamos.Find(prestamo.Id);

        Assert.IsNotNull(leido);
        Assert.AreEqual(prestamo.CantidadSolicitada, leido.CantidadSolicitada);
        Assert.AreEqual(prestamo.FechaHoraPrestamo, leido.FechaHoraPrestamo);
        Assert.AreEqual(prestamo.FechaEsperadaDevolucion, leido.FechaEsperadaDevolucion);
    }

    [TestMethod]
    public void AgregarPrestamo_PrestamoValido_OtroContextoLeeSolicitanteYTipo()
    {
        Prestamo prestamo = CrearPrestamo();

        GuardarConOtroContexto(prestamo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo leido = contextoQueLee.Prestamos
            .Include(guardado => guardado.Solicitante)
            .Include(guardado => guardado.TipoEquipamiento)
            .Single(guardado => guardado.Id == prestamo.Id);

        Assert.AreEqual(DniSolicitante, leido.Solicitante.Dni);
        Assert.AreEqual(NombreTipo, leido.TipoEquipamiento.Nombre);
    }

    [TestMethod]
    public void AgregarPrestamo_UnidadesPrestadasDeNuevo_CadaPrestamoConservaSusUnidades()
    {
        Usuario solicitante = CrearSolicitante();
        TipoEquipamiento tipo = CrearTipoEquipamiento();
        List<UnidadFisica> unidades = CrearUnidadesDelTipo(tipo);

        Prestamo devuelto = new Prestamo(
            solicitante: solicitante,
            tipoEquipamiento: tipo,
            cantidadSolicitada: CantidadSolicitada,
            fechaHoraPrestamo: FechaFija);
        devuelto.Activar(unidades);
        Devolver(devuelto);

        Prestamo activo = new Prestamo(
            solicitante: solicitante,
            tipoEquipamiento: tipo,
            cantidadSolicitada: CantidadSolicitada,
            fechaHoraPrestamo: FechaFija.AddDays(10));
        activo.Activar(unidades);

        GuardarConOtroContexto(devuelto, activo);

        List<string> idsEsperados = new List<string> { IdPrimeraUnidad, IdSegundaUnidad };
        CollectionAssert.AreEquivalent(idsEsperados, LeerIdsDeUnidades(devuelto.Id));
        CollectionAssert.AreEquivalent(idsEsperados, LeerIdsDeUnidades(activo.Id));
    }

    [TestMethod]
    public void AgregarPrestamo_PrestamoRechazado_OtroContextoLeeEstadoEvaluadorYMotivo()
    {
        Prestamo prestamo = CrearPrestamo();
        prestamo.Rechazar(CrearEvaluador(), MotivoDelRechazo);

        GuardarConOtroContexto(prestamo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo leido = contextoQueLee.Prestamos
            .Include(guardado => guardado.Solicitante)
            .Include(guardado => guardado.Evaluador)
            .Single(guardado => guardado.Id == prestamo.Id);

        Assert.AreEqual(EstadoPrestamo.Rechazado, leido.Estado);
        Assert.IsNotNull(leido.Evaluador);
        Assert.AreEqual(DniEvaluador, leido.Evaluador.Dni);
        Assert.AreEqual(DniSolicitante, leido.Solicitante.Dni);
        Assert.AreEqual(MotivoDelRechazo, leido.MotivoRechazo);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_IdInexistente_DevuelveNulo()
    {
        GuardarConOtroContexto(CrearPrestamo());
        Guid idInexistente = Guid.NewGuid();

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueLee);
        Prestamo? obtenido = repositorio.ObtenerPrestamoPorId(idInexistente);

        Assert.IsNull(obtenido);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoAgregado_DevuelveSusDatos()
    {
        Prestamo prestamo = CrearPrestamo();
        GuardarConOtroContexto(prestamo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueLee);
        Prestamo? obtenido = repositorio.ObtenerPrestamoPorId(prestamo.Id);

        Assert.IsNotNull(obtenido);
        Assert.AreEqual(prestamo.Id, obtenido.Id);
        Assert.AreEqual(prestamo.CantidadSolicitada, obtenido.CantidadSolicitada);
        Assert.AreEqual(prestamo.FechaHoraPrestamo, obtenido.FechaHoraPrestamo);
        Assert.AreEqual(prestamo.FechaEsperadaDevolucion, obtenido.FechaEsperadaDevolucion);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoAgregado_TraeSolicitanteYTipo()
    {
        Prestamo prestamo = CrearPrestamo();
        GuardarConOtroContexto(prestamo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueLee);
        Prestamo? obtenido =  repositorio.ObtenerPrestamoPorId(prestamo.Id);

        Assert.IsNotNull(obtenido);
        Assert.IsNotNull(obtenido.Solicitante);
        Assert.AreEqual(DniSolicitante, obtenido.Solicitante.Dni);
        Assert.IsNotNull(obtenido.TipoEquipamiento);
        Assert.AreEqual(NombreTipo, obtenido.TipoEquipamiento.Nombre);
    }

    private void GuardarConOtroContexto(params Prestamo[] prestamos)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueGuarda);

        foreach (Prestamo prestamo in prestamos)
        {
            repositorio.AgregarPrestamo(prestamo);
        }
    }

    private static Prestamo CrearPrestamo()
    {
        return new Prestamo(
            solicitante: CrearSolicitante(),
            tipoEquipamiento: CrearTipoEquipamiento(),
            cantidadSolicitada: CantidadSolicitada,
            fechaHoraPrestamo: FechaFija);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: DniSolicitante,
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente());

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(
            dni: DniEvaluador,
            nombre: "Sofia",
            apellido: "Martinez",
            correo: "sofia.martinez@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Supervisor());

        return usuario;
    }

    private static TipoEquipamiento CrearTipoEquipamiento()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(
            nombre: NombreTipo,
            descripcion: "Computadora para laboratorio",
            duracionMaxPrestamo: 5,
            requiereAprobacion: false,
            categoria: Categoria.Computacion);

        return tipo;
    }

    private List<string> LeerIdsDeUnidades(Guid idPrestamo)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Prestamo leido = contextoQueLee.Prestamos
            .Include(guardado => guardado.UnidadesAsignadas)
            .Single(guardado => guardado.Id == idPrestamo);

        return leido.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList();
    }

    private static List<UnidadFisica> CrearUnidadesDelTipo(TipoEquipamiento tipo)
    {
        UnidadFisica primera = new UnidadFisica(idInstitucional: IdPrimeraUnidad, ubiFisica: UbicacionUnidad, numSerie: null);
        UnidadFisica segunda = new UnidadFisica(idInstitucional: IdSegundaUnidad, ubiFisica: UbicacionUnidad, numSerie: null);

        tipo.AgregarUnidad(primera);
        tipo.AgregarUnidad(segunda);

        return new List<UnidadFisica> { primera, segunda };
    }

    private static void Devolver(Prestamo prestamo)
    {
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            unidad.Disponible();
        }

        prestamo.MarcarDevuelto();
    }
}