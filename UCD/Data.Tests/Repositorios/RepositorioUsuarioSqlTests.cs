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

    private static Usuario CrearUsuario()
    {
        return new Usuario(dni: "47153923",
            nombre: "Lucia",
            apellido: "Fernandez",
            correo: "lucia.fernandez@ucd.edu.uy",
            contrasena: "Prueba2026");
    }
}