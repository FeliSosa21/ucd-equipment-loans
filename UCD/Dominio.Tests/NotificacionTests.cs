namespace Dominio.Tests;

[TestClass]
public sealed class NotificacionTests
{
    private static readonly DateTime FechaFija = new(2026, 11, 10, 9, 0, 0);
    private const string DniSolicitante = "12345672";

    [TestMethod]
    public void Constructor_DatosValidos_QuedaSinLeerConSuPrestamoTipoYFechaHora()
    {
        Prestamo prestamo = CrearPrestamo();

        Notificacion notificacion = new Notificacion(prestamo, TipoNotificacion.PrestamoVencido, FechaFija);

        Assert.AreSame(prestamo, notificacion.Prestamo);
        Assert.AreEqual(TipoNotificacion.PrestamoVencido, notificacion.Tipo);
        Assert.AreEqual(FechaFija, notificacion.FechaHora);
        Assert.IsFalse(notificacion.EstaLeida);
    }

    [TestMethod]
    public void Constructor_PrestamoNulo_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
            new Notificacion(null!, TipoNotificacion.PrestamoVencido, FechaFija));
    }

    [TestMethod]
    public void Constructor_TipoNoDefinido_LanzaDominioException()
    {
        Assert.ThrowsException<DominioException>(() =>
            new Notificacion(CrearPrestamo(), (TipoNotificacion)99, FechaFija));
    }

    [TestMethod]
    public void MarcarComoLeida_SinLeer_QuedaLeida()
    {
        Notificacion notificacion = CrearNotificacion();

        notificacion.MarcarComoLeida();

        Assert.IsTrue(notificacion.EstaLeida);
    }

    [TestMethod]
    public void MarcarComoLeida_YaLeida_SigueLeida()
    {
        Notificacion notificacion = CrearNotificacion();
        notificacion.MarcarComoLeida();

        notificacion.MarcarComoLeida();

        Assert.IsTrue(notificacion.EstaLeida);
    }

    [TestMethod]
    public void EsDe_DniDelSolicitante_DevuelveTrue()
    {
        Notificacion notificacion = CrearNotificacion();

        bool esDelSolicitante = notificacion.EsDe(DniSolicitante);

        Assert.IsTrue(esDelSolicitante);
    }

    [TestMethod]
    public void EsDe_OtroDni_DevuelveFalse()
    {
        Notificacion notificacion = CrearNotificacion();

        bool esDeOtro = notificacion.EsDe("45678905");

        Assert.IsFalse(esDeOtro);
    }

    private static Notificacion CrearNotificacion()
    {
        return new Notificacion(CrearPrestamo(), TipoNotificacion.PrestamoVencido, FechaFija);
    }

    private static Prestamo CrearPrestamo()
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: true, categoria: Categoria.Audiovisual);

        return new Prestamo(CrearSolicitante(), tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
    }

    private static Usuario CrearSolicitante()
    {
        return new Usuario(dni: DniSolicitante, nombre: "Renato", apellido: "Alves", correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");
    }
}
