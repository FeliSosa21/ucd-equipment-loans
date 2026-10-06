using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioCatalogoTests
{
    [TestMethod]
    public void ConsultarTipos_FiltroVacio_DevuelveTodos()
    {
        var servicio = CrearServicio(CrearTipo(nombre: "Proyector Sony"), CrearTipo(nombre:"Proyector Microsoft"));

        var resultado = servicio.ConsultarTipos(new FiltroCatalogoDto());

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

        var resultado = servicio.ConsultarTipos(filtro);

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

        var resultado = servicio.ConsultarTipos(filtro);

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

        var resultado = servicio.ConsultarTipos(filtro);

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

        var resultado = servicio.ConsultarTipos(filtro);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Notebook Lenovo", resultado[0].Nombre);
    }

    [TestMethod]
    public void ConsultarTipos_SinCoincidencias_DevuelveListaVacia()
    {
        var teclado = CrearTipo(nombre: "Teclado HP", categoria: Categoria.Computacion);
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto { Categoria = "Audiovisual" };

        var resultado = servicio.ConsultarTipos(filtro);

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

        var resultado = servicio.ConsultarTipos(filtro);

        Assert.AreEqual(2, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_CatalogoSinTipos_DevuelveListaVacia()
    {
        var servicio = CrearServicio();
        var filtro = new FiltroCatalogoDto();

        var resultado = servicio.ConsultarTipos(filtro);

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ConsultarTipos_CategoriaInexistente_DevuelveListaVacia()
    {
        var teclado = CrearTipo(nombre: "Teclado HP");
        var servicio = CrearServicio(teclado);
        var filtro = new FiltroCatalogoDto { Categoria = "Inexistente" };

        var resultado = servicio.ConsultarTipos(filtro);

        Assert.AreEqual(0, resultado.Count);
    }

    private static ServicioCatalogo CrearServicio(params TipoEquipamiento[] tipos)
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);
        var catalogo = new Catalogo("Catalogo Central");
        foreach (var tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorio.Guardar(catalogo);
        return new ServicioCatalogo(repositorio);
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