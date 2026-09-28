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
        Usuario usuario = new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa", correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");

        repositorio.AgregarUsuario(usuario);

        CollectionAssert.Contains(memoria.Usuarios, usuario);
    }

    [TestMethod]
    public void ObtenerUsuarioPorDni_DniExistente_DevuelveElUsuario()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa", correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");
        repositorio.AgregarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorDni(dni: "12345672");

        Assert.AreEqual(usuario, resultado);
    }
}
