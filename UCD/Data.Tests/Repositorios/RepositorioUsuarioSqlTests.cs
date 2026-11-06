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

        CollectionAssert.AreEqual(new List<string> { "Alvarez", "Fernandez", "Gomez" },
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
            dni: "45678905", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez2@ucd.edu.uy"));
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "Fernandez" });

        CollectionAssert.AreEqual(new List<string> { "12345672", "45678905" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_TextoConEspaciosAlrededor_LoEncuentra()
    {
        _repositorio.AgregarUsuario(CrearUsuario(
            dni: "47153923", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = " Fernandez " });

        CollectionAssert.AreEqual(new List<string> { "47153923" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_FiltroPorTipo_DevuelveSoloLosDeEseTipo()
    {
        TipoUsuario docente = TipoUsuario.Docente();
        Usuario lucia = CrearUsuario(
            dni: "12345672", nombre: "Lucia", apellido: "Fernandez", correo: "lucia.fernandez@ucd.edu.uy");
        lucia.AsignarTipo(docente);
        TipoUsuario estudiante = TipoUsuario.Estudiante();
        Usuario martin = CrearUsuario(
            dni: "45678905", nombre: "Martin", apellido: "Fernandez", correo: "martin.fernandez@ucd.edu.uy");
        martin.AsignarTipo(estudiante);
        _repositorio.AgregarUsuario(lucia);
        _repositorio.AgregarUsuario(martin);

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Tipo = docente });

        CollectionAssert.AreEqual(new List<string> { "12345672" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_TextoYTipo_DevuelveSoloLosQueCumplenAmbos()
    {
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
        _repositorio.AgregarUsuario(lucia);
        _repositorio.AgregarUsuario(martin);
        _repositorio.AgregarUsuario(sofia);

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(
            new FiltroUsuarios { Texto = "fernandez", Tipo = docente });

        CollectionAssert.AreEqual(
            new List<string> { "12345672" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    [DataRow("lucia")]
    [DataRow("fernandez")]
    [DataRow("4715")]
    [DataRow("ucd.edu")]
    public void ObtenerUsuarios_TextoDeUnCampo_LoEncuentra(string texto)
    {
        _repositorio.AgregarUsuario(CrearUsuario(correo: "lf2026@ucd.edu.uy"));

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = texto });

        CollectionAssert.AreEqual(new List<string> { "47153923" },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ObtenerUsuarios_NingunoCoincide_DevuelveListaVacia()
    {
        _repositorio.AgregarUsuario(CrearUsuario());

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "gomez" });

        Assert.AreEqual(0, usuarios.Count);
    }

    [TestMethod]
    public void ObtenerUsuarios_ConTipo_OtroContextoLeeSuTipo()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Docente());
        _repositorio.AgregarUsuario(usuario);

        IReadOnlyList<Usuario> usuarios = LeerConOtroContexto(new FiltroUsuarios { Texto = "fernandez" });

        Assert.AreEqual(usuario.Tipo.Id, usuarios[0].Tipo.Id);
    }

    [TestMethod]
    public void AgregarUsuario_DniRepetido_LanzaInvalidOperationException()
    {
        _repositorio.AgregarUsuario(CrearUsuario());
        Usuario repetido = CrearUsuario(correo: "otro.correo@ucd.edu.uy");

        Assert.ThrowsException<InvalidOperationException>(() => _repositorio.AgregarUsuario(repetido));
    }

    [TestMethod]
    public void AgregarUsuario_CorreoRepetidoConOtrasMayusculas_LanzaInvalidOperationException()
    {
        _repositorio.AgregarUsuario(CrearUsuario());
        Usuario repetido = CrearUsuario(dni: "12345672",
            correo: "LUCIA.FERNANDEZ@ucd.edu.uy");

        Assert.ThrowsException<InvalidOperationException>(() => _repositorio.AgregarUsuario(repetido));
    }
    
    [TestMethod]
    public void AgregarUsuario_UsuarioNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.AgregarUsuario(null!));
    }
    
    [TestMethod]
    public void ObtenerUsuarioPorCorreo_CorreoNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.ObtenerUsuarioPorCorreo(null!));
    }
    
    [TestMethod]
    public void ObtenerUsuarioPorDni_DniNulo_LanzaArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _repositorio.ObtenerUsuarioPorDni(null!));
    }
    
    [TestMethod]
    public void ActualizarUsuario_UsuarioLeidoConOtroContexto_GuardaElCambio()
    {
        _repositorio.AgregarUsuario(CrearUsuario());
        Usuario leido;
        using (SqlContext contextoQueLee = new SqlContext(_opciones))
        {
            leido = new RepositorioUsuarioSql(contextoQueLee).ObtenerUsuarioPorDni("47153923")!;
        }
        leido.Desactivar();

        using (SqlContext contextoQueActualiza = new SqlContext(_opciones))
        {
            new RepositorioUsuarioSql(contextoQueActualiza).ActualizarUsuario(leido);
        }

        using SqlContext contextoQueVerifica = new SqlContext(_opciones);
        Assert.AreEqual(EstadoUsuario.Desactivado,
            new RepositorioUsuarioSql(contextoQueVerifica).ObtenerUsuarioPorDni("47153923")?.Estado);
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
