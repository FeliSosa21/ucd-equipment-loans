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

    [TestMethod]
    public void ObtenerUsuarios_TextoQueCoincideConVarios_DevuelveSoloEsos()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "67890127", nombre: "Sofia", apellido: "Gomez", correo: "sofia.gomez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "fernandez" });

        CollectionAssert.AreEquivalent(new List<string> { "12345672", "45678905" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    [DataRow("lucia")]
    [DataRow("fernandez")]
    [DataRow("4715")]
    [DataRow("ucd.edu")]
    public void ObtenerUsuarios_TextoDeUnCampo_LoEncuentra(string texto)
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "47153923", nombre: "Lucia", apellido: "Fernandez", correo: "lf2026@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = texto });

        CollectionAssert.AreEqual(new List<string> { "47153923" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_VariosCoinciden_LosDevuelveOrdenadosPorApellido()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "67890127", nombre: "Sofia", apellido: "Gomez", correo: "sofia.gomez@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Alvarez", correo: "lucia.alvarez@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "ucd" });

        CollectionAssert.AreEqual(new List<string> { "Alvarez", "Fernandez", "Gomez" },
            usuarios.Select(usuario => usuario.Apellido).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_MismoApellido_LosOrdenaPorNombre()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "Fernandez" });

        CollectionAssert.AreEqual(new List<string> { "Lucia", "Martin" },
            usuarios.Select(usuario => usuario.Nombre).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_MismoNombreYApellido_LosOrdenaPorDni()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "45678905", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez2@ucd.edu.uy"));
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "Fernandez" });

        CollectionAssert.AreEqual(new List<string> { "12345672", "45678905" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_TextoConEspaciosAlrededor_LoEncuentra()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "47153923", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = " Fernandez " });

        CollectionAssert.AreEqual(new List<string> { "47153923" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroPorTipo_DevuelveSoloLosDeEseTipo()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        TipoUsuario docente = TipoUsuario.Docente();
        Usuario lucia = CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy");
        lucia.AsignarTipo(docente);
        TipoUsuario estudiante = TipoUsuario.Estudiante();
        Usuario martin = CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy");
        martin.AsignarTipo(estudiante);
        repositorio.AgregarUsuario(lucia);
        repositorio.AgregarUsuario(martin);

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Tipo = docente });

        CollectionAssert.AreEqual(new List<string> { "12345672" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_TextoYTipo_DevuelveSoloLosQueCumplenAmbos()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        TipoUsuario docente = TipoUsuario.Docente();
        Usuario lucia = CrearUsuario(dni: "12345672", nombre: "Lucia",
            apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy");
        lucia.AsignarTipo(docente);
        Usuario martin = CrearUsuario(dni: "45678905", nombre: "Martin",
            apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy");
        martin.AsignarTipo(TipoUsuario.Estudiante());
        Usuario sofia = CrearUsuario(dni: "67890127", nombre: "Sofia",
            apellido: "Gomez", correo: "sofia.gomez@ucd.edu.uy");
        sofia.AsignarTipo(docente);
        repositorio.AgregarUsuario(lucia);
        repositorio.AgregarUsuario(martin);
        repositorio.AgregarUsuario(sofia);

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(
            new FiltroUsuarios { Texto = "fernandez", Tipo = docente });

        CollectionAssert.AreEqual(new List<string> { "12345672" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_NingunoCoincide_DevuelveListaVacia()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        repositorio.AgregarUsuario(CrearUsuario(
            dni: "47153923", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "gomez" });

        Assert.AreEqual(0, usuarios.Count);
    }

    [TestMethod]
    public void EliminarUsuario_UsuarioAgregado_ObtenerUsuarioPorDniNoLoTrae()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);

        repositorio.EliminarUsuario(usuario);

        Assert.IsNull(repositorio.ObtenerUsuarioPorDni(usuario.Dni));
    }

    [TestMethod]
    public void EliminarUsuario_UsuarioNulo_LanzaArgumentNullException()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.EliminarUsuario(null!));
    }

    [TestMethod]
    public void EliminarUsuario_DosVeces_NoFallaYSigueEliminado()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);
        repositorio.EliminarUsuario(usuario);

        repositorio.EliminarUsuario(usuario);

        Assert.IsNull(repositorio.ObtenerUsuarioPorDni(usuario.Dni));
    }

    [TestMethod]
    public void ObtenerUsuarioPorCorreo_UsuarioEliminado_DevuelveNull()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        Usuario usuario = CrearUsuario();
        repositorio.AgregarUsuario(usuario);
        repositorio.EliminarUsuario(usuario);

        Usuario? resultado = repositorio.ObtenerUsuarioPorCorreo(usuario.Correo);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerUsuarios_UsuarioEliminado_NoLoTrae()
    {
        RepositorioUsuario repositorio = new RepositorioUsuario(new Memoria());
        Usuario lucia = CrearUsuario(dni: "12345672", nombre: "Lucia",
            apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy");
        Usuario martin = CrearUsuario(dni: "45678905", nombre: "Martin",
            apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy");
        repositorio.AgregarUsuario(lucia);
        repositorio.AgregarUsuario(martin);
        repositorio.EliminarUsuario(martin);

        IReadOnlyList<Usuario> usuarios = repositorio.ObtenerUsuarios(new FiltroUsuarios { Texto = "fernandez" });

        CollectionAssert.AreEqual(new List<string> { "12345672" },
            usuarios.Select(usuario => usuario.Dni).ToList());
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