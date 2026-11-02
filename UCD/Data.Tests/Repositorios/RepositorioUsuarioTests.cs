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

        Usuario? resultado = repositorio.ObtenerUsuarioPorDni(dni: usuario.Dni);

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

        Usuario? resultado = repositorio.ObtenerUsuarioPorCorreo(correo: usuario.Correo.ToUpperInvariant());

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

    [TestMethod]
    public void ObtenerUsuariosQueCoinciden_SinTexto_DevuelveLosUsuarios()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerUsuariosQueCoinciden(null, 50);

        CollectionAssert.Contains(resultado.ToList(), usuario);
    }

    [TestMethod]
    public void ObtenerUsuariosQueCoinciden_TextoDeUnUsuario_DevuelveSoloEse()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        Usuario felipe = CrearUsuario();
        Usuario otro = CrearUsuario(dni: "11111111", nombre: "Ana", apellido: "Perez", correo: "ana@hotmail.com");
        repositorio.AgregarUsuario(felipe);
        repositorio.AgregarUsuario(otro);

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerUsuariosQueCoinciden(" sosa ", 50);

        CollectionAssert.AreEqual(new List<Usuario> { felipe }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerUsuariosQueCoinciden_MasUsuariosQueElMaximo_DevuelveSoloElMaximo()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        repositorio.AgregarUsuario(CrearUsuario());
        repositorio.AgregarUsuario(CrearUsuario(dni: "11111111", nombre: "Ana", apellido: "Perez", correo: "ana@hotmail.com"));

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerUsuariosQueCoinciden(null, 1);

        Assert.AreEqual(1, resultado.Count);
    }

    private static Usuario CrearUsuario(string dni = "12345672", string nombre = "Felipe", string apellido = "Sosa",
        string correo = "felipe@hotmail.com")
    {
        return new Usuario(
            dni: dni,
            nombre: nombre,
            apellido: apellido,
            correo: correo,
            contrasena: "VivaLaOrt2026");
    }
}