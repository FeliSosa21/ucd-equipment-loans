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

    [TestMethod]
    public void Listar_SinFiltros_DevuelveTodosConSusDatos()
    {
        TipoEquipamiento proyector = new TipoEquipamiento("Proyector Epson", "Para aulas", 3, true,
            Categoria.Audiovisual);
        TipoEquipamiento notebook = new TipoEquipamiento("Notebook Lenovo", "ThinkPad T14", 5, false,
            Categoria.Computacion);
        notebook.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(null, null);

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
    public void Listar_SinCatalogo_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        ServicioTiposEquipamiento servicio = new ServicioTiposEquipamiento(new RepositorioCatalogo(memoria),
            new RepositorioUsuario(memoria));

        Assert.ThrowsException<ServicioException>(() => servicio.Listar(null, null));
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelvelosQueContienenElTexto()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar("epson", null);

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
    }

    [TestMethod]
    public void Listar_FiltroPorCategoria_DevuelveSoloEsaCategoria()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(null, "Computacion");

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Notebook Lenovo", tipos[0].Nombre);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LoAgregaActivo()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");

        servicio.Registrar(datos, DniSupervisor);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(null, null);
        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Camara Canon EOS", tipos[0].Nombre);
        Assert.AreEqual("Descripcion de prueba", tipos[0].Descripcion);
        Assert.AreEqual(4, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Audiovisual", tipos[0].Categoria);
        Assert.AreEqual("Activo", tipos[0].Estado);
    }

    [TestMethod]
    public void Registrar_ResponsablesSinPermiso_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(datos, DniFuncionario));
    }

    [TestMethod]
    public void Registrar_ResponsablesInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(datos, "11111111"));
    }

    [TestMethod]
    public void Registrar_NombreRepetido_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos("camara canon eos", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(datos, DniSupervisor));
    }

    [DataTestMethod]
    [DataRow("Muebles")]
    [DataRow("9")]
    public void Registrar_CategoriaInexistente_LanzaServicioException(string categoria)
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", categoria);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(datos, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_DatosValidos_ActualizaElTipo()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS R", "Electronica");

        servicio.Modificar("Camara Canon EOS", datos, DniSupervisor);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(null, null);
        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Camara Canon EOS R", tipos[0].Nombre);
        Assert.AreEqual("Descripcion de prueba", tipos[0].Descripcion);
        Assert.AreEqual(4, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Electronica", tipos[0].Categoria);
    }

    [TestMethod]
    public void Modificar_ResponsablesSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS R", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar("Camara Canon EOS", datos, DniFuncionario));
    }

    [TestMethod]
    public void Modificar_TipoInexistente_LanzaServicioException()
    {
        ServicioTiposEquipamiento servicio = CrearServicio();
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar("Camara Canon EOS", datos, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_NombreDeOtroTipo_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);
        DatosTipoEquipamientoDto datos = CrearDatos("notebook lenovo", "Audiovisual");

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar("Proyector Epson", datos, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_MismoNombre_ActualizaLosDemasDatos()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");

        servicio.Modificar("Camara Canon EOS", datos, DniSupervisor);

        Assert.AreEqual("Descripcion de prueba", servicio.Listar(null, null)[0].Descripcion);
    }

    [TestMethod]
    public void Modificar_DuracionCero_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);
        DatosTipoEquipamientoDto datos = CrearDatos("Camara Canon EOS", "Audiovisual");
        datos.DuracionMaxPrestamo = 0;

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar("Camara Canon EOS", datos, DniSupervisor));
    }

    [TestMethod]
    public void Discontinuar_TipoActivo_QuedaDiscontinuado()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        servicio.Discontinuar("Camara Canon EOS", DniSupervisor);

        Assert.AreEqual("Discontinuado", servicio.Listar(null, null)[0].Estado);
    }

    [TestMethod]
    public void Discontinuar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento camara = CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar("Camara Canon EOS", DniFuncionario));
    }

    [TestMethod]
    public void Discontinuar_YaDiscontinuado_LanzaServicioException()
    {
        TipoEquipamiento camara =  CrearTipo("Camara Canon EOS", Categoria.Audiovisual);
        camara.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(camara);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Discontinuar("Camara Canon EOS", DniSupervisor));
    }

    private static ServicioTiposEquipamiento CrearServicio(params TipoEquipamiento[] tipos)
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo repositorioCatalogo = new RepositorioCatalogo(memoria);
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorioCatalogo.Guardar(catalogo);

        RepositorioUsuario repositorioUsuario = new RepositorioUsuario(memoria);
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniSupervisor, "supervisor@ucd.edu.uy", TipoUsuario.Supervisor));
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniFuncionario, "funcionario@ucd.edu.uy",
            TipoUsuario.FuncionarioCE));
        return new ServicioTiposEquipamiento(repositorioCatalogo, repositorioUsuario);
    }

    private static Usuario CrearUsuario(string dni, string correo, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni, "Nombre", "Apellido", correo, "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);
        return usuario;
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

    private static TipoEquipamiento CrearTipo(string nombre, Categoria categoria)
    {
        return new TipoEquipamiento(nombre, "Descripcion", 3, false, categoria);
    }
}