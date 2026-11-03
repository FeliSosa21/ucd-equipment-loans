namespace Dominio.Tests;

[TestClass]
public sealed class RegistroAuditoriaTests
{
    private static readonly DateTime FechaHoraValida = new DateTime(2026, 10, 26, 11, 30, 0);

    [TestMethod]
    public void Constructor_DatosValidos_AsignaLosDatos()
    {
        Usuario responsable = CrearUsuario();

        RegistroAuditoria registro = new RegistroAuditoria(
            registro: RegistroDeAccion.DeUsuario(responsable, FechaHoraValida),
            accion: AccionAuditada.SolicitudDePrestamo,
            detalle: "Prestamo de 2 Notebook");

        Assert.AreEqual(FechaHoraValida, registro.FechaHora);
        Assert.AreSame(responsable, registro.Responsable);
        Assert.AreEqual(AccionAuditada.SolicitudDePrestamo, registro.Accion);
        Assert.AreEqual("Prestamo de 2 Notebook", registro.Detalle);
    }

    [TestMethod]
    public void EsDelSistema_SinResponsable_DevuelveTrue()
    {
        RegistroAuditoria registro = new RegistroAuditoria(
            registro: RegistroDeAccion.DelSistema(FechaHoraValida),
            accion: AccionAuditada.ActivacionDePrestamo,
            detalle: "Prestamo activado automaticamente");

        bool resultado = registro.EsDelSistema();

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public void EsDelSistema_ConResponsable_DevuelveFalse()
    {
        RegistroAuditoria registro = CrearRegistro();

        bool resultado = registro.EsDelSistema();

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(99)]
    public void Constructor_AccionInvalida_LanzaExcepcion(int accion)
    {
        Assert.ThrowsException<DominioException>(() => CrearRegistro(accion: (AccionAuditada)accion));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_DetalleNuloVacioOEspacios_LanzaExcepcion(string? detalle)
    {
        Assert.ThrowsException<DominioException>(() => CrearRegistro(detalle: detalle!));
    }

    [TestMethod]
    public void Constructor_DetalleCon201Caracteres_LanzaExcepcion()
    {
        string detalle = new string('a', 201);

        Assert.ThrowsException<DominioException>(() => CrearRegistro(detalle: detalle));
    }

    [TestMethod]
    public void Constructor_DetalleCon200Caracteres_SeCrea()
    {
        string detalle = new string('a', 200);

        RegistroAuditoria registro = CrearRegistro(detalle: detalle);

        Assert.AreEqual(detalle, registro.Detalle);
    }

    [TestMethod]
    public void Constructor_DetalleConEspaciosAlrededor_LoGuardaRecortado()
    {
        RegistroAuditoria registro = CrearRegistro(detalle: "  Prestamo de 2 Notebook  ");

        Assert.AreEqual("Prestamo de 2 Notebook", registro.Detalle);
    }

    [TestMethod]
    public void Constructor_SinRegistroDeAccion_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => new RegistroAuditoria(
            registro: null!,
            accion: AccionAuditada.SolicitudDePrestamo,
            detalle: "Prestamo de 2 Notebook"));
    }

    [TestMethod]
    public void Constructor_IntentoDeAprobacion_SeCrea()
    {
        RegistroAuditoria registro = CrearRegistro(accion: AccionAuditada.IntentoDeAprobacion);

        Assert.AreEqual(AccionAuditada.IntentoDeAprobacion, registro.Accion);
    }

    private static RegistroAuditoria CrearRegistro(
        AccionAuditada accion = AccionAuditada.SolicitudDePrestamo,
        string detalle = "Prestamo de 2 Notebook")
    {
        return new RegistroAuditoria(
            registro: RegistroDeAccion.DeUsuario(CrearUsuario(), FechaHoraValida),
            accion: accion,
            detalle: detalle);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(
            dni: "12345672",
            nombre: "Felipe",
            apellido: "Sosa",
            correo: "felipe@hotmail.com",
            contrasena: "VivaLaOrt2026");
    }
}
