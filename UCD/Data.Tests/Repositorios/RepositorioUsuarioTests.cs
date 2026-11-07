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
    public void ObtenerPaginaDeUsuarios_PrimeraPagina_DevuelveLosPrimerosOrdenadosPorApellido()
    {
        RepositorioUsuario repositorio = CrearRepositorioConTresUsuarios(out Usuario ana, out Usuario bruno, out _);

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerPaginaDeUsuarios(null, pagina: 1, tamanoPagina: 2);

        CollectionAssert.AreEqual(new List<Usuario> { ana, bruno }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeUsuarios_SegundaPagina_DevuelveLosSiguientes()
    {
        RepositorioUsuario repositorio = CrearRepositorioConTresUsuarios(out _, out _, out Usuario carla);

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerPaginaDeUsuarios(null, pagina: 2, tamanoPagina: 2);

        CollectionAssert.AreEqual(new List<Usuario> { carla }, resultado.ToList());
    }

    [TestMethod]
    public void ObtenerPaginaDeUsuarios_ConFiltro_DevuelveSoloLosQueCoinciden()
    {
        RepositorioUsuario repositorio = CrearRepositorioConTresUsuarios(out _, out Usuario bruno, out _);

        IReadOnlyList<Usuario> resultado = repositorio.ObtenerPaginaDeUsuarios(" bruno ", pagina: 1, tamanoPagina: 2);

        CollectionAssert.AreEqual(new List<Usuario> { bruno }, resultado.ToList());
    }

    [TestMethod]
    public void ContarUsuarios_ConFiltro_CuentaSoloLosQueCoinciden()
    {
        RepositorioUsuario repositorio = CrearRepositorioConTresUsuarios(out _, out _, out _);

        Assert.AreEqual(3, repositorio.ContarUsuarios(null));
        Assert.AreEqual(1, repositorio.ContarUsuarios("bruno"));
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroNulo_LanzaArgumentNullException()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.ObtenerUsuarios(null!));
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroSinCriterios_DevuelveListaVacia()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario());

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios());

        Assert.AreEqual(0, usuarios.Count);
    }

    // Se agregan desordenados para comprobar que la pagina sale ordenada por apellido.
    private static RepositorioUsuario CrearRepositorioConTresUsuarios(out Usuario ana, out Usuario bruno,
        out Usuario carla)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        ana = CrearUsuario(dni: "11111111", nombre: "Ana", apellido: "Acosta", correo: "ana@hotmail.com");
        bruno = CrearUsuario(dni: "22222222", nombre: "Bruno", apellido: "Bentancur", correo: "bruno@hotmail.com");
        carla = CrearUsuario(dni: "12345672", nombre: "Carla", apellido: "Castro", correo: "carla@hotmail.com");
        repositorio.AgregarUsuario(carla);
        repositorio.AgregarUsuario(ana);
        repositorio.AgregarUsuario(bruno);
        return repositorio;
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