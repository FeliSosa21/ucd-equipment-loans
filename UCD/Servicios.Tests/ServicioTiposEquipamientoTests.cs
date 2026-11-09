using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioTiposEquipamientoTests
{
    private const string DniSupervisor = "12345672";
    private const string DniFuncionario = "45678905";
    private const string DniSupervisorDesactivado = "23456783";
    private const string DniDocente = "34567894";

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
    public void Listar_SinFiltros_DevuelveTodosConSusDatos()
    {
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector Epson", categoria: Categoria.Audiovisual,
            descripcion: "Para aulas", requiereAprobacion: true);
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook Lenovo", categoria: Categoria.Computacion,
            descripcion: "ThinkPad T14");
        notebook.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = ListarTodos(servicio);

        Assert.AreEqual(2, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
        Assert.AreEqual("Para aulas", tipos[0].Descripcion);
        Assert.AreEqual(3, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Audiovisual", tipos[0].Categoria);
        Assert.AreEqual("Activo", tipos[0].Estado);
        Assert.AreEqual("Discontinuado", tipos[1].Estado);
    }

    [TestMethod]
    public void Listar_CatalogoInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo + 1, new FiltroTiposDto(), DniSupervisor));
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelveLosQueContienenElTexto()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(_idCatalogo,
            new FiltroTiposDto { Nombre = "epson" }, DniSupervisor);

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
    }

    [TestMethod]
    public void Listar_FiltroPorCategoria_DevuelveSoloEsaCategoria()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(_idCatalogo,
            new FiltroTiposDto { Categoria = "Computacion" }, DniSupervisor);

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Notebook Lenovo", tipos[0].Nombre);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LoAgregaActivo()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        servicio.Registrar(_idCatalogo, datos, DniSupervisor);

        List<TipoEquipamientoDetalleDto> tipos = ListarTodos(servicio);
        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Camara Canon EOS", tipos[0].Nombre);
        Assert.AreEqual("Descripcion de prueba", tipos[0].Descripcion);
        Assert.AreEqual(4, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Audiovisual", tipos[0].Categoria);
        Assert.AreEqual("Activo", tipos[0].Estado);
    }

    [TestMethod]
    public void Registrar_ResponsableSinPermiso_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_ResponsableDesactivado_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniSupervisorDesactivado));
    }

    [TestMethod]
    public void Registrar_ResponsableInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, "11111111"));
    }

    [TestMethod]
    public void Registrar_NombreRepetido_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "camara canon eos", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniSupervisor));
    }

    [DataTestMethod]
    [DataRow("Muebles")]
    [DataRow("9")]
    [DataRow("1")]
    [DataRow("0")]
    public void Registrar_CategoriaInexistente_LanzaServicioException(string categoria)
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: categoria);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniSupervisor));
    }

    [TestMethod]
    public void Registrar_CategoriaEnMinusculas_LaGuarda()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "audiovisual");

        servicio.Registrar(_idCatalogo, datos, DniSupervisor);

        Assert.AreEqual("Audiovisual", ListarTodos(servicio)[0].Categoria);
    }

    [TestMethod]
    public void Modificar_DatosValidos_ActualizaElTipo()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Electronica");

        servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor);

        List<TipoEquipamientoDetalleDto> tipos = ListarTodos(servicio);
        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Camara Canon EOS R", tipos[0].Nombre);
        Assert.AreEqual("Descripcion de prueba", tipos[0].Descripcion);
        Assert.AreEqual(4, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Electronica", tipos[0].Categoria);
    }

    [TestMethod]
    public void Modificar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniFuncionario));
    }

    [TestMethod]
    public void Modificar_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisorDesactivado));
    }

    [TestMethod]
    public void Modificar_TipoInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor));
    }

    [TestMethod]
    public void Modificar_NombreDeOtroTipo_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "notebook lenovo", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Proyector Epson", datos), DniSupervisor));
    }

    [TestMethod]
    public void Modificar_MismoNombre_ActualizaLosDemasDatos()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor);

        Assert.AreEqual("Descripcion de prueba", ListarTodos(servicio)[0].Descripcion);
    }

    [TestMethod]
    public void Modificar_DuracionCero_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");
        datos.DuracionMaxPrestamo = 0;

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor));
    }

    [TestMethod]
    public void Discontinuar_TipoActivo_QuedaDiscontinuado()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniSupervisor);

        Assert.AreEqual("Discontinuado", ListarTodos(servicio)[0].Estado);
    }

    [TestMethod]
    public void Discontinuar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniFuncionario));
    }

    [TestMethod]
    public void Discontinuar_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniSupervisorDesactivado));
    }

    [TestMethod]
    public void Discontinuar_YaDiscontinuado_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniSupervisor));
    }

    [TestMethod]
    public void Reactivar_TipoDiscontinuado_QuedaActivo()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisor);

        Assert.AreEqual("Activo", ListarTodos(servicio)[0].Estado);
    }

    [TestMethod]
    public void Reactivar_TipoActivo_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisor));
    }

    [TestMethod]
    public void Reactivar_TipoInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisor));
    }

    [TestMethod]
    public void Reactivar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniFuncionario));
    }

    [TestMethod]
    public void Reactivar_ResponsableDesactivado_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisorDesactivado));
    }

    [TestMethod]
    public void Registrar_TextosConEspacios_LosGuardaRecortados()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "  Camara Canon EOS  ", categoria: "Audiovisual");
        datos.Descripcion = "  Camara reflex  ";

        servicio.Registrar(_idCatalogo, datos, DniSupervisor);

        TipoEquipamientoDetalleDto tipo = ListarTodos(servicio)[0];
        Assert.AreEqual("Camara Canon EOS", tipo.Nombre);
        Assert.AreEqual("Camara reflex", tipo.Descripcion);
    }

    [TestMethod]
    public void Modificar_NombreConEspacios_LoGuardaRecortado()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "  Camara Canon EOS R  ", categoria: "Audiovisual");

        servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor);

        Assert.AreEqual("Camara Canon EOS R", ListarTodos(servicio)[0].Nombre);
    }

    [TestMethod]
    public void Listar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() => ListarTodos(servicio));
    }

    [TestMethod]
    public void Listar_TipoDeOtroCatalogo_NoAparece()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));
        GuardarOtroCatalogo("Catalogo Secundario", CrearTipo("Proyector Epson", Categoria.Audiovisual),
            CrearTipo("Notebook Lenovo", Categoria.Computacion));

        List<TipoEquipamientoDetalleDto> tipos = ListarTodos(servicio);

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
    }

    [TestMethod]
    public void Registrar_CatalogoInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(_idCatalogo + 1, datos, DniSupervisor));
    }

    [TestMethod]
    public void Registrar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DarDeBajaElCatalogo();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniSupervisor));
    }

    [TestMethod]
    public void Registrar_MismoNombreQueUnTipoDeOtroCatalogo_LoRegistra()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        GuardarOtroCatalogo("Catalogo Secundario", CrearTipo("Camara Canon EOS", Categoria.Audiovisual));
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        servicio.Registrar(_idCatalogo, datos, DniSupervisor);

        Assert.AreEqual(1, ListarTodos(servicio).Count);
    }

    [TestMethod]
    public void Modificar_TipoDeOtroCatalogo_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        GuardarOtroCatalogo("Catalogo Secundario", CrearTipo("Camara Canon EOS", Categoria.Audiovisual));
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor));
    }

    [TestMethod]
    public void Modificar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Camara Canon EOS", Categoria.Audiovisual));
        DarDeBajaElCatalogo();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor));
    }

    [TestMethod]
    public void Discontinuar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Camara Canon EOS", Categoria.Audiovisual));
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniSupervisor));
    }

    [TestMethod]
    public void Reactivar_CatalogoDadoDeBaja_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisor));
    }

    [TestMethod]
    public void Listar_FuncionarioConGestionarUnidades_ListaLosTipos()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));

        List<TipoEquipamientoDetalleDto> tipos =
            servicio.Listar(_idCatalogo, new FiltroTiposDto(), DniFuncionario);

        Assert.AreEqual(1, tipos.Count);
    }

    [TestMethod]
    public void Listar_UsuarioSinPermiso_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));
        _entorno.GuardarUsuario(DniDocente, "docente@ucd.edu.uy", TipoUsuario.Docente());

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroTiposDto(), DniDocente));
    }

    [TestMethod]
    public void Listar_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroTiposDto(), DniSupervisorDesactivado));
    }

    [TestMethod]
    public void Listar_DniInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio(CrearTipo("Proyector Epson", Categoria.Audiovisual));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Listar(_idCatalogo, new FiltroTiposDto(), "11111111"));
    }

    [TestMethod]
    public void Registrar_DatosValidos_SeLeeConOtroContexto()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        servicio.Registrar(_idCatalogo, datos, DniSupervisor);

        TipoEquipamiento? guardado = LeerTipo("Camara Canon EOS");
        Assert.IsNotNull(guardado);
        Assert.AreEqual(Categoria.Audiovisual, guardado.Categoria);
        Assert.AreEqual(EstadoTipoEquipamiento.Activo, guardado.Estado);
    }

    [TestMethod]
    public void Modificar_DatosValidos_SeLeeConOtroContexto()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Electronica");

        servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor);

        TipoEquipamiento? guardado = LeerTipo("Camara Canon EOS R");
        Assert.IsNotNull(guardado);
        Assert.AreEqual(Categoria.Electronica, guardado.Categoria);
        Assert.IsNull(LeerTipo("Camara Canon EOS"));
    }

    [TestMethod]
    public void Discontinuar_TipoActivo_SeLeeConOtroContexto()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        servicio.Discontinuar(_idCatalogo, "Camara Canon EOS", DniSupervisor);

        Assert.AreEqual(EstadoTipoEquipamiento.Discontinuado, LeerTipo("Camara Canon EOS")!.Estado);
    }

    [TestMethod]
    public void Reactivar_TipoDiscontinuado_SeLeeConOtroContexto()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        servicio.Reactivar(_idCatalogo, "Camara Canon EOS", DniSupervisor);

        Assert.AreEqual(EstadoTipoEquipamiento.Activo, LeerTipo("Camara Canon EOS")!.Estado);
    }

    [TestMethod]
    public void Registrar_FallaElGuardado_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicioQueFallaAlGuardar();
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(_idCatalogo, datos, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_FallaElGuardado_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio =
            CrearServicioQueFallaAlGuardar(CrearTipo("Camara Canon EOS", Categoria.Audiovisual));
        DatosTipoEquipamientoDto datos = CrearDatos(nombre: "Camara Canon EOS R", categoria: "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(_idCatalogo, CrearModificacion("Camara Canon EOS", datos), DniSupervisor));
    }

    [TestMethod]
    public void Listar_FiltroNull_LanzaArgumentNullException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();

        Assert.ThrowsException<ArgumentNullException>(() => servicio.Listar(_idCatalogo, null!, DniSupervisor));
    }

    private ServicioTiposEquipamiento CrearServicio(params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _idCatalogo = _entorno.GuardarCatalogo(catalogo);

        _entorno.GuardarUsuario(DniSupervisor, "supervisor@ucd.edu.uy", TipoUsuario.Supervisor());
        _entorno.GuardarUsuario(DniFuncionario, "funcionario@ucd.edu.uy", TipoUsuario.FuncionarioCE());
        _entorno.GuardarUsuario(DniSupervisorDesactivado, "desactivado@ucd.edu.uy", TipoUsuario.Supervisor(),
            desactivado: true);
        return new ServicioTiposEquipamiento(_entorno.Catalogos, _entorno.Usuarios);
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

    private TipoEquipamiento? LeerTipo(string nombre)
    {
        using SqlContext lectura = _entorno.NuevoContexto();
        return new RepositorioCatalogoSql(lectura).ObtenerTipoPorNombre(_idCatalogo, nombre);
    }

    private ServicioTiposEquipamiento CrearServicioQueFallaAlGuardar(params TipoEquipamiento[] tipos)
    {
        CrearServicio(tipos);
        SqlContext contexto = _entorno.NuevoContextoQueFallaAlGuardar();
        return new ServicioTiposEquipamiento(new RepositorioCatalogoSql(contexto), new RepositorioUsuarioSql(contexto));
    }

    private List<TipoEquipamientoDetalleDto> ListarTodos(ServicioTiposEquipamiento servicio)
    {
        return servicio.Listar(_idCatalogo, new FiltroTiposDto(), DniSupervisor);
    }

    private static ModificacionTipoEquipamientoDto CrearModificacion(string nombreActual,
        DatosTipoEquipamientoDto datos)
    {
        return new ModificacionTipoEquipamientoDto { NombreActual = nombreActual, Datos = datos };
    }

    private static DatosTipoEquipamientoDto CrearDatos(string nombre, string categoria)
    {
        return new DatosTipoEquipamientoDto
        {
            Nombre = nombre,
            Descripcion = "Descripcion de prueba",
            DuracionMaxPrestamo = 4,
            RequiereAprobacion = true,
            Categoria = categoria
        };
    }

    private static TipoEquipamiento CrearTipo(string nombre, Categoria categoria, string descripcion = "Descripcion",
        bool requiereAprobacion = false)
    {
        return new TipoEquipamiento(nombre, descripcion, 3, requiereAprobacion, categoria);
    }
}