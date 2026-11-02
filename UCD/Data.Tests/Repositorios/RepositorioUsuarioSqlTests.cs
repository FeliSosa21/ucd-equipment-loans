using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioUsuarioSqlTests
{
    private SqlContext _contexto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        DbContextOptions<SqlContext> opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(opciones);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void AgregarUsuario_UsuarioValido_QuedaGuardado()
    {
        RepositorioUsuarioSql repositorio = new RepositorioUsuarioSql(_contexto);
        Usuario usuario = CrearUsuario();

        repositorio.AgregarUsuario(usuario);

        Assert.AreEqual(1, _contexto.Usuarios.Count());
    }

    [TestMethod]
    public void ObtenerUsuariosPorDni_UsuarioAgregado_LoDevuelve()
    {
        RepositorioUsuarioSql repositorio = new RepositorioUsuarioSql(_contexto);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);
        
        Usuario? resultado = repositorio.ObtenerUsuarioPorDni(usuario.Dni);
        
        Assert.AreSame(usuario, resultado);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(dni: "47153923",
            nombre: "Lucia",
            apellido: "Fernandez",
            correo: "lucia.fernandez@ucd.edu.uy",
            contrasena: "Prueba2026");
    }
}