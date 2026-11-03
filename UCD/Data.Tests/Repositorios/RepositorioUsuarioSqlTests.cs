using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioUsuarioSqlTests
{
    private SqlContext _contexto = null!;
    private RepositorioUsuarioSql _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        DbContextOptions<SqlContext> opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(opciones);
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
    public void ObtenerPaginaDeUsuarios_SinFiltro_LosDevuelveOrdenadosPorApellido()
    {
        Usuario fernandez = CrearUsuario();
        Usuario alvarez = CrearUsuario(dni: "12345672", apellido: "Alvarez", correo: "martin.alvarez@ucd.edu.uy");
        _repositorio.AgregarUsuario(fernandez);
        _repositorio.AgregarUsuario(alvarez);
        
        IReadOnlyList<Usuario> resultado = _repositorio.ObtenerPaginaDeUsuarios(null, pagina: 1, tamanoPagina: 10);
        
        CollectionAssert.AreEqual(new List<Usuario> { alvarez, fernandez }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeUsuarios_SegundaPaginaDeUno_DevuelveSoloElSegundo()
    {
        Usuario fernandez = CrearUsuario();
        Usuario alvarez = CrearUsuario(dni: "12345672", apellido: "Alvarez", correo: "martin.alvarez@ucd.edu.uy");
        _repositorio.AgregarUsuario(fernandez);
        _repositorio.AgregarUsuario(alvarez);
        
        IReadOnlyList<Usuario> resultado = _repositorio.ObtenerPaginaDeUsuarios(null, pagina: 2, tamanoPagina: 1);
        
        CollectionAssert.AreEqual(new List<Usuario> { fernandez }, resultado.ToList());
    }

    private static Usuario CrearUsuario(string dni = "47153923", string nombre = "Lucia",
        string apellido = "Fernandez", string correo = "lucia.fernandez@ucd.edu.uy")
    {
        return new Usuario(dni: dni, nombre: nombre, apellido: apellido, correo: correo, contrasena: "Prueba2026");
    }
}