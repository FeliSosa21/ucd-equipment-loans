using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class RegistradorDeAuditoriaTests
{
    private static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;
    private RegistradorDeAuditoria _registrador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
        _registrador = new RegistradorDeAuditoria(new RepositorioAuditoriaSql(_contexto), new RelojFijo(Ahora));
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void RegistrarDeUsuario_DatosValidos_GuardaElRegistroConLaHoraDelReloj()
    {
        Usuario responsable = CrearUsuario();

        _registrador.RegistrarDeUsuario(AccionAuditada.RegistroDeUsuario, responsable, "Se registro Felipe Sosa");

        using SqlContext lectura = new SqlContext(_opciones);
        RegistroAuditoria guardado = new RepositorioAuditoriaSql(lectura).ObtenerRegistros(new FiltroAuditoria { Desde = Ahora }).Single();
        Assert.AreEqual(Ahora, guardado.FechaHora);
        Assert.AreEqual(responsable.Id, guardado.Responsable!.Id);
        Assert.AreEqual(responsable.Dni, guardado.Responsable.Dni);
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

        using SqlContext lectura = new SqlContext(_opciones);
        RegistroAuditoria guardado = new RepositorioAuditoriaSql(lectura).ObtenerRegistros(new FiltroAuditoria { Desde = otraHora }).Single();
        Assert.AreEqual(otraHora, guardado.FechaHora);
        Assert.AreEqual(responsable.Id, guardado.Responsable!.Id);
        Assert.AreEqual(responsable.Dni, guardado.Responsable.Dni);
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
