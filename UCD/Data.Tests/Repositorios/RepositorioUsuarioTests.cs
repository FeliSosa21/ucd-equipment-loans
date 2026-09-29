using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioUsuarioTests
{
    [TestMethod]
    public void AgregarUsuario_UsuarioValido_SeGuarda()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();

        repositorio.AgregarUsuario(usuario);

        CollectionAssert.Contains(memoria.Usuarios, usuario);
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_DniExistente_DevuelveElUsuario()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorDni(dni: "12345672");

        Assert.AreEqual(usuario, resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_DniInexistente_DevuelveNulo()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorDni(dni: "45678905");

        Assert.IsNull(resultado);
    }


    [TestMethod]
    public void ObtenerUsuarioPorCorreo_CorreoConMayusculas_DevuelveElUsuario()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorCorreo(correo: "FelIpE@hotmail.COM");
        Assert.AreEqual(usuario, resultado);
    }

    [TestMethod]
    public void ObtenerUsuarioPorCorreo_CorreoInexistente_DevuelveNulo()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorCorreo(correo: "sosa@hotmail.com");

        Assert.IsNull(resultado);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa", correo: "felipe@hotmail.com",
            contrasena: "VivaLaOrt2026");
    }
}