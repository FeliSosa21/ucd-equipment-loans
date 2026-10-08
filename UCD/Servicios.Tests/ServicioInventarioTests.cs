using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;
[TestClass]
public class ServicioInventarioTests
{
    [TestMethod]
    public void ContarPorTipo_UnidadesEnCadaEstado_CuentaCadaEstado()
    {
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0002"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0003", estado: EstadoUnidadFisica.Prestada));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0004", estado: EstadoUnidadFisica.EnMantenimiento));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0005", estado: EstadoUnidadFisica.DadaDeBaja));
        ServicioInventario servicio = CrearServicio(notebook);

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(new FiltroInventarioDto());

        Assert.AreEqual(1, resultado.Count);
        CantidadesPorEstadoDto fila = resultado[0];
        Assert.AreEqual("Notebook", fila.Nombre);
        Assert.AreEqual("Computacion", fila.Categoria);
        Assert.AreEqual(2, fila.Disponibles);
        Assert.AreEqual(1, fila.Prestadas);
        Assert.AreEqual(1, fila.EnMantenimiento);
        Assert.AreEqual(1, fila.DadasDeBaja);
        Assert.AreEqual(5, fila.Total);
    }

    [TestMethod]
    public void ContarPorTipo_SinCatalogo_DevuelveListaVacia()
    {
        ServicioInventario servicio = new ServicioInventario(new RepositorioCatalogo(new Memoria()));
        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(new FiltroInventarioDto());

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ContarPorTipo_FiltroPorCategoria_DevuelveSoloEsaCategoria()
    {
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector", categoria: Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        ServicioInventario servicio = CrearServicio(proyector, notebook);
        FiltroInventarioDto filtro = new FiltroInventarioDto { Categoria = "Computacion" };

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(filtro);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Notebook", resultado[0].Nombre);
    }

    [TestMethod]
    public void ContarPorTipo_FiltroPorEstado_DeuvelveSoloLosTiposConUnidadesEnEseEstado()
    {
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        TipoEquipamiento kitArduino = CrearTipo(nombre: "Kit Arduino", categoria: Categoria.Laboratorio);
        kitArduino.AgregarUnidad(CrearUnidad(idInstitucional: "KIT-0001"));
        kitArduino.AgregarUnidad(CrearUnidad(idInstitucional: "KIT-0002", estado: EstadoUnidadFisica.EnMantenimiento));
        ServicioInventario servicio = CrearServicio(proyector, kitArduino);
        FiltroInventarioDto filtro = new FiltroInventarioDto { Estado = "EnMantenimiento" };

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(filtro);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Kit Arduino", resultado[0].Nombre);
        Assert.AreEqual(1, resultado[0].Disponibles);
    }

    [TestMethod]
    public void ContarPorCategoria_VariosTiposDeLaMismaCategoria_SumaSusUnidades()
    {
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0002", estado: EstadoUnidadFisica.Prestada));
        TipoEquipamiento tablet = CrearTipo(nombre: "Tablet", categoria: Categoria.Computacion);
        tablet.AgregarUnidad(CrearUnidad(idInstitucional: "TAB-0001", estado: EstadoUnidadFisica.DadaDeBaja));
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector", categoria: Categoria.Audiovisual);
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        ServicioInventario servicio = CrearServicio(notebook, proyector, tablet);

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorCategoria();

        Assert.AreEqual(5, resultado.Count);
        CantidadesPorEstadoDto computacion = resultado[0];
        Assert.AreEqual("Computacion", computacion.Nombre);
        Assert.AreEqual(1, computacion.Disponibles);
        Assert.AreEqual(1,computacion.Prestadas);
        Assert.AreEqual(1, computacion.DadasDeBaja);
        Assert.AreEqual(3, computacion.Total);
        Assert.AreEqual(1, resultado[1].Total);
    }

    [TestMethod]
    public void ContarPorTipo_FiltroNull_LanzaArgumentNullException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ContarPorTipo(null!));
    }

    private static ServicioInventario CrearServicio(params TipoEquipamiento[] tipos)
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo repositorio = new RepositorioCatalogo(memoria);
        Catalogo catalogo = new Catalogo("Catalogo Central");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorio.Guardar(catalogo);
        return new ServicioInventario(repositorio);
    }

    private static TipoEquipamiento CrearTipo(string nombre = "Proyector", Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre: nombre, descripcion: "Para pruebas", duracionMaxPrestamo: 5,
            requiereAprobacion: false, categoria: categoria);
    }

    // Lleva la unidad al estado pedido con las transiciones del dominio
    // (a mantenimiento solo se llega desde prestada).
    private static UnidadFisica CrearUnidad(string idInstitucional,
        EstadoUnidadFisica estado = EstadoUnidadFisica.Disponible)
    {
        UnidadFisica unidad = new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Deposito central",
            numSerie: null);
        if (estado == EstadoUnidadFisica.Prestada || estado == EstadoUnidadFisica.EnMantenimiento)
        {
            unidad.Prestada();
        }

        if (estado == EstadoUnidadFisica.EnMantenimiento)
        {
            unidad.AMantenimiento();
        }

        if (estado == EstadoUnidadFisica.DadaDeBaja)
        {
            unidad.DarDeBaja();
        }

        return unidad;
    }
}