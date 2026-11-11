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

    private const string TipoActivo = "Proyector Epson";
    private const string TipoDelCatalogoEliminado = "Notebook Lenovo";

    private ContextoDePrueba _datos = null!;
    private int _idCatalogoActivo;
    private int _idCatalogoEliminado;

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
        _idCatalogoActivo = _datos.GuardarCatalogo("Audiovisual", CrearTipo(TipoActivo, "EQP-000"));
        _idCatalogoEliminado = _datos.GuardarCatalogo("Computacion vieja", CrearTipo(TipoDelCatalogoEliminado, "NTB-000"));
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

    [TestMethod]
    public void ContarRegistros_PrestamoALas2359DelUltimoDia_Cuenta()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 5, 23, 59, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(1, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_PrestamoALas0000DelDiaSiguiente_NoCuenta()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 6, 0, 0, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(0, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_PrestamoALas0000DelPrimerDia_Cuenta()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 1, 0, 0, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(1, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_PrestamoALas2359DelDiaAnterior_NoCuenta()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 10, 31, 23, 59, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(0, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_DevolucionALas2359DelUltimoDia_Cuenta()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 5, 23, 59, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(1, cantidades.Devoluciones);
    }

    [TestMethod]
    public void ContarRegistros_DevolucionALas0000DelDiaSiguiente_NoCuenta()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 6, 0, 0, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniAdministrador);

        Assert.AreEqual(0, cantidades.Devoluciones);
    }

    [TestMethod]
    public void ContarRegistros_DevolucionDeUnPrestamoSolicitadoAntesDelRango_CuentaPorFechaDeRegistro()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 3, 9, 0, 0));
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(2, 4), DniAdministrador);

        Assert.AreEqual(1, cantidades.Devoluciones);
        Assert.AreEqual(0, cantidades.Prestamos);
    }

    [TestMethod]
    public void ContarRegistros_CatalogosActivoYEliminado_CuentaLosDos()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 2, 10, 0, 0));
        GuardarPrestamo(_idCatalogoEliminado, TipoDelCatalogoEliminado, new DateTime(2026, 11, 3, 10, 0, 0));
        GuardarDevolucion(_idCatalogoEliminado, TipoDelCatalogoEliminado, new DateTime(2026, 11, 4, 10, 0, 0));
        _datos.DarDeBajaCatalogo(_idCatalogoEliminado);
        ServicioExportacion servicio = CrearServicio();

        CantidadesAExportarDto cantidades = servicio.ContarRegistros(CrearRango(1, 5), DniSupervisor);

        Assert.AreEqual(2, cantidades.Prestamos);
        Assert.AreEqual(1, cantidades.Devoluciones);
    }

    private ServicioExportacion CrearServicio(params IExportadorDeTabla[] exportadores)
    {
        return new ServicioExportacion(_datos.Prestamos, _datos.Devoluciones, _datos.Catalogos, _datos.Usuarios,
            exportadores);
    }

    private void GuardarPrestamo(int idCatalogo, string nombreTipo, DateTime fechaPrestamo)
    {
        _datos.GuardarPrestamoActivoEn(CrearRegistro(idCatalogo, nombreTipo, fechaPrestamo, fechaPrestamo));
    }

    // El prestamo se solicita un mes antes de la devolucion: solo cuenta la fecha en que se registro la devolucion.
    private void GuardarDevolucion(int idCatalogo, string nombreTipo, DateTime fechaDevolucion)
    {
        _datos.GuardarDevolucion(CrearRegistro(idCatalogo, nombreTipo, fechaDevolucion.AddDays(-30), fechaDevolucion));
    }

    private static RegistroDePrueba CrearRegistro(int idCatalogo, string nombreTipo, DateTime fechaPrestamo,
        DateTime fechaDevolucion)
    {
        return new RegistroDePrueba
        {
            IdCatalogo = idCatalogo,
            NombreTipo = nombreTipo,
            DniSolicitante = DniDocente,
            FechaPrestamo = fechaPrestamo,
            DniQuienRegistra = DniFuncionario,
            FechaDevolucion = fechaDevolucion
        };
    }

    // Varias unidades por tipo: cada prestamo activo ocupa una.
    private static TipoEquipamiento CrearTipo(string nombre, string prefijoDeUnidades)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre, "Equipo de prueba", duracionMaxPrestamo: 5,
            requiereAprobacion: false, categoria: Categoria.Audiovisual);
        for (int numero = 1; numero <= 6; numero++)
        {
            tipo.AgregarUnidad(new UnidadFisica(prefijoDeUnidades + numero, "Deposito A", numSerie: null));
        }

        return tipo;
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
