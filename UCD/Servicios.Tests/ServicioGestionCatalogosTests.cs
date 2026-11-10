using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioGestionCatalogosTests
{
    private const string DniSupervisor = "12345672";
    private const string NombreValido = "Laboratorio de Informatica";
    private const string CampoNombre = "Nombre";
    private const string DniDocente = "45678905";
    private const string DniDesactivado = "23456783";
    private const string DniInexistente = "34567894";

    private ContextoDePrueba _entorno = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _entorno = new ContextoDePrueba();
        _entorno.GuardarUsuario(DniSupervisor, TipoUsuario.Supervisor());
    }

    [TestCleanup]
    public void Limpiar()
    {
        _entorno.Dispose();
    }

    [TestMethod]
    public void Crear_NombreValido_SeLeeConOtroContexto()
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Crear(NombreValido, DniSupervisor);

        IReadOnlyList<Catalogo> guardados = LeerCatalogosActivos();
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(NombreValido, guardados[0].Nombre);
    }

    [TestMethod]
    public void Crear_NombreRepetido_LanzaServicioException()
    {
        ServicioGestionCatalogos servicio = CrearServicio();
        servicio.Crear(NombreValido, DniSupervisor);

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Crear(NombreValido, DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
        Assert.AreEqual(1, LeerCatalogosActivos().Count);
    }

    [DataTestMethod]
    [DataRow("laboratorio de informatica")]
    [DataRow("  Laboratorio de Informatica  ")]
    public void Crear_NombreEquivalenteConMayusculasOEspacios_LanzaServicioException(string nombre)
    {
        ServicioGestionCatalogos servicio = CrearServicio();
        servicio.Crear(NombreValido, DniSupervisor);

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Crear(nombre, DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
    }

    [TestMethod]
    public void Crear_NombreConEspacios_LoGuardaRecortado()
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Crear("  " + NombreValido + "  ", DniSupervisor);

        Assert.AreEqual(NombreValido, LeerCatalogosActivos()[0].Nombre);
    }

    [TestMethod]
    public void Crear_NombreDeUnCatalogoEliminado_LoCrea()
    {
        Catalogo eliminado = new Catalogo(NombreValido);
        eliminado.DarDeBaja();
        _entorno.GuardarCatalogo(eliminado);
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Crear(NombreValido, DniSupervisor);

        Assert.AreEqual(1, LeerCatalogosActivos().Count);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("ab")]
    [DataRow("Un nombre de catalogo demasiado largo: supera los ochenta caracteres permitidos por el sistema")]
    public void Crear_NombreFueraDeLargo_LanzaServicioExceptionEnElCampo(string nombre)
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Crear(nombre, DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
        Assert.AreEqual(0, LeerCatalogosActivos().Count);
    }

    [TestMethod]
    public void Crear_UsuarioSinPermiso_LanzaServicioException()
    {
        ServicioGestionCatalogos servicio = CrearServicio();
        _entorno.GuardarUsuario(DniDocente, TipoUsuario.Docente());

        Assert.ThrowsException<ServicioException>(() => servicio.Crear(NombreValido, DniDocente));

        Assert.AreEqual(0, LeerCatalogosActivos().Count);
    }

    [TestMethod]
    public void Crear_UsuarioDesactivado_LanzaServicioException()
    {
        ServicioGestionCatalogos servicio = CrearServicio();
        _entorno.GuardarUsuarioDesactivado(DniDesactivado, TipoUsuario.Supervisor());

        Assert.ThrowsException<ServicioException>(() => servicio.Crear(NombreValido, DniDesactivado));
    }

    [TestMethod]
    public void Crear_DniInexistente_LanzaServicioException()
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.Crear(NombreValido, DniInexistente));
    }

    [TestMethod]
    public void Crear_FallaElGuardado_LanzaServicioExceptionEnElCampo()
    {
        ServicioGestionCatalogos servicio = CrearServicioQueFallaAlGuardar();

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Crear(NombreValido, DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
    }

    [TestMethod]
    public void Modificar_NombreValido_SeLeeConOtroContexto()
    {
        int id = _entorno.GuardarCatalogo(new Catalogo("Aulas"));
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Modificar(id, NombreValido, DniSupervisor);

        IReadOnlyList<Catalogo> guardados = LeerCatalogosActivos();
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(NombreValido, guardados[0].Nombre);
    }

    [TestMethod]
    public void Modificar_NombreDeOtroCatalogo_LanzaServicioException()
    {
        _entorno.GuardarCatalogo(new Catalogo(NombreValido));
        int id = _entorno.GuardarCatalogo(new Catalogo("Aulas"));
        ServicioGestionCatalogos servicio = CrearServicio();

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(id, "  laboratorio de informatica ", DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
    }

    [DataTestMethod]
    [DataRow("Aulas")]
    [DataRow("AULAS")]
    [DataRow("  Aulas  ")]
    public void Modificar_SinCambiarElNombreOSoloSuCapitalizacion_LoPermite(string nombre)
    {
        int id = _entorno.GuardarCatalogo(new Catalogo("Aulas"));
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Modificar(id, nombre, DniSupervisor);

        Assert.AreEqual(nombre.Trim(), LeerCatalogosActivos()[0].Nombre);
    }

    [TestMethod]
    public void Modificar_CatalogoInexistente_LanzaServicioException()
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.Modificar(1, NombreValido, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_CatalogoEliminado_LanzaServicioException()
    {
        Catalogo eliminado = new Catalogo("Aulas");
        eliminado.DarDeBaja();
        int id = _entorno.GuardarCatalogo(eliminado);
        ServicioGestionCatalogos servicio = CrearServicio();

        Assert.ThrowsException<ServicioException>(() => servicio.Modificar(id, NombreValido, DniSupervisor));
    }

    [TestMethod]
    public void Modificar_FallaElGuardado_LanzaServicioException()
    {
        int id = _entorno.GuardarCatalogo(new Catalogo("Aulas"));
        ServicioGestionCatalogos servicio = CrearServicioQueFallaAlGuardar();

        Assert.ThrowsException<ServicioException>(() => servicio.Modificar(id, NombreValido, DniSupervisor));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("ab")]
    [DataRow("Un nombre de catalogo demasiado largo: supera los ochenta caracteres permitidos por el sistema")]
    public void Modificar_NombreFueraDeLargo_LanzaServicioExceptionEnElCampo(string nombre)
    {
        int id = _entorno.GuardarCatalogo(new Catalogo("Aulas"));
        ServicioGestionCatalogos servicio = CrearServicio();

        ServicioException error = Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(id, nombre, DniSupervisor));

        Assert.AreEqual(CampoNombre, error.Campo);
        Assert.AreEqual("Aulas", LeerCatalogosActivos()[0].Nombre);
    }

    private ServicioGestionCatalogos CrearServicio()
    {
        return new ServicioGestionCatalogos(_entorno.Catalogos, _entorno.Usuarios, _entorno.Prestamos);
    }

    private ServicioGestionCatalogos CrearServicioQueFallaAlGuardar()
    {
        SqlContext contexto = _entorno.NuevoContextoQueFallaAlGuardar();
        return new ServicioGestionCatalogos(new RepositorioCatalogoSql(contexto),
            new RepositorioUsuarioSql(contexto), new RepositorioPrestamoSql(contexto));
    }

    private IReadOnlyList<Catalogo> LeerCatalogosActivos()
    {
        using SqlContext lectura = _entorno.NuevoContexto();
        return new RepositorioCatalogoSql(lectura).ObtenerCatalogosActivos();
    }
}
