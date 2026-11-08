using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioAuditoriaTests
{
    private const string DniAdministrador = "11111111";
    private const string DniDocente = "45678905";
    private static readonly DateOnly Dia = new DateOnly(2026, 11, 5);

    [TestMethod]
    public void ListarRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAuditoria servicio = CrearServicio(new Memoria());

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ListarRegistros(null!, DniAdministrador));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableSinPermiso_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        AgregarUsuario(memoria, DniDocente, TipoUsuario.Docente());
        ServicioAuditoria servicio = CrearServicio(memoria);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniDocente));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableDesactivado_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador()).Desactivar();
        ServicioAuditoria servicio = CrearServicio(memoria);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniAdministrador));
    }

    [TestMethod]
    public void ListarRegistros_FiltroDesde_DevuelveLosDeEseDiaEnAdelante()
    {
        Memoria memoria = new Memoria();
        Usuario administrador = AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(memoria, administrador, Dia.ToDateTime(TimeOnly.MinValue).AddMinutes(-1), "El dia anterior");
        AgregarRegistro(memoria, administrador, Dia.ToDateTime(TimeOnly.MinValue), "Ese dia");
        ServicioAuditoria servicio = CrearServicio(memoria);

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "Ese dia" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_FiltroHasta_IncluyeElDeEseDiaALas2359()
    {
        Memoria memoria = new Memoria();
        Usuario administrador = AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador());
        DateTime diaSiguiente = Dia.AddDays(1).ToDateTime(TimeOnly.MinValue);
        AgregarRegistro(memoria, administrador, diaSiguiente.AddMinutes(-1), "Ese dia a las 23:59");
        AgregarRegistro(memoria, administrador, diaSiguiente, "El dia siguiente a las 00:00");
        AgregarRegistro(memoria, administrador, diaSiguiente.AddMinutes(1), "El dia siguiente a las 00:01");
        ServicioAuditoria servicio = CrearServicio(memoria);

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Hasta = Dia }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "Ese dia a las 23:59" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_FiltroDni_DevuelveSoloLosDeEseResponsable()
    {
        Memoria memoria = new Memoria();
        Usuario administrador = AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador());
        Usuario docente = AgregarUsuario(memoria, DniDocente, TipoUsuario.Docente());
        AgregarRegistro(memoria, administrador, Dia.ToDateTime(TimeOnly.MinValue), "Del administrador");
        AgregarRegistro(memoria, docente, Dia.ToDateTime(TimeOnly.MinValue), "De la docente");
        ServicioAuditoria servicio = CrearServicio(memoria);

        List<RegistroAuditoriaDto> registros =
            servicio.ListarRegistros(new FiltroAuditoriaDto { DniUsuario = DniDocente }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "De la docente" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_DniConEspacios_LoRecortaAntesDeFiltrar()
    {
        Memoria memoria = new Memoria();
        AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador());
        Usuario docente = AgregarUsuario(memoria, DniDocente, TipoUsuario.Docente());
        AgregarRegistro(memoria, docente, Dia.ToDateTime(TimeOnly.MinValue), "De la docente");
        ServicioAuditoria servicio = CrearServicio(memoria);

        List<RegistroAuditoriaDto> registros = servicio.ListarRegistros(
            new FiltroAuditoriaDto { DniUsuario = $"  {DniDocente}  " }, DniAdministrador);

        CollectionAssert.AreEqual(new List<string> { "De la docente" },
            registros.Select(registro => registro.Detalle).ToList());
    }

    [TestMethod]
    public void ListarRegistros_Registro_FechaHoraEnIso8601()
    {
        Memoria memoria = new Memoria();
        Usuario administrador = AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador());
        AgregarRegistro(memoria, administrador, Dia.ToDateTime(new TimeOnly(9, 30, 15)), "Alta");
        ServicioAuditoria servicio = CrearServicio(memoria);

        RegistroAuditoriaDto registro =
            servicio.ListarRegistros(new FiltroAuditoriaDto { Desde = Dia }, DniAdministrador).Single();

        Assert.AreEqual("2026-11-05T09:30:15", registro.FechaHora);
    }

    [TestMethod]
    public void TieneCriterios_SoloDniEnBlanco_DevuelveFalse()
    {
        ServicioAuditoria servicio = CrearServicio(new Memoria());

        bool tieneCriterios = servicio.TieneCriterios(new FiltroAuditoriaDto { DniUsuario = "   " });

        Assert.IsFalse(tieneCriterios);
    }

    private static void AgregarRegistro(Memoria memoria, Usuario responsable, DateTime fechaHora, string detalle)
    {
        new RepositorioAuditoria(memoria).AgregarRegistro(new RegistroAuditoria(
            RegistroDeAccion.DeUsuario(responsable, fechaHora), AccionAuditada.AltaDeUsuario, detalle));
    }

    private static Usuario AgregarUsuario(Memoria memoria, string dni, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: "Ana", apellido: "Perez", correo: $"{dni}@hotmail.com",
            contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);
        new RepositorioUsuario(memoria).AgregarUsuario(usuario);
        return usuario;
    }

    private static ServicioAuditoria CrearServicio(Memoria memoria)
    {
        return new ServicioAuditoria(new RepositorioAuditoria(memoria), new RepositorioUsuario(memoria));
    }
}
