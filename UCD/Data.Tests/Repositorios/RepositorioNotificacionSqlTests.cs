using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioNotificacionSqlTests
{
    private static readonly DateTime FechaFija = new(2026, 11, 10, 9, 0, 0);
    private const string DniSolicitante = "12345672";
    private const string DniOtroSolicitante = "45678905";

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void AgregarNotificacion_NotificacionNueva_OtroContextoLaLeeSinLeerConSuPrestamoTipoYFecha()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);

        GuardarConOtroContexto(new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        List<Notificacion> leidas = contextoQueLee.Notificaciones
            .Include(notificacion => notificacion.Prestamo)
            .ToList();

        Assert.AreEqual(1, leidas.Count);
        Assert.AreEqual(prestamo.Id, leidas[0].Prestamo.Id);
        Assert.AreEqual(TipoNotificacion.PrestamoVencido, leidas[0].Tipo);
        Assert.AreEqual(FechaFija, leidas[0].FechaHora);
        Assert.IsFalse(leidas[0].EstaLeida);
    }

    [TestMethod]
    public void ObtenerNoLeidasDeUsuario_NotificacionesDeDosUsuarios_DevuelveSoloLasDelUsuario()
    {
        Notificacion delSolicitante = CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante));
        Notificacion deOtro = CrearNotificacionDeVencido(CrearPrestamoActivo(DniOtroSolicitante));
        GuardarConOtroContexto(delSolicitante, deOtro);

        IReadOnlyList<Notificacion> resultado = LeerNoLeidasDe(DniSolicitante);

        CollectionAssert.AreEqual(new List<int> { delSolicitante.Id },
            resultado.Select(notificacion => notificacion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerNoLeidasDeUsuario_NotificacionGuardada_TraeElSolicitanteYElTipoDeEquipamientoDelPrestamo()
    {
        GuardarConOtroContexto(CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante)));

        Notificacion leida = LeerNoLeidasDe(DniSolicitante).Single();

        Assert.IsNotNull(leida.Prestamo);
        Assert.AreEqual(DniSolicitante, leida.Prestamo.Solicitante.Dni);
        Assert.AreEqual("Proyector Epson", leida.Prestamo.TipoEquipamiento.Nombre);
    }

    [TestMethod]
    public void ObtenerNoLeidasDeUsuario_UnaLeidaYOtraSinLeer_DevuelveSoloLaSinLeer()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        Notificacion leida = CrearNotificacionDeVencido(prestamo);
        leida.MarcarComoLeida();
        Notificacion sinLeer = CrearNotificacionDeVencido(prestamo);
        GuardarConOtroContexto(leida, sinLeer);

        IReadOnlyList<Notificacion> resultado = LeerNoLeidasDe(DniSolicitante);

        CollectionAssert.AreEqual(new List<int> { sinLeer.Id },
            resultado.Select(notificacion => notificacion.Id).ToList());
    }

    [TestMethod]
    public void ObtenerNoLeidasDeUsuario_VariasSinLeer_DeLaMasRecienteALaMasViejaYEnEmpateLaUltimaRegistrada()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        Notificacion primeraDelDiaSiguiente =
            new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija.AddDays(1));
        Notificacion delDia = new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija);
        Notificacion segundaDelDiaSiguiente =
            new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija.AddDays(1));
        GuardarConOtroContexto(primeraDelDiaSiguiente, delDia, segundaDelDiaSiguiente);

        IReadOnlyList<Notificacion> resultado = LeerNoLeidasDe(DniSolicitante);

        CollectionAssert.AreEqual(
            new List<int> { segundaDelDiaSiguiente.Id, primeraDelDiaSiguiente.Id, delDia.Id },
            resultado.Select(notificacion => notificacion.Id).ToList());
    }

    [TestMethod]
    public void ExisteVencidoSinLeer_PrestamoConUnaDeVencidoSinLeer_DevuelveTrue()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        GuardarConOtroContexto(CrearNotificacionDeVencido(prestamo));

        bool existe = LeerSiExisteVencidoSinLeer(prestamo.Id);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExisteVencidoSinLeer_LaDeVencidoEstaLeida_DevuelveFalse()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        Notificacion leida = CrearNotificacionDeVencido(prestamo);
        leida.MarcarComoLeida();
        GuardarConOtroContexto(leida);

        bool existe = LeerSiExisteVencidoSinLeer(prestamo.Id);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ExisteVencidoSinLeer_SoloTieneUnaDeOtroTipo_DevuelveFalse()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        GuardarConOtroContexto(new Notificacion(prestamo, TipoNotificacion.SolicitudAprobada, FechaFija));

        bool existe = LeerSiExisteVencidoSinLeer(prestamo.Id);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ExisteVencidoSinLeer_LaDeVencidoEsDeOtroPrestamo_DevuelveFalse()
    {
        Prestamo prestamo = CrearPrestamoActivo(DniSolicitante);
        Prestamo otroPrestamo = CrearPrestamoActivo(DniOtroSolicitante);
        GuardarConOtroContexto(CrearNotificacionDeVencido(otroPrestamo));

        bool existe = LeerSiExisteVencidoSinLeer(prestamo.Id);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ObtenerNotificacionPorId_IdExistente_LaTraeConElSolicitanteYElTipoDeEquipamientoDelPrestamo()
    {
        Notificacion otra = CrearNotificacionDeVencido(CrearPrestamoActivo(DniOtroSolicitante));
        Notificacion buscada = CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante));
        GuardarConOtroContexto(otra, buscada);

        Notificacion? leida = LeerNotificacionPorId(buscada.Id);

        Assert.IsNotNull(leida);
        Assert.AreEqual(buscada.Id, leida.Id);
        Assert.AreEqual(DniSolicitante, leida.Prestamo.Solicitante.Dni);
        Assert.AreEqual("Proyector Epson", leida.Prestamo.TipoEquipamiento.Nombre);
    }

    [TestMethod]
    public void ActualizarNotificacion_MarcadaComoLeida_OtroContextoLaLeeLeida()
    {
        Notificacion notificacion = CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante));
        GuardarConOtroContexto(notificacion);

        using (SqlContext contextoQueModifica = new SqlContext(_opciones))
        {
            RepositorioNotificacionSql repositorio = new RepositorioNotificacionSql(contextoQueModifica);
            Notificacion guardada = repositorio.ObtenerNotificacionPorId(notificacion.Id)!;
            guardada.MarcarComoLeida();
            repositorio.ActualizarNotificacion(guardada);
        }

        Assert.IsTrue(LeerNotificacionPorId(notificacion.Id)!.EstaLeida);
    }

    [TestMethod]
    public void AgregarNotificacion_LaBaseRechazaElGuardado_LanzaInvalidOperationExceptionConLaOriginalComoInterna()
    {
        using SqlContext contextoQueFalla = new ContextoQueFallaAlGuardar(_opciones);
        RepositorioNotificacionSql repositorio = new RepositorioNotificacionSql(contextoQueFalla);
        Notificacion notificacion = CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante));

        InvalidOperationException excepcion =
            Assert.ThrowsException<InvalidOperationException>(() => repositorio.AgregarNotificacion(notificacion));

        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DbUpdateException));
    }

    [TestMethod]
    public void AgregarNotificacion_LaBaseRechazaElGuardado_DejaElSeguimientoVacio()
    {
        using SqlContext contextoQueFalla = new ContextoQueFallaAlGuardar(_opciones);
        RepositorioNotificacionSql repositorio = new RepositorioNotificacionSql(contextoQueFalla);
        Notificacion notificacion = CrearNotificacionDeVencido(CrearPrestamoActivo(DniSolicitante));

        Assert.ThrowsException<InvalidOperationException>(() => repositorio.AgregarNotificacion(notificacion));

        Assert.IsFalse(contextoQueFalla.ChangeTracker.Entries().Any());
    }

    private Notificacion? LeerNotificacionPorId(int id)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioNotificacionSql(contextoQueLee).ObtenerNotificacionPorId(id);
    }

    private bool LeerSiExisteVencidoSinLeer(Guid idPrestamo)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioNotificacionSql(contextoQueLee).ExisteVencidoSinLeer(idPrestamo);
    }

    private IReadOnlyList<Notificacion> LeerNoLeidasDe(string dni)
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);

        return new RepositorioNotificacionSql(contextoQueLee).ObtenerNoLeidasDeUsuario(dni);
    }

    private static Notificacion CrearNotificacionDeVencido(Prestamo prestamo)
    {
        return new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija);
    }

    private void GuardarConOtroContexto(params Notificacion[] notificaciones)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioNotificacionSql repositorio = new RepositorioNotificacionSql(contextoQueGuarda);

        foreach (Notificacion notificacion in notificaciones)
        {
            repositorio.AgregarNotificacion(notificacion);
        }
    }

    private static Prestamo CrearPrestamoActivo(string dniSolicitante)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Equipo de prueba",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null);
        tipo.AgregarUnidad(unidad);
        Prestamo prestamo = new Prestamo(CrearUsuario(dniSolicitante), tipo, cantidadSolicitada: 1,
            fechaHoraPrestamo: FechaFija);
        prestamo.Activar(new List<UnidadFisica> { unidad });

        return prestamo;
    }

    private static Usuario CrearUsuario(string dni)
    {
        return new Usuario(dni: dni, nombre: "Renato", apellido: "Alves", correo: $"usuario{dni}@ejemplo.com",
            contrasena: "VivaLaOrt2026");
    }
}
