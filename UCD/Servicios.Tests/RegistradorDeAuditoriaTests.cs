using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

[TestClass]
public class RegistradorDeAuditoriaTests
{
    private static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);

    private RepositorioAuditoria _repositorio = null!;
    private RegistradorDeAuditoria _registrador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorio = new RepositorioAuditoria(new Memoria());
        _registrador = new RegistradorDeAuditoria(_repositorio, new RelojFijo(Ahora));
    }

    [TestMethod]
    public void RegistrarDeUsuario_DatosValidos_GuardaElRegistroConLaHoraDelReloj()
    {
        Usuario responsable = CrearUsuario();

        _registrador.RegistrarDeUsuario(AccionAuditada.RegistroDeUsuario, responsable, "Se registro Felipe Sosa");

        RegistroAuditoria guardado = _repositorio.ObtenerRegistros(new FiltroAuditoria { Desde = Ahora }).Single();
        Assert.AreEqual(Ahora, guardado.FechaHora);
        Assert.AreSame(responsable, guardado.Responsable);
        Assert.AreEqual(AccionAuditada.RegistroDeUsuario, guardado.Accion);
        Assert.AreEqual("Se registro Felipe Sosa", guardado.Detalle);
    }

    [TestMethod]
    public void Registrar_RegistroDeAccionDado_UsaSuFechaYNoLaDelReloj()
    {
        Usuario responsable = CrearUsuario();
        DateTime otraHora = Ahora.AddMinutes(-5);

        _registrador.Registrar(RegistroDeAccion.DeUsuario(responsable, otraHora),
            AccionAuditada.DevolucionDePrestamo, "Devolucion");

        RegistroAuditoria guardado = _repositorio.ObtenerRegistros(new FiltroAuditoria { Desde = otraHora }).Single();
        Assert.AreEqual(otraHora, guardado.FechaHora);
        Assert.AreSame(responsable, guardado.Responsable);
        Assert.AreEqual(AccionAuditada.DevolucionDePrestamo, guardado.Accion);
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
