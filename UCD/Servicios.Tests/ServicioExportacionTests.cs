using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioExportacionTests
{
    private const string DniAdministrador = "89012349";
    private const string DniSupervisor = "23456783";
    private const string DniDocente = "12345672";
    private const string DniEstudiante = "67890127";
    private const string DniFuncionario = "78901238";
    private const string DniDesactivado = "34567894";
    private const string DniInexistente = "56789016";

    private ContextoDePrueba _datos = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _datos = new ContextoDePrueba();
        _datos.GuardarUsuario(DniAdministrador, TipoUsuario.Administrador());
        _datos.GuardarUsuario(DniSupervisor, TipoUsuario.Supervisor());
        _datos.GuardarUsuario(DniDocente, TipoUsuario.Docente());
        _datos.GuardarUsuario(DniEstudiante, TipoUsuario.Estudiante());
        _datos.GuardarUsuario(DniFuncionario, TipoUsuario.FuncionarioCE());
        _datos.GuardarUsuarioDesactivado(DniDesactivado, TipoUsuario.Administrador());
    }

    [TestCleanup]
    public void Limpiar()
    {
        _datos.Dispose();
    }

    [TestMethod]
    public void ContarRegistros_Administrador_NoLanzaExcepcion()
    {
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(0, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_Supervisor_NoLanzaExcepcion()
    {
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniSupervisor);

        Assert.AreEqual(0, cantidades.Devoluciones);
    }

    [TestMethod]
    public void ContarRegistros_Docente_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.ContarRegistros(CrearRango(1, 5), DniDocente));
    }

    [TestMethod]
    public void ContarRegistros_Estudiante_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.ContarRegistros(CrearRango(1, 5), DniEstudiante));
    }

    [TestMethod]
    public void ContarRegistros_Funcionario_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.ContarRegistros(CrearRango(1, 5), DniFuncionario));
    }

    [TestMethod]
    public void ContarRegistros_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.ContarRegistros(CrearRango(1, 5), DniDesactivado));
    }

    [TestMethod]
    public void ContarRegistros_UsuarioInexistente_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.ContarRegistros(CrearRango(1, 5), DniInexistente));
    }

    [TestMethod]
    public void ContarRegistros_RangoInvertido_LanzaServicioExceptionEnElCampoDesde()
    {
        ServicioExportacion servicio = CrearServicio();

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.ContarRegistros(CrearRango(5, 1), DniAdministrador));

        Assert.AreEqual(nameof(RangoDeExportacionDto.Desde), ex.Campo);
    }

    [TestMethod]
    public void ContarRegistros_MismoDiaEnAmbosExtremos_NoLanzaExcepcion()
    {
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(3, 3), DniAdministrador);

        Assert.AreEqual(0, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_SinFechaDesde_LanzaServicioExceptionEnElCampoDesde()
    {
        ServicioExportacion servicio = CrearServicio();
        RangoDeExportacionDto rango = new RangoDeExportacionDto { Hasta = new DateOnly(2026, 11, 5) };

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.ContarRegistros(rango, DniAdministrador));

        Assert.AreEqual(nameof(RangoDeExportacionDto.Desde), ex.Campo);
    }

    [TestMethod]
    public void ContarRegistros_SinFechaHasta_LanzaServicioExceptionEnElCampoHasta()
    {
        ServicioExportacion servicio = CrearServicio();
        RangoDeExportacionDto rango = new RangoDeExportacionDto { Desde = new DateOnly(2026, 11, 1) };

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.ContarRegistros(rango, DniAdministrador));

        Assert.AreEqual(nameof(RangoDeExportacionDto.Hasta), ex.Campo);
    }

    private ServicioExportacion CrearServicio(params IExportadorDeTabla[] exportadores)
    {
        return new ServicioExportacion(_datos.Prestamos, _datos.Devoluciones, _datos.Catalogos, _datos.Usuarios,
            exportadores);
    }

    private static RangoDeExportacionDto CrearRango(int diaDesde, int diaHasta)
    {
        return new RangoDeExportacionDto
        {
            Desde = new DateOnly(2026, 11, diaDesde),
            Hasta = new DateOnly(2026, 11, diaHasta)
        };
    }
}
