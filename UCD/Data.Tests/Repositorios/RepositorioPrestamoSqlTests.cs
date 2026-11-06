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

        Prestamo? obtenido = ObtenerConOtroContexto(idInexistente);

        Assert.IsNull(obtenido);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoAgregado_DevuelveSusDatos()
    {
        Prestamo prestamo = CrearPrestamo();
        GuardarConOtroContexto(prestamo);

        Prestamo? obtenido = ObtenerConOtroContexto(prestamo.Id);

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

        Prestamo? obtenido = ObtenerConOtroContexto(prestamo.Id);

        Assert.IsNotNull(obtenido);
        Assert.IsNotNull(obtenido.Solicitante);
        Assert.AreEqual(DniSolicitante, obtenido.Solicitante.Dni);
        Assert.IsNotNull(obtenido.TipoEquipamiento);
        Assert.AreEqual(NombreTipo, obtenido.TipoEquipamiento.Nombre);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_TipoConUnidades_TraeLasUnidadesDelTipo()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento();
        CrearUnidadesDelTipo(tipo);

        Prestamo prestamo = CrearPrestamoDelTipo(tipo);
        GuardarConOtroContexto(prestamo);

        Prestamo? obtenido = ObtenerConOtroContexto(prestamo.Id);

        Assert.IsNotNull(obtenido);
        List<string> idsEsperados = new List<string> { IdPrimeraUnidad, IdSegundaUnidad };
        List<string> idsDelTipo = obtenido.TipoEquipamiento.Unidades.Select(unidad => unidad.IdInstitucional).ToList();
        CollectionAssert.AreEquivalent(idsEsperados, idsDelTipo);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoRechazado_TraeElEvaluador()
    {
        Prestamo prestamo = CrearPrestamo();
        prestamo.Rechazar(CrearEvaluador(), MotivoDelRechazo);
        GuardarConOtroContexto(prestamo);

        Prestamo? obtenido = ObtenerConOtroContexto(prestamo.Id);

        Assert.IsNotNull(obtenido);
        Assert.IsNotNull(obtenido.Evaluador);
        Assert.AreEqual(DniEvaluador, obtenido.Evaluador.Dni);
    }

    [TestMethod]
    public void ObtenerPrestamoPorId_PrestamoActivo_TraeSusUnidadesAsignadas()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento();
        List<UnidadFisica> unidades = CrearUnidadesDelTipo(tipo);

        Prestamo prestamo = CrearPrestamoDelTipo(tipo);
        prestamo.Activar(unidades);
        GuardarConOtroContexto(prestamo);

        Prestamo? obtenido = ObtenerConOtroContexto(prestamo.Id);

        Assert.IsNotNull(obtenido);
        List<string> idsEsperados = new List<string> { IdPrimeraUnidad, IdSegundaUnidad };
        List<string> idsAsignados = obtenido.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList();
        CollectionAssert.AreEquivalent(idsEsperados, idsAsignados);
    }

    [TestMethod]
    public void ActualizarPrestamo_PrestamoObtenidoYRechazado_OtroContextoVeElRechazo()
    {
        Prestamo prestamo = CrearPrestamo();
        GuardarConOtroContexto(prestamo);
        GuardarUsuarioConOtroContexto(CrearEvaluador());

        using SqlContext contextoQueModifica = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueModifica);
        Prestamo obtenido = repositorio.ObtenerPrestamoPorId(prestamo.Id)!;
        Usuario evaluador = contextoQueModifica.Usuarios.Single(usuario => usuario.Dni == DniEvaluador);
        obtenido.Rechazar(evaluador, MotivoDelRechazo);

        repositorio.ActualizarPrestamo(obtenido);

        Prestamo? leido = ObtenerConOtroContexto(prestamo.Id);
        Assert.IsNotNull(leido);
        Assert.AreEqual(EstadoPrestamo.Rechazado, leido.Estado);
        Assert.AreEqual(MotivoDelRechazo, leido.MotivoRechazo);
        Assert.IsNotNull(leido.Evaluador);
        Assert.AreEqual(DniEvaluador, leido.Evaluador.Dni);
    }

    [TestMethod]
    public void ActualizarPrestamo_PrestamoObtenidoYAprobado_OtroContextoLoVeActivoConSusUnidadesPrestadas()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento(requiereAprobacion: true);
        CrearUnidadesDelTipo(tipo);
        Prestamo prestamo = CrearPrestamoDelTipo(tipo);
        GuardarConOtroContexto(prestamo);
        GuardarUsuarioConOtroContexto(CrearEvaluador());

        using SqlContext contextoQueModifica = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueModifica);
        Prestamo obtenido = repositorio.ObtenerPrestamoPorId(prestamo.Id)!;
        Usuario evaluador = contextoQueModifica.Usuarios.Single(usuario => usuario.Dni == DniEvaluador);
        List<UnidadFisica> unidades = obtenido.TipoEquipamiento.UnidadesDisponibles().Take(obtenido.CantidadSolicitada).ToList();
        obtenido.Aprobar(evaluador, unidades);

        repositorio.ActualizarPrestamo(obtenido);

        Prestamo? leido = ObtenerConOtroContexto(prestamo.Id);
        Assert.IsNotNull(leido);
        Assert.AreEqual(EstadoPrestamo.Activo, leido.Estado);
        List<string> idsEsperados = new List<string> { IdPrimeraUnidad, IdSegundaUnidad };
        List<string> idsAsignados = leido.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList();
        CollectionAssert.AreEquivalent(idsEsperados, idsAsignados);
        Assert.AreEqual(0, leido.TipoEquipamiento.CantidadDisponibles());
    }

    [TestMethod]
    public void ObtenerPrestamos_SinFiltros_DevuelveElPrestamoAgregado() 
    {
        Prestamo prestamo = CrearPrestamo();
        GuardarConOtroContexto(prestamo);

        IReadOnlyList<Prestamo> obtenidos = ObtenerPrestamosConOtroContexto(new FiltroPorCatalogoYFechas());

        Assert.AreEqual(1, obtenidos.Count);
        Assert.AreEqual(prestamo.Id, obtenidos[0].Id);
    }

    [TestMethod]
    public void ObtenerPrestamos_PrestamoActivo_TraeSolicitanteTipoYUnidadesAsignadas()
    {
        TipoEquipamiento tipo = CrearTipoEquipamiento();
        List<UnidadFisica> unidades = CrearUnidadesDelTipo(tipo);

        Prestamo prestamo = CrearPrestamoDelTipo(tipo);
        prestamo.Activar(unidades);
        GuardarConOtroContexto(prestamo);

        Prestamo obtenido = ObtenerPrestamosConOtroContexto(new FiltroPorCatalogoYFechas()).Single();

        Assert.IsNotNull(obtenido.Solicitante);
        Assert.AreEqual(DniSolicitante, obtenido.Solicitante.Dni);
        Assert.IsNotNull(obtenido.TipoEquipamiento);
        Assert.AreEqual(NombreTipo, obtenido.TipoEquipamiento.Nombre);
        List<string> idsEsperados = new List<string> { IdPrimeraUnidad, IdSegundaUnidad };
        List<string> idsAsignados = obtenido.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList();
        CollectionAssert.AreEquivalent(idsEsperados, idsAsignados);
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

    private void GuardarUsuarioConOtroContexto(Usuario usuario)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);

        contextoQueGuarda.Usuarios.Add(usuario);
        contextoQueGuarda.SaveChanges();
    }

    private Prestamo? ObtenerConOtroContexto(Guid idPrestamo)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueLee);

        return repositorio.ObtenerPrestamoPorId(idPrestamo);
    }

    private IReadOnlyList<Prestamo> ObtenerPrestamosConOtroContexto(FiltroPorCatalogoYFechas filtro)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioPrestamoSql repositorio = new RepositorioPrestamoSql(contextoQueLee);

        return repositorio.ObtenerPrestamos(filtro);
    }

    private static Prestamo CrearPrestamo()
    {
        return CrearPrestamoDelTipo(CrearTipoEquipamiento());
    }

    private static Prestamo CrearPrestamoDelTipo(TipoEquipamiento tipo)
    {
        return new Prestamo(
            solicitante: CrearSolicitante(),
            tipoEquipamiento: tipo,
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

    private static TipoEquipamiento CrearTipoEquipamiento(bool requiereAprobacion = false)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(
            nombre: NombreTipo,
            descripcion: "Computadora para laboratorio",
            duracionMaxPrestamo: 5,
            requiereAprobacion: requiereAprobacion,
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