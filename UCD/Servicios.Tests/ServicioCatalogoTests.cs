using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioCatalogoTests
{
    private const string DniDocente = "12345672";
    private const string DniSupervisor = "23456783";
    private const string DniSinPermisos = "45678905";
    private const string DniDesactivado = "34567894";
    private const string DniInexistente = "56789016";

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
    public void ConsultarTipos_FiltroVacio_DevuelveTodos()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Proyector Sony"), CrearTipo(nombre:"Proyector Microsoft"));

        var resultado = servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniDocente);

        Assert.AreEqual(2, resultado.Count);
        Assert.AreEqual("Proyector Sony", resultado[0].Nombre);
        Assert.AreEqual("Proyector Microsoft", resultado[1].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_FiltroPorCategoria_DevuelveSoloEsaCategoria()
    {
        var proyector = CrearTipo(nombre: "Proyector Sony", categoria: Categoria.Audiovisual);
        var notebook = CrearTipo(nombre: "notebook", categoria: Categoria.Computacion);
        var servicio = CrearServicio(proyector, notebook);
        var filtro = new FiltroCatalogoDto { Categoria = "Computacion" };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("notebook", resultado[0].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_FiltroPorEstado_DevuelveSoloEseEstado()
    {
        var proyector = CrearTipo(nombre: "Proyector Sony");
        var notebook = CrearTipo(nombre: "notebook");
        notebook.Discontinuar();
        var servicio = CrearServicio(proyector, notebook);
        var filtro = new FiltroCatalogoDto { Estado = "Discontinuado" };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("notebook", resultado[0].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_MinimoDisponibles_DevuelveLosQueTienenAlMenosN()
    {
        var proyector = CrearTipo(nombre: "Proyector Sony");
        var notebook = CrearTipo(nombre: "notebook");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0001"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0002"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0003"));
        var servicio = CrearServicio(proyector, notebook);
        var filtro = new FiltroCatalogoDto{ MinimoDisponibles = 2 };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("notebook", resultado[0].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_CategoriaYEstado_DevuelveLosQueCumplenAmbos()
    {
        var teclado = CrearTipo(nombre: "Teclado HP", categoria: Categoria.Computacion);
        var monitor = CrearTipo(nombre: "Monitor Dell", categoria: Categoria.Audiovisual);
        var notebook = CrearTipo(nombre: "Notebook Lenovo", categoria: Categoria.Computacion);
        monitor.Discontinuar();
        notebook.Discontinuar();
        var servicio = CrearServicio(teclado, monitor, notebook);
        var filtro = new FiltroCatalogoDto { Categoria = "Computacion", Estado = "Discontinuado" };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Notebook Lenovo", resultado[0].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_SinCoincidencias_DevuelveListaVacia()
    {
        var teclado = CrearTipo(nombre: "Teclado HP", categoria: Categoria.Computacion);
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto { Categoria = "Audiovisual" };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_MinimoCero_DevuelveTodos()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var monitor = CrearTipo(nombre: "Monitor Dell");
        monitor.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0001"));
        var servicio = CrearServicio(teclado, monitor);
        var filtro = new FiltroCatalogoDto { MinimoDisponibles = 0 };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_CatalogoSinTipos_DevuelveListaVacia()
    {
        var servicio = CrearServicio();
        var filtro = new FiltroCatalogoDto();

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_CategoriaInexistente_DevuelveListaVacia()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto { Categoria = "Inexistente" };

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_MinimoNegativo_LanzaServicioException()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto { MinimoDisponibles = -1 };

        Assert.ThrowsException<ServicioException>(() => servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente));
    }

    [TestMethod]
    public void ConsultarTipos_CatalogoInexistente_LanzaServicioException()
    {
        var servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ConsultarTipos(_idCatalogo + 1, new FiltroCatalogoDto(), DniDocente));
    }

    [TestMethod]
    public void ConsultarTipos_CatalogoDadoDeBaja_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniDocente));
    }

    [TestMethod]
    public void ConsultarTipos_TipoConElMismoNombreEnOtroCatalogo_NoAparece()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        var tipoDelOtroCatalogo = CrearTipo(nombre: "Teclado HP");
        tipoDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "EQP-0009"));
        GuardarOtroCatalogo("Catalogo Secundario", tipoDelOtroCatalogo, CrearTipo(nombre: "Proyector Sony"));

        var resultado = servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Teclado HP", resultado[0].Nombre);
        Assert.AreEqual(0, resultado[0].CantidadDisponibles);
    }

    [TestMethod]
    public void ConsultarTipos_SupervisorSinFiltro_VeActivosYDiscontinuados()
    {
        var discontinuado = CrearTipo(nombre: "Monitor LG");
        discontinuado.Discontinuar();
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"), discontinuado);
        _entorno.GuardarUsuario(DniSupervisor, "supervisor@ucd.edu.uy", TipoUsuario.Supervisor());

        var resultado = servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniSupervisor);

        Assert.AreEqual(2, resultado.Count);
        Assert.AreEqual("Activo", resultado[0].Estado);
        Assert.AreEqual("Discontinuado", resultado[1].Estado);
    }

    [TestMethod]
    public void ConsultarTipos_UsuarioSinPermiso_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        _entorno.GuardarUsuario(DniSinPermisos, "sinpermisos@ucd.edu.uy", TipoUsuario.SinPermisos());

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniSinPermisos));
    }

    [TestMethod]
    public void ConsultarTipos_UsuarioDesactivado_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        _entorno.GuardarUsuario(DniDesactivado, "desactivado@ucd.edu.uy", TipoUsuario.Docente(), desactivado: true);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniDesactivado));
    }

    [TestMethod]
    public void ConsultarTipos_DniInexistente_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniInexistente));
    }

    [TestMethod]
    public void ConsultarTipos_DocenteSinFiltro_NoVeTiposDiscontinuados()
    {
        var discontinuado = CrearTipo(nombre: "Monitor LG");
        discontinuado.Discontinuar();
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"), discontinuado);

        var resultado = servicio.ConsultarTipos(_idCatalogo, new FiltroCatalogoDto(), DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Teclado HP", resultado[0].Nombre);
        Assert.AreEqual("Activo", resultado[0].Estado);
    }

    [TestMethod]
    public void ConsultarTipos_FiltroNull_LanzaArgumentNullException()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var servicio = CrearServicio(teclado);

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ConsultarTipos(_idCatalogo, null!, DniDocente));
    }

    [TestMethod]
    public void ConsultarTipos_TipoConUnidades_DtoTraeDatosYDisponibles()
    {
        var teclado = CrearTipo(nombre: "Teclado HP", categoria: Categoria.Computacion);
        var disponible = CrearUnidad(idInstitucional: "EQP-0001");
        var prestada = CrearUnidad(idInstitucional: "EQP-0002");
        teclado.AgregarUnidad(disponible);
        teclado.AgregarUnidad(prestada);
        prestada.Prestada();
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto();

        var resultado = servicio.ConsultarTipos(_idCatalogo, filtro, DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Computacion", resultado[0].Categoria);
        Assert.AreEqual("Activo", resultado[0].Estado);
        Assert.AreEqual(1, resultado[0].CantidadDisponibles);
    }

    [TestMethod]
    public void ObtenerUnidadesDisponibles_TipoExistente_DevuelveSoloDisponibles()
    {
        var proyector = CrearTipo(nombre: "Proyector Sony");
        var teclado = CrearTipo(nombre: "Teclado HP");
        var unidadProyector = CrearUnidad(idInstitucional: "EQP-0001");
        var disponible = CrearUnidad(idInstitucional: "EQP-0002");
        var prestada = CrearUnidad(idInstitucional: "EQP-0003");
        proyector.AgregarUnidad(unidadProyector);
        teclado.AgregarUnidad(disponible);
        teclado.AgregarUnidad(prestada);
        prestada.Prestada();
        var servicio = CrearServicio(proyector, teclado);

        var resultado = servicio.ObtenerUnidadesDisponibles(_idCatalogo, "Teclado HP", DniDocente);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("EQP-0002", resultado[0].IdInstitucional);
    }

    [TestMethod]
    public void ObtenerUnidadesDisponibles_TipoInexistente_LanzaServicioException()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var servicio = CrearServicio(teclado);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerUnidadesDisponibles(_idCatalogo, "Teclado Lenovo", DniDocente));
    }

    [TestMethod]
    public void ObtenerUnidadesDisponibles_TipoDeOtroCatalogo_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        GuardarOtroCatalogo("Catalogo Secundario", CrearTipo(nombre: "Proyector Sony"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ObtenerUnidadesDisponibles(_idCatalogo, "Proyector Sony", DniDocente));
    }

    [TestMethod]
    public void ObtenerUnidadesDisponibles_CatalogoDadoDeBaja_LanzaServicioException()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Teclado HP"));
        DarDeBajaElCatalogo();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ObtenerUnidadesDisponibles(_idCatalogo, "Teclado HP", DniDocente));
    }

    private ServicioCatalogo CrearServicio(params TipoEquipamiento[] tipos)
    {
        var catalogo = new Catalogo("Catalogo Central");
        foreach (var tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _idCatalogo = _entorno.GuardarCatalogo(catalogo);
        _entorno.GuardarUsuario(DniDocente, "docente@ucd.edu.uy", TipoUsuario.Docente());
        return new ServicioCatalogo(_entorno.Catalogos, _entorno.Usuarios);
    }

    private void GuardarOtroCatalogo(string nombre, params TipoEquipamiento[] tipos)
    {
        var catalogo = new Catalogo(nombre);
        foreach (var tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _entorno.GuardarCatalogo(catalogo);
    }

    private void DarDeBajaElCatalogo()
    {
        using SqlContext contexto = _entorno.NuevoContexto();
        var repositorio = new RepositorioCatalogoSql(contexto);
        var catalogo = repositorio.ObtenerCatalogoActivoPorId(_idCatalogo)!;
        catalogo.DarDeBaja();
        repositorio.ActualizarCatalogo(catalogo);
    }

    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson EB-770Fi",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 8,
        bool requiereAprobacion = true,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }

    private static UnidadFisica CrearUnidad(
        string idInstitucional = "EQP-0001",
        string ubiFisica = "Laboratorio Prueba",
        string? numSerie = "PF3A7X21")
    {
        return new UnidadFisica(idInstitucional, ubiFisica, numSerie);
    }
}