using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioAuditoriaTests
{
    private const string DniAdministrador = "11111111";
    private const string DniDocente = "45678905";
    private static readonly DateOnly Dia = new DateOnly(2026, 11, 5);

    private SqlContext _preparacion = null!;
    private SqlContext _contexto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        DbContextOptions<SqlContext> opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _preparacion = new SqlContext(opciones);
        TiposDePrueba.GuardarEn(_preparacion);
        _contexto = new SqlContext(opciones);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
        _preparacion.Dispose();
    }

    [TestMethod]
    public void ListarRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAuditoria servicio = CrearServicio();

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ListarRegistros(null!, DniAdministrador));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableSinPermiso_LanzaServicioException()
    {
        AgregarUsuario(DniDocente, TipoUsuario.Docente());
        ServicioAuditoria servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniDocente));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableDesactivado_LanzaServicioException()
    {
        AgregarUsuario(DniAdministrador, TipoUsuario.Administrador()).Desactivar();
        _preparacion.SaveChanges();
        ServicioAuditoria servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniAdministrador));
    }

    [TestMethod]
    public void ListarRegistros_FiltroDesde_DevuelveLosDeEseDiaEnAdelante()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue).AddMinutes(-1), "El dia anterior");
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue), "Ese dia");
        ServicioAuditoria servicio = CrearServicio();

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "Ese dia" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_FiltroHasta_IncluyeElDeEseDiaALas2359()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        DateTime diaSiguiente = Dia.AddDays(1).ToDateTime(TimeOnly.MinValue);
        AgregarRegistro(administrador, diaSiguiente.AddMinutes(-1), "Ese dia a las 23:59");
        AgregarRegistro(administrador, diaSiguiente, "El dia siguiente a las 00:00");
        AgregarRegistro(administrador, diaSiguiente.AddMinutes(1), "El dia siguiente a las 00:01");
        ServicioAuditoria servicio = CrearServicio();

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Hasta = Dia }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "Ese dia a las 23:59" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_HastaEsLaUltimaFecha_DevuelveLosRegistrosSinLimiteSuperior()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue), "Ese dia");
        ServicioAuditoria servicio = CrearServicio();
        FiltroAuditoriaDto filtro = new FiltroAuditoriaDto { Desde = Dia, Hasta = DateOnly.MaxValue };

        List<RegistroAuditoriaDto> registros = servicio.ListarRegistros(filtro, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "Ese dia" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_FiltroDni_DevuelveSoloLosDeEseResponsable()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        Usuario docente = AgregarUsuario(DniDocente, TipoUsuario.Docente());
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue), "Del administrador");
        AgregarRegistro(docente, Dia.ToDateTime(TimeOnly.MinValue), "De la docente");
        ServicioAuditoria servicio = CrearServicio();

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { DniUsuario = DniDocente }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "De la docente" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_DniConEspacios_LoRecortaAntesDeFiltrar()
    {
        AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        Usuario docente = AgregarUsuario(DniDocente, TipoUsuario.Docente());
        AgregarRegistro(docente, Dia.ToDateTime(TimeOnly.MinValue), "De la docente");
        ServicioAuditoria servicio = CrearServicio();

        List<RegistroAuditoriaDto> registros = servicio.ListarRegistros(
            new FiltroAuditoriaDto { DniUsuario = $"  {DniDocente}  " }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "De la docente" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_Registro_FechaHoraEnIso8601()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(administrador, Dia.ToDateTime(new TimeOnly(9, 30, 15)), "Alta");
        ServicioAuditoria servicio = CrearServicio();

        RegistroAuditoriaDto registro =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador).Single();

        Assert.AreEqual("2026-11-05T09:30:15", registro.FechaHora);
    }

    [TestMethod]
    public void ListarRegistros_Registro_UsuarioConNombreYDni()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue), "Alta");
        ServicioAuditoria servicio = CrearServicio();

        RegistroAuditoriaDto registro =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador).Single();

        Assert.AreEqual("Ana Perez (DNI 11111111)", registro.Usuario);
    }

    [TestMethod]
    public void ListarRegistros_Registro_AccionConElNombreDelEnum()
    {
        Usuario administrador = AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(administrador, Dia.ToDateTime(TimeOnly.MinValue), "Alta");
        ServicioAuditoria servicio = CrearServicio();

        RegistroAuditoriaDto registro =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador).Single();

        Assert.AreEqual(nameof(AccionAuditada.AltaDeUsuario), registro.Accion);
    }

    [TestMethod]
    public void ListarRegistros_RegistroDelSistema_UsuarioEsSistema()
    {
        AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        new RepositorioAuditoriaSql(_preparacion).AgregarRegistro(new RegistroAuditoria(
            RegistroDeAccion.DelSistema(Dia.ToDateTime(TimeOnly.MinValue)), AccionAuditada.RechazoDePrestamo,
            "Rechazo automatico"));
        ServicioAuditoria servicio = CrearServicio();

        RegistroAuditoriaDto registro =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador).Single();

        Assert.AreEqual("Sistema", registro.Usuario);
    }

    [TestMethod]
    public void ListarRegistros_DesdePosteriorAHasta_LanzaServicioExceptionDelCampoDesde()
    {
        AgregarUsuario(DniAdministrador, TipoUsuario.Administrador());
        ServicioAuditoria servicio = CrearServicio();
        FiltroAuditoriaDto filtro = new FiltroAuditoriaDto { Desde = Dia.AddDays(1), Hasta = Dia };

        ServicioException error =
            Assert.ThrowsException<ServicioException>(() => servicio.ListarRegistros(filtro, DniAdministrador));

        Assert.AreEqual(nameof(FiltroAuditoriaDto.Desde), error.Campo);
    }

    [TestMethod]
    public void TieneCriterios_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAuditoria servicio = CrearServicio();

        Assert.ThrowsException<ArgumentNullException>(() => servicio.TieneCriterios(null!));
    }

    [TestMethod]
    public void TieneCriterios_SoloDniEnBlanco_DevuelveFalse()
    {
        ServicioAuditoria servicio = CrearServicio();

        bool tieneCriterios = servicio.TieneCriterios(new FiltroAuditoriaDto { DniUsuario = "   " });

        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_SoloHastaEnLaUltimaFecha_DevuelveFalse()
    {
        ServicioAuditoria servicio = CrearServicio();

        bool tieneCriterios = servicio.TieneCriterios(new FiltroAuditoriaDto { Hasta = DateOnly.MaxValue });

        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_SoloDni_DevuelveTrue()
    {
        ServicioAuditoria servicio = CrearServicio();

        bool tieneCriterios = servicio.TieneCriterios(new FiltroAuditoriaDto { DniUsuario = DniDocente });

        Assert.IsTrue(tieneCriterios);
    }

    private void AgregarRegistro(Usuario responsable, DateTime fechaHora, string detalle)
    {
        new RepositorioAuditoriaSql(_preparacion).AgregarRegistro(new RegistroAuditoria(
            RegistroDeAccion.DeUsuario(responsable, fechaHora), AccionAuditada.AltaDeUsuario, detalle));
    }

    private Usuario AgregarUsuario(string dni, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: "Ana", apellido: "Perez", correo: $"{dni}@hotmail.com",
            contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(new RepositorioTipoUsuarioSql(_preparacion).ObtenerTipoPorNombre(tipo.Nombre)!);
        new RepositorioUsuarioSql(_preparacion).AgregarUsuario(usuario);
        return usuario;
    }

    private ServicioAuditoria CrearServicio()
    {
        return new ServicioAuditoria(new RepositorioAuditoriaSql(_contexto), new RepositorioUsuarioSql(_contexto));
    }
}
