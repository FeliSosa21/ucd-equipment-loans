using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioInventarioTests
{
    private const string DniFuncionario = "12345672";
    private const string DniDocente = "45678905";
    private const string DniDesactivado = "23456783";
    private const string DniInexistente = "34567894";

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
    public void ContarPorTipo_UnidadesEnCadaEstado_CuentaCadaEstado()
    {
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0002"));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0003", estado: EstadoUnidadFisica.Prestada));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0004", estado: EstadoUnidadFisica.EnMantenimiento));
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0005", estado: EstadoUnidadFisica.DadaDeBaja));
        ServicioInventario servicio = CrearServicio(notebook);

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(_idCatalogo, new FiltroInventarioDto(), DniFuncionario);

        Assert.AreEqual(1, resultado.Count);
        CantidadesPorEstadoDto fila = resultado[0];
        Assert.AreEqual("Notebook", fila.Agrupador);
        Assert.AreEqual("Computacion", fila.Categoria);
        Assert.AreEqual(2, fila.Disponibles);
        Assert.AreEqual(1, fila.Prestadas);
        Assert.AreEqual(1, fila.EnMantenimiento);
        Assert.AreEqual(1, fila.DadasDeBaja);
        Assert.AreEqual(5, fila.Total);
    }

    [TestMethod]
    public void ContarPorTipo_CatalogoInexistente_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorTipo(_idCatalogo + 1, new FiltroInventarioDto(), DniFuncionario));
    }

    [TestMethod]
    public void ContarPorCategoria_CatalogoInexistente_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorCategoria(_idCatalogo + 1, DniFuncionario));
    }

    [TestMethod]
    public void ContarPorTipo_FiltroPorCategoria_DevuelveSoloEsaCategoria()
    {
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector", categoria: Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        ServicioInventario servicio = CrearServicio(proyector, notebook);
        FiltroInventarioDto filtro = new FiltroInventarioDto { Categoria = "Computacion" };

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(_idCatalogo, filtro, DniFuncionario);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Notebook", resultado[0].Agrupador);
    }

    [TestMethod]
    public void ContarPorTipo_FiltroPorEstado_DevuelveSoloLosTiposConUnidadesEnEseEstado()
    {
        TipoEquipamiento proyector = CrearTipo(nombre: "Proyector");
        proyector.AgregarUnidad(CrearUnidad(idInstitucional: "PRY-0001"));
        TipoEquipamiento kitArduino = CrearTipo(nombre: "Kit Arduino", categoria: Categoria.Laboratorio);
        kitArduino.AgregarUnidad(CrearUnidad(idInstitucional: "KIT-0001"));
        kitArduino.AgregarUnidad(CrearUnidad(idInstitucional: "KIT-0002", estado: EstadoUnidadFisica.EnMantenimiento));
        ServicioInventario servicio = CrearServicio(proyector, kitArduino);
        FiltroInventarioDto filtro = new FiltroInventarioDto { Estado = "EnMantenimiento" };

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorTipo(_idCatalogo, filtro, DniFuncionario);

        Assert.AreEqual(1, resultado.Count);
        Assert.AreEqual("Kit Arduino", resultado[0].Agrupador);
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

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorCategoria(_idCatalogo, DniFuncionario);

        Assert.AreEqual(5, resultado.Count);
        CantidadesPorEstadoDto computacion = BuscarFila(resultado, "Computacion");
        Assert.AreEqual(1, computacion.Disponibles);
        Assert.AreEqual(1, computacion.Prestadas);
        Assert.AreEqual(1, computacion.DadasDeBaja);
        Assert.AreEqual(3, computacion.Total);
        Assert.AreEqual(1, BuscarFila(resultado, "Audiovisual").Total);
    }

    [TestMethod]
    public void ContarPorCategoria_FilaDeCategoria_ElAgrupadorEsLaCategoria()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion));

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorCategoria(_idCatalogo, DniFuncionario);

        CantidadesPorEstadoDto computacion = BuscarFila(resultado, "Computacion");
        Assert.AreEqual("Computacion", computacion.Agrupador);
    }

    [TestMethod]
    public void ContarPorTipo_FiltroNull_LanzaArgumentNullException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ContarPorTipo(_idCatalogo, null!, DniFuncionario));
    }

    [TestMethod]
    public void ContarPorTipo_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.DarDeBajaCatalogo(_idCatalogo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorTipo(_idCatalogo, new FiltroInventarioDto(), DniFuncionario));
    }

    [TestMethod]
    public void ContarPorCategoria_CatalogoDadoDeBaja_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.DarDeBajaCatalogo(_idCatalogo);

        Assert.ThrowsException<ServicioException>(() => servicio.ContarPorCategoria(_idCatalogo, DniFuncionario));
    }

    [TestMethod]
    public void ContarPorCategoria_UnidadesDeOtroCatalogo_NoSeCuentan()
    {
        TipoEquipamiento notebook = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        notebook.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0001"));
        ServicioInventario servicio = CrearServicio(notebook);
        TipoEquipamiento notebookDelOtroCatalogo = CrearTipo(nombre: "Notebook", categoria: Categoria.Computacion);
        notebookDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0002"));
        notebookDelOtroCatalogo.AgregarUnidad(CrearUnidad(idInstitucional: "NTB-0003"));
        _entorno.GuardarCatalogo("Catalogo Secundario", notebookDelOtroCatalogo);

        List<CantidadesPorEstadoDto> resultado = servicio.ContarPorCategoria(_idCatalogo, DniFuncionario);

        Assert.AreEqual(1, BuscarFila(resultado, "Computacion").Total);
    }

    [TestMethod]
    public void ContarPorTipo_UsuarioSinPermiso_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.GuardarUsuario(DniDocente, TipoUsuario.Docente());

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorTipo(_idCatalogo, new FiltroInventarioDto(), DniDocente));
    }

    [TestMethod]
    public void ContarPorCategoria_UsuarioSinPermiso_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.GuardarUsuario(DniDocente, TipoUsuario.Docente());

        Assert.ThrowsException<ServicioException>(() => servicio.ContarPorCategoria(_idCatalogo, DniDocente));
    }

    [TestMethod]
    public void ContarPorTipo_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.GuardarUsuarioDesactivado(DniDesactivado, TipoUsuario.FuncionarioCE());

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorTipo(_idCatalogo, new FiltroInventarioDto(), DniDesactivado));
    }

    [TestMethod]
    public void ContarPorCategoria_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());
        _entorno.GuardarUsuarioDesactivado(DniDesactivado, TipoUsuario.FuncionarioCE());

        Assert.ThrowsException<ServicioException>(() => servicio.ContarPorCategoria(_idCatalogo, DniDesactivado));
    }

    [TestMethod]
    public void ContarPorTipo_DniInexistente_LanzaServicioException()
    {
        ServicioInventario servicio = CrearServicio(CrearTipo());

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ContarPorTipo(_idCatalogo, new FiltroInventarioDto(), DniInexistente));
    }

    private ServicioInventario CrearServicio(params TipoEquipamiento[] tipos)
    {
        Catalogo catalogo = new Catalogo("Catalogo Central");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        _idCatalogo = _entorno.GuardarCatalogo(catalogo);
        _entorno.GuardarUsuario(DniFuncionario, TipoUsuario.FuncionarioCE());
        return new ServicioInventario(_entorno.Catalogos, _entorno.Usuarios);
    }

    private static TipoEquipamiento CrearTipo(string nombre = "Proyector", Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre: nombre, descripcion: "Para pruebas", duracionMaxPrestamo: 5,
            requiereAprobacion: false, categoria: categoria);
    }

    // Busca la fila por la categoria y no por su posicion, que depende del orden del enum.
    private static CantidadesPorEstadoDto BuscarFila(List<CantidadesPorEstadoDto> filas, string categoria)
    {
        CantidadesPorEstadoDto? fila = filas.Find(f => f.Categoria == categoria);
        Assert.IsNotNull(fila);
        return fila;
    }

    // Lleva la unidad recien creada (disponible) al estado pedido con una transicion directa del dominio.
    private static UnidadFisica CrearUnidad(string idInstitucional,
        EstadoUnidadFisica estado = EstadoUnidadFisica.Disponible)
    {
        UnidadFisica unidad = new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Deposito central",
            numSerie: null);
        if (estado == EstadoUnidadFisica.Prestada)
        {
            unidad.Prestada();
        }
        else if (estado == EstadoUnidadFisica.EnMantenimiento)
        {
            unidad.AMantenimiento();
        }
        else if (estado == EstadoUnidadFisica.DadaDeBaja)
        {
            unidad.DarDeBaja();
        }

        return unidad;
    }
}