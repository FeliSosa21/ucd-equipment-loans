using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioUsuarioSqlTests
{
    private SqlContext _contexto = null!;
    private RepositorioUsuarioSql _repositorio = null!;
    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
        _repositorio = new RepositorioUsuarioSql(_contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void AgregarUsuario_UsuarioValido_QuedaGuardado()
    {
        Usuario usuario = CrearUsuario();

        _repositorio.AgregarUsuario(usuario);

        Assert.AreEqual(1, _contexto.Usuarios.Count());
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_UsuarioAgregado_LoDevuelve()
    {
        Usuario usuario = CrearUsuario();
        _repositorio.AgregarUsuario(usuario);

        Usuario? resultado = _repositorio.ObtenerUsuarioPorDni(usuario.Dni);

        Assert.AreSame(usuario, resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_DniInexistente_DevuelveNull()
    {
        Usuario? resultado = _repositorio.ObtenerUsuarioPorDni("54670643");

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorCorreo_UsuarioAgregado_LoDevuelve()
    {
        Usuario usuario = CrearUsuario();
        _repositorio.AgregarUsuario(usuario);

        Usuario? resultado = _repositorio.ObtenerUsuarioPorCorreo(usuario.Correo);

        Assert.AreSame(usuario, resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorCorreo_CorreoConOtrasMayusculas_LoDevuelve()
    {
        Usuario usuario = CrearUsuario();
        _repositorio.AgregarUsuario(usuario);
        
        Usuario? resultado = _repositorio.ObtenerUsuarioPorCorreo("LUCIA.Fernandez@UCD.edu.uy");
        
        Assert.AreSame(usuario, resultado);
    }

    [TestMethod]
    public void ActualizarUsuario_UsuarioDesactivado_OtroContextoLoLeeDesactivado()
    {
        Usuario fernandez = CrearUsuario();
        _repositorio.AgregarUsuario(fernandez);
        fernandez.Desactivar();

        _repositorio.ActualizarUsuario(fernandez);

        using SqlContext otroContexto = new SqlContext(_opciones);
        Usuario? leido = new RepositorioUsuarioSql(otroContexto).ObtenerUsuarioPorDni(fernandez.Dni);
        Assert.AreEqual(EstadoUsuario.Desactivado, leido?.Estado);
    }
    
    [TestMethod]
    public void AgregarUsuario_ConTipoUsuario_GuardaElTipoUsuarioEnSuTabla()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Docente());

        _repositorio.AgregarUsuario(usuario);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Assert.AreEqual(1, contextoQueLee.Set<TipoUsuario>().Count());
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_ConTipo_OtroContextoLeeSuTipo()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Docente());
        _repositorio.AgregarUsuario(usuario);
        
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario? leido = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni(usuario.Dni);
        
        Assert.AreEqual(usuario.Tipo.Id, leido?.Tipo.Id);
    }
    
    [TestMethod]
    public void ObtenerUsuarioPorCorreo_ConTipo_OtroContextoLeeSuTipo()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Docente());
        _repositorio.AgregarUsuario(usuario);
        
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Usuario? leido = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorCorreo(usuario.Correo);
        
        Assert.AreEqual(usuario.Tipo.Id, leido?.Tipo.Id);
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.ObtenerUsuarios(null!));
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroSinCriterios_DevuelveListaVacia()
    {
        _repositorio.AgregarUsuario(CrearUsuario());
        
        IReadOnlyList<Usuario> usuarios = _repositorio.ObtenerUsuarios(new FiltroUsuarios());
        
        Assert.AreEqual(0, usuarios.Count);
    }

    [TestMethod]
    public void ObtenerUsuarios_TextoQueCoincideConVarios_DevuelveSoloEsos()
    {
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "67890127", nombre: "Sofia", apellido: "Gomez", correo: "sofia.gomez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "fernandez" });

        CollectionAssert.AreEquivalent(new List<string> { "12345672", "45678905" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_VariosCoinciden_LosDevuelveOrdenadosPorApellido()
    {
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "67890127", nombre: "Sofia", apellido: "Gomez", correo: "sofia.gomez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Alvarez", correo: "lucia.alvarez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        
        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "ucd" });
        
        CollectionAssert.AreEqual(new List<string> { "Alvarez", "Fernandez","Gomez" },
            usuarios.Select(usuario => usuario.Apellido).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_MismoApellido_LosOrdenaPorNombre()
    {
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));
        
        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "Fernandez" });
        
        CollectionAssert.AreEqual(new List<string> { "Lucia", "Martin" },
            usuarios.Select(usuario => usuario.Nombre).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_MismoNombreYApellido_LosOrdenaPorDni()
    {
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Lucia", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));
        
        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "Fernandez" });
        
        CollectionAssert.AreEqual(new List<string> { "12345672", "45678905" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    private static Usuario CrearUsuario(string dni = "47153923", string nombre = "Lucia",
        string apellido = "Fernandez", string correo = "lucia.fernandez@ucd.edu.uy")
    {
        return new Usuario(dni: dni, nombre: nombre, apellido: apellido, correo: correo, contrasena: "Prueba2026");
    }

    private IReadOnlyList<Usuario> LeerConOtroContexto(FiltroUsuarios filtro)
    {
        using SqlContext contexto = new SqlContext(_opciones);
        return new RepositorioUsuarioSql(contexto).ObtenerUsuarios(filtro);
    }
}