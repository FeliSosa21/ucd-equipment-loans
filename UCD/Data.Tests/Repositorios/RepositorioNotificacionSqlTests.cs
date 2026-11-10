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
