using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioUnidadesTests
{
    private const string DniFuncionario = "12345672";
    private const string DniDocente = "45678905";
    private const string DniFuncionarioDesactivado = "23456783";

    private ContextoDePrueba _entorno = null!;
    private int _idCatalogo;

    [TestInitialize]
    public void Inicializar()
    {
        _entorno = new ContextoDePrueba();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _entorno.Dispose();
    }

    [TestMethod]
    public void Listar_SinFiltros_DevuelveLasUnidadesDeTodosLosTipos()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001", numeroSerie: "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        UnidadFisica conObservaciones = CrearUnidad(idInstitucional: "NTB-0001");
        conObservaciones.ModificarInfo("Laboratorio 2", null, "Pantalla rayada");
        notebook.AgregarUnidad(conObservaciones);
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = ListarTodas(servicio);

        Assert.AreEqual(2, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
        Assert.AreEqual("Pantalla rayada", unidades[1].Observaciones);
    }

    [TestMethod]
    public void Listar_FiltroPorTipo_DevuelveSoloLasDeEseTipo()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar(_idCatalogo,
            new FiltroUnidadesDto { NombreTipo = "Notebook Lenovo" }, DniFuncionario);

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("NTB-0001", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Listar_FiltroPorTipoConOtrasMayusculas_DevuelveLasDeEseTipo()
    {
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        ServicioUnidades servicio = CrearServicio(notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar(_idCatalogo,
            new FiltroUnidadesDto { NombreTipo = "notebook lenovo" }, DniFuncionario);

        Assert.AreEqual(1, unidades.Count);
    }

    [TestMethod]
    public void Listar_FiltroPorEstado_DevuelveSoloLasDeEseEstado()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica enMantenimiento = CrearUnidad(idInstitucional: "PRY-0002");
        enMantenimiento.AMantenimiento();
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        proyector.AgregarUnidad(enMantenimiento);
        ServicioUnidades servicio = CrearServicio(proyector);

        List<UnidadDetalleDto> unidades = servicio.Listar(_idCatalogo,
            new FiltroUnidadesDto { Estado = "EnMantenimiento" }, DniFuncionario);

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0002", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LaAgregaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: "SN123");

        servicio.Registrar(_idCatalogo, datos, DniFuncionario);

        List<UnidadDetalleDto> unidades = ListarTodas(servicio);
        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
    }

    [TestMethod]
    public void Registrar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniDocente));
    }

    [TestMethod]
    public void Registrar_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void Registrar_TipoInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio();
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_IdRepetidoEnOtroTipo_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0001"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Notebook Lenovo", idInstitucional: "EQP-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_SerieRepetida_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001", numeroSerie: "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Notebook Lenovo", idInstitucional: "NTB-0001",
            numeroSerie: "sn123");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_DosUnidadesSinSerie_RegistraAmbas()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0002",
            numeroSerie: null);

        servicio.Registrar(_idCatalogo, datos, DniFuncionario);

        Assert.AreEqual(2, ListarTodas(servicio).Count);
    }

    [TestMethod]
    public void Registrar_TipoDiscontinuado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.Discontinuar();
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [DataTestMethod]
    [DataRow("pry-0001")]
    [DataRow("PRY0001")]
    [DataRow("PR-00001")]
    public void Registrar_IdConFormatoInvalido_LanzaServicioException(string idInstitucional)
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: idInstitucional,
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_TextosConEspacios_LosGuardaRecortados()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = new DatosUnidadDto
        {
            NombreTipo = "Proyector Epson",
            IdInstitucional = " PRY-0001 ",
            UbicacionFisica = "  Deposito A  ",
            NumeroSerie = "   "
        };

        servicio.Registrar(_idCatalogo, datos, DniFuncionario);

        UnidadDetalleDto unidad = ListarTodas(servicio)[0];
        Assert.AreEqual("PRY-0001", unidad.IdInstitucional);
        Assert.AreEqual("Deposito A", unidad.UbicacionFisica);
        Assert.IsNull(unidad.NumeroSerie);
    }

    [TestMethod]
    public void Modificar_DatosValidos_ActualizaLaUnidad()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001", numeroSerie: "SN123"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: "SN999"), DniFuncionario);

        UnidadDetalleDto unidad = ListarTodas(servicio)[0];
        Assert.AreEqual("Laboratorio 3", unidad.UbicacionFisica);
        Assert.AreEqual("SN999", unidad.NumeroSerie);
        Assert.AreEqual("Lampara nueva", unidad.Observaciones);
    }

    [TestMethod]
    public void Modificar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: null), DniDocente));
    }

    [TestMethod]
    public void Modificar_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: null),
                DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void Modificar_UnidadInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: null), DniFuncionario));
    }

    [TestMethod]
    public void Modificar_SerieDeOtraUnidad_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001", numeroSerie: "SN123"));
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0002", numeroSerie: "SN456"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0002", numeroSerie: "SN123"), DniFuncionario));
    }

    [TestMethod]
    public void Modificar_MismaSerie_ActualizaLosDemasDatos()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001", numeroSerie: "SN123"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: "SN123"), DniFuncionario);

        Assert.AreEqual("Laboratorio 3", ListarTodas(servicio)[0].UbicacionFisica);
    }

    [TestMethod]
    public void Modificar_UbicacionMuyCorta_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: null);
        datos.UbicacionFisica = "AB";

        Assert.ThrowsException<ServicioException>(() => servicio.Modificar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadDisponible_QuedaEnMantenimiento()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionario);

        Assert.AreEqual("EnMantenimiento", ListarTodas(servicio)[0].Estado);
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadDadaDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.DarDeBaja();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadPrestada_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.Prestada();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadPrestada_SiguePrestada()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.Prestada();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionario));

        Assert.AreEqual("Prestada", ListarTodas(servicio)[0].Estado);
    }

    [TestMethod]
    public void EnviarAMantenimiento_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() => servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniDocente));
    }

    [TestMethod]
    public void EnviarAMantenimiento_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void VolverADisponible_UnidadEnMantenimiento_QuedaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.AMantenimiento();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.VolverADisponible(_idCatalogo, "PRY-0001", DniFuncionario);

        Assert.AreEqual("Disponible", ListarTodas(servicio)[0].Estado);
    }

    [TestMethod]
    public void VolverADisponible_UnidadPrestada_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.Prestada();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.VolverADisponible(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void VolverADisponible_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.AMantenimiento();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() => servicio.VolverADisponible(_idCatalogo, "PRY-0001", DniDocente));
    }

    [TestMethod]
    public void VolverADisponible_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.AMantenimiento();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.VolverADisponible(_idCatalogo, "PRY-0001", DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void DarDeBaja_UnidadDisponible_QuedaDadaDeBaja()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniFuncionario);

        Assert.AreEqual("DadaDeBaja", ListarTodas(servicio)[0].Estado);
    }

    [TestMethod]
    public void DarDeBaja_UnidadPrestada_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.Prestada();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() => servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void DarDeBaja_UnidadYaDadaDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.DarDeBaja();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() => servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void DarDeBaja_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() => servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniDocente));
    }

    [TestMethod]
    public void DarDeBaja_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void Registrar_IdConEspaciosRepetidoEnOtroTipo_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0001", numeroSerie: "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Notebook Lenovo", idInstitucional: " EQP-0001 ",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_SerieConEspaciosRepetida_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0001", numeroSerie: "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Notebook Lenovo", idInstitucional: "NTB-0001",
            numeroSerie: " SN123 ");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Listar_UnidadesDeOtroCatalogo_NoAparecen()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        TipoEquipamiento proyectorDelOtroCatalogo = CrearTipo("Proyector Epson");
        proyectorDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0002"));
        GuardarOtroCatalogo("Catalogo Secundario", proyectorDelOtroCatalogo);

        List<UnidadDetalleDto> unidades = ListarTodas(servicio);

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Listar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() => ListarTodas(servicio));
    }

    [TestMethod]
    public void Registrar_CatalogoInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(_idCatalogo + 1, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        DarDeBajaElCatalogo();
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_TipoDeOtroCatalogo_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        GuardarOtroCatalogo("Catalogo Secundario", CrearTipo("Notebook Lenovo"));
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Notebook Lenovo", idInstitucional: "NTB-0001",
            numeroSerie: null);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Modificar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0001", numeroSerie: null),
                DniFuncionario));
    }

    [TestMethod]
    public void Modificar_UnidadDeOtroCatalogo_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        TipoEquipamiento proyectorDelOtroCatalogo = CrearTipo("Proyector Epson");
        proyectorDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0002"));
        GuardarOtroCatalogo("Catalogo Secundario", proyectorDelOtroCatalogo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearEdicion(idInstitucional: "PRY-0002", numeroSerie: null),
                DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_CatalogoDadoDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadDeOtroCatalogo_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        TipoEquipamiento proyectorDelOtroCatalogo = CrearTipo("Proyector Epson");
        proyectorDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0002"));
        GuardarOtroCatalogo("Catalogo Secundario", proyectorDelOtroCatalogo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento(_idCatalogo, "PRY-0002", DniFuncionario));
    }

    [TestMethod]
    public void VolverADisponible_CatalogoDadoDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad(idInstitucional: "PRY-0001");
        unidad.AMantenimiento();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.VolverADisponible(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void DarDeBaja_CatalogoDadoDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.DarDeBaja(_idCatalogo, "PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void Listar_UsuarioSinPermiso_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroUnidadesDto(), DniDocente));
    }

    [TestMethod]
    public void Listar_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroUnidadesDto(), DniFuncionarioDesactivado));
    }

    [TestMethod]
    public void Listar_DniInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroUnidadesDto(), "11111111"));
    }

    [TestMethod]
    public void Registrar_DatosValidos_SeLeeConOtroContexto()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));
        DatosUnidadDto datos = CrearDatos(nombreTipo: "Proyector Epson", idInstitucional: "PRY-0001",
            numeroSerie: "SN123");

        servicio.Registrar(_idCatalogo, datos, DniFuncionario);

        UnidadFisica? guardada = LeerUnidad("PRY-0001");
        Assert.IsNotNull(guardada);
        Assert.AreEqual("Deposito A", guardada.UbiFisica);
        Assert.AreEqual("SN123", guardada.NumSerie);
    }

    private ServicioUnidades CrearServicio(params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _idCatalogo = _entorno.GuardarCatalogo(catalogo);

        _entorno.GuardarUsuario(DniFuncionario, "funcionario@ucd.edu.uy", TipoUsuario.FuncionarioCE());
        _entorno.GuardarUsuario(DniDocente, "docente@ucd.edu.uy", TipoUsuario.Docente());
        _entorno.GuardarUsuario(DniFuncionarioDesactivado, "desactivado@ucd.edu.uy", TipoUsuario.FuncionarioCE(),
            desactivado: true);
        return new ServicioUnidades(_entorno.Catalogos, _entorno.Usuarios);
    }

    private void GuardarOtroCatalogo(string nombre, params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo(nombre);
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _entorno.GuardarCatalogo(catalogo);
    }

    private void DarDeBajaElCatalogo()
    {
        using SqlContext contexto = _entorno.NuevoContexto();
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);
        Catalogo catalogo = repositorio.ObtenerCatalogoActivoPorId(_idCatalogo)!;
        catalogo.DarDeBaja();
        repositorio.ActualizarCatalogo(catalogo);
    }

    private UnidadFisica? LeerUnidad(string idInstitucional)
    {
        using SqlContext lectura = _entorno.NuevoContexto();
        Catalogo catalogo = new RepositorioCatalogoSql(lectura).ObtenerCatalogoActivoPorId(_idCatalogo)!;
        return catalogo.Tipos.SelectMany(tipo => tipo.Unidades)
            .FirstOrDefault(unidad => unidad.IdInstitucional == idInstitucional);
    }

    private List<UnidadDetalleDto> ListarTodas(ServicioUnidades servicio)
    {
        return servicio.Listar(_idCatalogo, new FiltroUnidadesDto(), DniFuncionario);
    }

    private static TipoEquipamiento CrearTipo(string nombre)
    {
        return new TipoEquipamiento(nombre, "Descripcion", 3, false, Categoria.Audiovisual);
    }

    private static UnidadFisica CrearUnidad(string idInstitucional, string? numeroSerie = null)
    {
        return new UnidadFisica(idInstitucional, "Deposito A", numeroSerie);
    }

    private static DatosUnidadDto CrearDatos(string nombreTipo, string idInstitucional, string? numeroSerie)
    {
        return new DatosUnidadDto
        {
            NombreTipo = nombreTipo,
            IdInstitucional = idInstitucional,
            UbicacionFisica = "Deposito A",
            NumeroSerie = numeroSerie
        };
    }

    private static DatosUnidadDto CrearEdicion(string idInstitucional, string? numeroSerie)
    {
        return new DatosUnidadDto
        {
            IdInstitucional = idInstitucional,
            UbicacionFisica = "Laboratorio 3",
            NumeroSerie = numeroSerie,
            Observaciones = "Lampara nueva"
        };
    }
}