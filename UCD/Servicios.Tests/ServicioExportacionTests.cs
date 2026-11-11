using System.Text;
using ClosedXML.Excel;
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

    [TestMethod]
    public void Exportar_FormatoDesconocido_LanzaServicioExceptionEnElCampoFormato()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.Exportar(CrearPedido("PDF"), DniAdministrador));

        Assert.AreEqual(nameof(PedidoExportacionDto.Formato), ex.Campo);
    }

    [TestMethod]
    public void Exportar_SinFormato_LanzaServicioExceptionEnElCampoFormato()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.Exportar(CrearPedido(string.Empty), DniAdministrador));

        Assert.AreEqual(nameof(PedidoExportacionDto.Formato), ex.Campo);
    }

    [TestMethod]
    public void Exportar_ResponsableSinPermiso_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());

        Assert.ThrowsException<ServicioException>(() => servicio.Exportar(CrearPedido("TXT"), DniDocente));
    }

    [TestMethod]
    public void Exportar_ResponsableDesactivado_LanzaServicioException()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());

        Assert.ThrowsException<ServicioException>(() => servicio.Exportar(CrearPedido("TXT"), DniDesactivado));
    }

    [TestMethod]
    public void Exportar_RangoInvertido_LanzaServicioExceptionEnElCampoDesde()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());
        PedidoExportacionDto pedido = CrearPedido("TXT");
        pedido.Rango = CrearRango(5, 1);

        ServicioException ex = Assert.ThrowsException<ServicioException>(
            () => servicio.Exportar(pedido, DniAdministrador));

        Assert.AreEqual(nameof(RangoDeExportacionDto.Desde), ex.Campo);
    }

    [TestMethod]
    public void Exportar_FormatoConocido_DevuelveLosDosArchivosConNombreExtensionYTipoDeContenido()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorDePrueba());

        List<ArchivoExportadoDto> archivos = servicio.Exportar(CrearPedido("TXT"), DniSupervisor);

        Assert.AreEqual(2, archivos.Count);
        Assert.AreEqual("prestamos_2026-11-01_2026-11-05.txt", archivos[0].Nombre);
        Assert.AreEqual("devoluciones_2026-11-01_2026-11-05.txt", archivos[1].Nombre);
        Assert.AreEqual("text/plain", archivos[0].TipoContenido);
        Assert.AreEqual("text/plain", archivos[1].TipoContenido);
    }

    [TestMethod]
    public void Exportar_FormatoConocido_ElContenidoEsLoQueDevuelveElExportador()
    {
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        List<ArchivoExportadoDto> archivos = servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        Assert.AreEqual(2, exportador.TablasRecibidas.Count);
        CollectionAssert.AreEqual(exportador.Exportar(exportador.TablasRecibidas[0]), archivos[0].Contenido);
    }

    [TestMethod]
    public void Exportar_VariosExportadores_UsaElQueDeclaraElFormatoPedido()
    {
        ExportadorDePrueba otro = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(new ExportadorCsv(), otro, new ExportadorXlsx());

        List<ArchivoExportadoDto> archivos = servicio.Exportar(CrearPedido("CSV"), DniAdministrador);

        Assert.AreEqual(0, otro.TablasRecibidas.Count);
        Assert.AreEqual("text/csv", archivos[0].TipoContenido);
        Assert.IsTrue(archivos[1].Nombre.EndsWith(".csv"));
    }

    [TestMethod]
    public void Exportar_Prestamos_LosEncabezadosSonLosAcordados()
    {
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        CollectionAssert.AreEqual(new List<string>
        {
            "Catalogo", "Fecha de solicitud", "Solicitante", "Tipo de equipamiento", "Cantidad",
            "Unidades asignadas", "Estado", "Evaluador", "Fecha esperada de devolucion"
        }, TablaDePrestamos(exportador).Encabezados);
    }

    [TestMethod]
    public void Exportar_UnPrestamoActivo_ArmaLaFilaConSusDatos()
    {
        DateTime solicitud = new DateTime(2026, 11, 2, 10, 30, 15);
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, solicitud);
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        string esperada = CalendarioLaboral.SumarDiasHabiles(solicitud, 5).ToString("yyyy-MM-dd");
        CollectionAssert.AreEqual(new List<string>
        {
            "Audiovisual", "2026-11-02T10:30:15", "Nombre Apellido (DNI 12345672)", TipoActivo, "1", "EQP-0001",
            "Activo", string.Empty, esperada
        }, TablaDePrestamos(exportador).Filas.Single());
    }

    [TestMethod]
    public void Exportar_PrestamoAprobado_MuestraAlEvaluador()
    {
        _datos.GuardarPrestamoActivoEn(new RegistroDePrueba
        {
            IdCatalogo = _idCatalogoActivo,
            NombreTipo = TipoActivo,
            DniSolicitante = DniDocente,
            FechaPrestamo = new DateTime(2026, 11, 2, 10, 0, 0),
            DniEvaluador = DniSupervisor
        });
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        Assert.AreEqual("Nombre Apellido (DNI 23456783)", TablaDePrestamos(exportador).Filas.Single()[7]);
    }

    [TestMethod]
    public void Exportar_PrestamosDeDosCatalogosUnoEliminado_CadaFilaTraeSuCatalogo()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 2, 10, 0, 0));
        GuardarPrestamo(_idCatalogoEliminado, TipoDelCatalogoEliminado, new DateTime(2026, 11, 3, 10, 0, 0));
        _datos.DarDeBajaCatalogo(_idCatalogoEliminado);
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        List<List<string>> filas = TablaDePrestamos(exportador).Filas;
        Assert.AreEqual(2, filas.Count);
        Assert.AreEqual("Audiovisual", filas[0][0]);
        Assert.AreEqual(TipoActivo, filas[0][3]);
        Assert.AreEqual("Computacion vieja", filas[1][0]);
        Assert.AreEqual(TipoDelCatalogoEliminado, filas[1][3]);
    }

    [TestMethod]
    public void Exportar_PrestamosEnLosExtremosDelRango_SoloEntranLosQueEstanDentroDelDiaCompleto()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 10, 31, 23, 59, 0));
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 1, 0, 0, 0));
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 5, 23, 59, 0));
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 6, 0, 0, 0));
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        List<string> fechas = TablaDePrestamos(exportador).Filas.Select(fila => fila[1]).ToList();
        CollectionAssert.AreEqual(new List<string> { "2026-11-01T00:00:00", "2026-11-05T23:59:00" }, fechas);
    }

    [TestMethod]
    public void Exportar_Devoluciones_LosEncabezadosSonLosAcordados()
    {
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        CollectionAssert.AreEqual(new List<string>
        {
            "Catalogo", "Fecha de registro", "Solicitante", "Tipo de equipamiento", "Unidades devueltas",
            "Registrada por"
        }, TablaDeDevoluciones(exportador).Encabezados);
    }

    [TestMethod]
    public void Exportar_UnaDevolucion_ArmaLaFilaConSusDatos()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 3, 16, 45, 30));
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        CollectionAssert.AreEqual(new List<string>
        {
            "Audiovisual", "2026-11-03T16:45:30", "Nombre Apellido (DNI 12345672)", TipoActivo,
            "EQP-0001 (Sin observaciones)", "Nombre Apellido (DNI 78901238)"
        }, TablaDeDevoluciones(exportador).Filas.Single());
    }

    [TestMethod]
    public void Exportar_DevolucionConFallas_MuestraLaCondicionDeLaUnidad()
    {
        _datos.GuardarDevolucion(new RegistroDePrueba
        {
            IdCatalogo = _idCatalogoActivo,
            NombreTipo = TipoActivo,
            DniSolicitante = DniDocente,
            FechaPrestamo = new DateTime(2026, 11, 1, 9, 0, 0),
            DniQuienRegistra = DniFuncionario,
            FechaDevolucion = new DateTime(2026, 11, 2, 9, 0, 0),
            Condicion = CondicionDevolucion.Daniada
        });
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        Assert.AreEqual("EQP-0001 (Dañada)", TablaDeDevoluciones(exportador).Filas.Single()[4]);
    }

    [TestMethod]
    public void Exportar_DevolucionesDeDosCatalogosUnoEliminado_CadaFilaTraeSuCatalogo()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 2, 10, 0, 0));
        GuardarDevolucion(_idCatalogoEliminado, TipoDelCatalogoEliminado, new DateTime(2026, 11, 3, 10, 0, 0));
        _datos.DarDeBajaCatalogo(_idCatalogoEliminado);
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        List<List<string>> filas = TablaDeDevoluciones(exportador).Filas;
        Assert.AreEqual(2, filas.Count);
        Assert.AreEqual("Audiovisual", filas[0][0]);
        Assert.AreEqual("Computacion vieja", filas[1][0]);
        Assert.AreEqual(TipoDelCatalogoEliminado, filas[1][3]);
    }

    [TestMethod]
    public void Exportar_DevolucionesEnLosExtremosDelRango_SoloEntranLasRegistradasDentroDelDiaCompleto()
    {
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 10, 31, 23, 59, 0));
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 1, 0, 0, 0));
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 5, 23, 59, 0));
        GuardarDevolucion(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 6, 0, 0, 0));
        ExportadorDePrueba exportador = new ExportadorDePrueba();
        ServicioExportacion servicio = CrearServicio(exportador);

        servicio.Exportar(CrearPedido("TXT"), DniAdministrador);

        List<string> fechas = TablaDeDevoluciones(exportador).Filas.Select(fila => fila[1]).ToList();
        CollectionAssert.AreEqual(new List<string> { "2026-11-01T00:00:00", "2026-11-05T23:59:00" }, fechas);
    }

    [TestMethod]
    public void Exportar_CsvYXlsx_TienenLasMismasFilasDePrestamosYDevoluciones()
    {
        GuardarPrestamo(_idCatalogoActivo, TipoActivo, new DateTime(2026, 11, 2, 10, 0, 0));
        GuardarDevolucion(_idCatalogoEliminado, TipoDelCatalogoEliminado, new DateTime(2026, 11, 3, 10, 0, 0));
        _datos.DarDeBajaCatalogo(_idCatalogoEliminado);
        ServicioExportacion servicio = CrearServicio(new ExportadorCsv(), new ExportadorXlsx());

        List<ArchivoExportadoDto> csv = servicio.Exportar(CrearPedido("CSV"), DniAdministrador);
        List<ArchivoExportadoDto> xlsx = servicio.Exportar(CrearPedido("XLSX"), DniAdministrador);

        CollectionAssert.AreEqual(LeerFilasDelCsv(csv[0].Contenido), LeerFilasDelXlsx(xlsx[0].Contenido));
        CollectionAssert.AreEqual(LeerFilasDelCsv(csv[1].Contenido), LeerFilasDelXlsx(xlsx[1].Contenido));
        Assert.AreEqual(2, LeerFilasDelCsv(csv[0].Contenido).Count);
        Assert.AreEqual(2, LeerFilasDelCsv(csv[1].Contenido).Count);
    }

    [TestMethod]
    public void Exportar_CsvYXlsx_NombreExtensionYTipoDeContenidoDeCadaArchivo()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorCsv(), new ExportadorXlsx());

        List<ArchivoExportadoDto> csv = servicio.Exportar(CrearPedido("CSV"), DniAdministrador);
        List<ArchivoExportadoDto> xlsx = servicio.Exportar(CrearPedido("XLSX"), DniAdministrador);

        Assert.AreEqual("prestamos_2026-11-01_2026-11-05.csv", csv[0].Nombre);
        Assert.AreEqual("devoluciones_2026-11-01_2026-11-05.csv", csv[1].Nombre);
        Assert.AreEqual("prestamos_2026-11-01_2026-11-05.xlsx", xlsx[0].Nombre);
        Assert.AreEqual("devoluciones_2026-11-01_2026-11-05.xlsx", xlsx[1].Nombre);
        Assert.AreEqual("text/csv", csv[0].TipoContenido);
        Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", xlsx[0].TipoContenido);
    }

    [TestMethod]
    public void FormatosDisponibles_ConCsvYXlsx_DevuelveLosFormatosDeLosExportadores()
    {
        ServicioExportacion servicio = CrearServicio(new ExportadorCsv(), new ExportadorXlsx());

        List<string> formatos = servicio.FormatosDisponibles();

        CollectionAssert.AreEqual(new List<string> { "CSV", "XLSX" }, formatos);
    }

    private ServicioExportacion CrearServicio(params IExportadorDeTabla[] exportadores)
    {
        return new ServicioExportacion(_datos.Prestamos, _datos.Devoluciones, _datos.Catalogos, _datos.Usuarios,
            exportadores);
    }

    private static TablaExportableDto TablaDePrestamos(ExportadorDePrueba exportador)
    {
        return exportador.TablasRecibidas[0];
    }

    private static TablaExportableDto TablaDeDevoluciones(ExportadorDePrueba exportador)
    {
        return exportador.TablasRecibidas[1];
    }

    // Los datos de la prueba no llevan comas ni comillas, asi que alcanza con partir cada linea por coma.
    private static List<string> LeerFilasDelCsv(byte[] contenido)
    {
        string texto = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetString(contenido).TrimStart('﻿');
        return texto.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(linea => linea.Replace(",", "|")).ToList();
    }

    private static List<string> LeerFilasDelXlsx(byte[] contenido)
    {
        using XLWorkbook libro = new XLWorkbook(new MemoryStream(contenido));
        IXLWorksheet hoja = libro.Worksheet(1);
        List<string> filas = new List<string>();
        foreach (IXLRow fila in hoja.RowsUsed())
        {
            filas.Add(string.Join("|", fila.Cells(1, hoja.LastColumnUsed().ColumnNumber()).Select(c => c.GetString())));
        }

        return filas;
    }

    private static PedidoExportacionDto CrearPedido(string formato)
    {
        return new PedidoExportacionDto { Rango = CrearRango(1, 5), Formato = formato };
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
