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
            fechaHora: FechaHoraValida,
            responsable: responsable,
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
            fechaHora: FechaHoraValida,
            responsable: null,
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

    private static RegistroAuditoria CrearRegistro(
        DateTime? fechaHora = null,
        AccionAuditada accion = AccionAuditada.SolicitudDePrestamo,
        string detalle = "Prestamo de 2 Notebook")
    {
        return new RegistroAuditoria(
            fechaHora: fechaHora ?? FechaHoraValida,
            responsable: CrearUsuario(),
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
