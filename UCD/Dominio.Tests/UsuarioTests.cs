namespace Dominio.Tests;

[TestClass]
public sealed class UsuarioTests
{
    private const string NombreValido = "Felipe";
    private const string ApellidoValido = "Sosa";
    private const string DniValido = "12345672";
    private const string CorreoValido = "felipeservetto@hotmail.com";
    private const string ContrasenaValida = "VivalaOrt2026";

    [TestMethod]
    public void CrearUsuario_DatosValidos_QuedaActivo()
    {
        Usuario usuario = CrearUsuario();

        Assert.AreEqual(EstadoUsuario.Activo, usuario.Estado);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_AsignaDatosCorrectamente()
    {
        Usuario usuario = CrearUsuario();

        Assert.AreEqual(NombreValido, usuario.Nombre);
        Assert.AreEqual(ApellidoValido, usuario.Apellido);
        Assert.AreEqual(DniValido, usuario.Dni);
        Assert.AreEqual(CorreoValido, usuario.Correo);
    }

    [TestMethod]
    public void CrearUsuario_Autorregistro_NoDebeCambiarContrasena()
    {
        Usuario usuario = CrearUsuario();

        Assert.IsFalse(usuario.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_NombreCon1Caracter_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: "F"));
    }

    [TestMethod]
    public void CrearUsuario_NombreConMasDe50Caracteres_LanzaExcepcion()
    {
        string nombreLargo = new string('A', 51);

        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: nombreLargo));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CrearUsuario_NombreNuloVacioOEspacios_LanzaExcepcion(string? nombre)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: nombre));
    }

    [TestMethod]
    public void CrearUsuario_NombreCon2Caracteres_SeCrea()
    {
        string nombre = "aa";

        Usuario usuario = CrearUsuario(nombre: nombre);

        Assert.AreEqual(nombre, usuario.Nombre);
    }

    [TestMethod]
    public void CrearUsuario_NombreCon50Caracteres_SeCrea()
    {
        string nombre = new string('A', 50);

        Usuario usuario = CrearUsuario(nombre: nombre);

        Assert.AreEqual(nombre, usuario.Nombre);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CrearUsuario_ApellidoNuloVacioOEspacios_LanzaExcepcion(string? apellido)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido: apellido));
    }

    [TestMethod]
    public void CrearUsuario_ApellidoCon1Caracter_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido: "S"));
    }

    [TestMethod]
    public void CrearUsuario_ApellidoConMasDe50Caracteres_LanzaExcepcion()
    {
        string apellidoLargo = new string('A', 51);

        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido: apellidoLargo));
    }

    [TestMethod]
    public void CrearUsuario_ApellidoCon2Caracteres_SeCrea()
    {
        string apellido = "aa";

        Usuario usuario = CrearUsuario(apellido: apellido);

        Assert.AreEqual(apellido, usuario.Apellido);
    }

    [TestMethod]
    public void CrearUsuario_ApellidoCon50Caracteres_SeCrea()
    {
        string apellido = new string('A', 50);

        Usuario usuario = CrearUsuario(apellido: apellido);

        Assert.AreEqual(apellido, usuario.Apellido);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CrearUsuario_CorreoNuloVacioOEspacios_LanzaExcepcion(string? correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoSinArroba_LanzaExcepcion()
    {
        string correoSinArroba = "felipehotmail.com";

        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correoSinArroba));
    }

    [TestMethod]
    public void CrearUsuario_CorreoCon2Arrobas_LanzaExcepcion()
    {
        string correoCon2Arrobas = "feli@peho@tmail.com";

        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correoCon2Arrobas));
    }

    [TestMethod]
    public void CrearUsuario_CorreoSinParteLocal_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "@hotmail.com"));
    }

    [TestMethod]
    public void CrearUsuario_CorreoConEspacios_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "fe lipe@hotmail.com"));
    }

    [TestMethod]
    [DataRow("feli..pe@hotmail.com")]
    [DataRow("feli._pe@hotmail.com")]
    [DataRow("feli.-pe@hotmail.com")]
    [DataRow("feli_.pe@hotmail.com")]
    [DataRow("feli__pe@hotmail.com")]
    [DataRow("feli_-pe@hotmail.com")]
    [DataRow("feli-.pe@hotmail.com")]
    [DataRow("feli-_pe@hotmail.com")]
    [DataRow("feli--pe@hotmail.com")]
    public void CrearUsuario_CorreoConCaracteresEspecialesConsecutivos_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoSinDominio_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "felipe@"));
    }

    [TestMethod]
    [DataRow(".feli@gmail.com")]
    [DataRow("_feli@gmail.com")]
    [DataRow("-feli@gmail.com")]
    [DataRow("felipe.@hotmail.com")]
    [DataRow("felipe_@hotmail.com")]
    [DataRow("felipe-@hotmail.com")]
    public void CrearUsuario_CorreoTerminaOComienzaEnCaracterEspecial_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoDominioSinPunto_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "felipe@hotmail"));
    }

    [TestMethod]
    [DataRow("felipe@hotmail..com")]
    [DataRow("felipe@hotmail.com.")]
    [DataRow("felipe@.hotmail.com")]
    public void CrearUsuario_CorreoConDominioMalFormado_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    [DataRow("feli#pe@hotmail.com")]
    [DataRow("feli!pe@hotmail.com")]
    [DataRow("feli+pe@hotmail.com")]
    [DataRow("feli,pe@hotmail.com")]
    [DataRow("feli(pe@hotmail.com")]
    [DataRow("feli/pe@hotmail.com")]
    [DataRow("feliñe@hotmail.com")]
    public void CrearUsuario_CorreoConCaracterInvalidoEnParteLocal_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    [DataRow("felipe@hot_mail.com")]
    [DataRow("felipe@hot#mail.com")]
    [DataRow("felipe@hot!mail.com")]
    [DataRow("felipe@hot+mail.com")]
    [DataRow("felipe@josé.com")]
    public void CrearUsuario_CorreoConCaracterInvalidoEnDominio_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    [DataRow("felipe@-hotmail.com")]
    [DataRow("felipe@hotmail-.com")]
    [DataRow("felipe@mail.-ort.com")]
    public void CrearUsuario_CorreoConGuionEnBordeDeParteDeDominio_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    [DataRow("felipe@hotmail.c")]
    [DataRow("felipe@mail.ort.edu.u")]
    public void CrearUsuario_CorreoConTLDDeMenosDe2Caracteres_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoConTLDDeMasDe63Caracteres_LanzaExcepcion()
    {
        string tld = new string('a', 64);
        string correoLargo = "felipe@hotmail." + tld;

        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correoLargo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoConTLDDe63Caracteres_SeCrea()
    {
        string tld = new string('a', 63);
        string correo = "felipe@hotmail." + tld;

        Usuario usuario = CrearUsuario(correo: correo);

        Assert.AreEqual(correo, usuario.Correo);
    }

    [TestMethod]
    [DataRow("felipe@hotmail.12")]
    [DataRow("felipe@hotmail.123")]
    public void CrearUsuario_CorreoConTLDNumerico_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoConGuionEnTLD_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "felipe@hotmail.c-m"));
    }

    [TestMethod]
    [DataRow("feli.pe@hotmail.com")]
    [DataRow("feli_pe@hotmail.com")]
    [DataRow("feli-pe@hotmail.com")]
    [DataRow("f.e_l-i@hotmail.com")]
    [DataRow("felipe@mail.ort.edu.uy")]
    [DataRow("felipe@mi-dominio.com")]
    [DataRow("felipe@dominio123.com")]
    [DataRow("felipe@a-b-c.com")]
    [DataRow("felipe@MAIL.COM")]
    [DataRow("felipe@hotmail.uy")]
    [DataRow("felipe@hotmail.c0m")]
    public void CrearUsuario_CorreoValido_SeCrea(string correo)
    {
        Usuario usuario = CrearUsuario(correo: correo);

        Assert.AreEqual(correo, usuario.Correo);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CrearUsuario_ContrasenaNulaVaciaOEspacios_LanzaExcepcion(string? contrasena)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: contrasena));
    }

    [TestMethod]
    public void CrearUsuario_ContrasenaConMenosDe8Caracteres_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "Hola444"));
    }

    [TestMethod]
    public void CrearUsuario_ContrasenaSinMayuscula_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "hola4444"));
    }

    [TestMethod]
    public void CrearUsuario_ContrasenaSinMinuscula_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "HOLA4444"));
    }

    [TestMethod]
    public void CrearUsuario_ContrasenaSinNumero_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "HOLAchau"));
    }

    [TestMethod]
    [DataRow("Abcdefg1")]
    [DataRow("Abc123!@")]
    [DataRow("VivalaOrt2026")]
    public void CrearUsuario_ContrasenaValida_SeCrea(string contrasena)
    {
        Usuario usuario = CrearUsuario(contrasena: contrasena);

        Assert.IsNotNull(usuario);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CrearUsuario_DniNuloVacioOEspacios_LanzaExcepcion(string? dni)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: dni));
    }

    [TestMethod]
    [DataRow("1234567a")]
    [DataRow("a2345672")]
    [DataRow("1234.567")]
    [DataRow("12345-72")]
    [DataRow("1234 567")]
    public void CrearUsuario_DniConCaracteresNoNumericos_LanzaExcepcion(string dni)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: dni));
    }

    [TestMethod]
    [DataRow("123456")]
    [DataRow("123456789")]
    public void CrearUsuario_DniConLargoInvalido_LanzaExcepcion(string dni)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: dni));
    }

    [TestMethod]
    [DataRow("12345678")]
    [DataRow("45678901")]
    [DataRow("30000005")]
    [DataRow("01234562")]
    [DataRow("12345601")]
    public void CrearUsuario_DniConDigitoVerificadorIncorrecto_LanzaExcepcion(string dni)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: dni));
    }

    [TestMethod]
    public void CrearUsuario_DniTodosCeros_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: "00000000"));
    }

    [TestMethod]
    [DataRow("12345672")]
    [DataRow("45678905")]
    [DataRow("30000004")]
    [DataRow("01234561")]
    [DataRow("99999999")]
    [DataRow("12345600")]
    [DataRow("10000020")]
    public void CrearUsuario_DniValido_SeCrea(string dni)
    {
        Usuario usuario = CrearUsuario(dni: dni);

        Assert.AreEqual(dni, usuario.Dni);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_TieneSinPermisos()
    {
        Usuario usuario = CrearUsuario();

        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, usuario.Tipo.Nombre);
    }

    [TestMethod]
    public void CrearUsuario_DniCon7Digitos_AgregarCeroAdelante()
    {
        Usuario usuario = CrearUsuario(dni: "1234561");

        Assert.AreEqual("01234561", usuario.Dni);
    }

    [TestMethod]
    public void ContrasenaEsCorrecta_MismaContrasena_DevuelveTrue()
    {
        Usuario usuario = CrearUsuario();

        bool resultado = usuario.ContrasenaEsCorrecta(ContrasenaValida);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    [DataRow("vivalaort2026")]
    [DataRow("VIVALAORT2026")]
    [DataRow("VivalaOrt2027")]
    [DataRow("")]
    public void ContrasenaEsCorrecta_ContrasenaDistinta_DevuelveFalse(string contrasena)
    {
        Usuario usuario = CrearUsuario();

        bool resultado = usuario.ContrasenaEsCorrecta(contrasena);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_QuedaDesactivado()
    {
        Usuario usuario = CrearUsuario();

        usuario.Desactivar();

        Assert.AreEqual(EstadoUsuario.Desactivado, usuario.Estado);
    }

    [TestMethod]
    public void ModificarDatos_DatosValidos_LosActualiza()
    {
        Usuario usuario = CrearUsuario();

        usuario.ModificarDatos("Pedro", "Suarez", "pedro@hotmail.com");

        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("Suarez", usuario.Apellido);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_CorreoInvalido_NoCambiaNingunDato()
    {
        Usuario usuario = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => usuario.ModificarDatos("Pedro", "Suarez", "pedro@hotmail"));
        Assert.AreEqual(NombreValido, usuario.Nombre);
        Assert.AreEqual(ApellidoValido, usuario.Apellido);
        Assert.AreEqual(CorreoValido, usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_NombreInvalido_NoCambiaNingunDato()
    {
        Usuario usuario = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => usuario.ModificarDatos("P", "Suarez", "pedro@hotmail.com"));
        Assert.AreEqual(NombreValido, usuario.Nombre);
        Assert.AreEqual(ApellidoValido, usuario.Apellido);
        Assert.AreEqual(CorreoValido, usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_ApellidoInvalido_NoCambiaNingunDato()
    {
        Usuario usuario = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => usuario.ModificarDatos("Pedro", "S", "pedro@hotmail.com"));
        Assert.AreEqual(NombreValido, usuario.Nombre);
        Assert.AreEqual(ApellidoValido, usuario.Apellido);
        Assert.AreEqual(CorreoValido, usuario.Correo);
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_QuedaActivo()
    {
        Usuario usuario = CrearUsuario();
        usuario.Desactivar();

        usuario.Reactivar();

        Assert.AreEqual(EstadoUsuario.Activo, usuario.Estado);
    }

    [TestMethod]
    public void Eliminar_UsuarioActivo_QuedaEliminado()
    {
        Usuario usuario = CrearUsuario();

        usuario.Eliminar();

        Assert.IsTrue(usuario.EstaEliminado);
    }

    [TestMethod]
    public void Eliminar_UsuarioActivo_QuedaDesactivado()
    {
        Usuario usuario = CrearUsuario();

        usuario.Eliminar();

        Assert.AreEqual(EstadoUsuario.Desactivado, usuario.Estado);
    }

    [TestMethod]
    public void Eliminar_UsuarioYaEliminado_SigueEliminado()
    {
        Usuario usuario = CrearUsuario();
        usuario.Eliminar();

        usuario.Eliminar();

        Assert.IsTrue(usuario.EstaEliminado);
    }

    [TestMethod]
    public void Reactivar_UsuarioEliminado_LanzaDominioExceptionConCampoEstado()
    {
        Usuario usuario = CrearUsuario();
        usuario.Eliminar();

        DominioException excepcion = Assert.ThrowsException<DominioException>(() => usuario.Reactivar());

        Assert.AreEqual(nameof(Usuario.Estado), excepcion.Campo);
        Assert.AreEqual(EstadoUsuario.Desactivado, usuario.Estado);
    }

    [TestMethod]
    public void AsignarTipo_Administrador_TieneTipoAdministrador()
    {
        Usuario usuario = CrearUsuario();

        usuario.AsignarTipo(TipoUsuario.Administrador());

        Assert.AreEqual(TipoUsuario.Administrador().Nombre, usuario.Tipo.Nombre);
    }

    [TestMethod]
    public void AsignarTipo_Nulo_LanzaExcepcion()
    {
        Usuario usuario = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => usuario.AsignarTipo(null!));
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, usuario.Tipo.Nombre);
    }

    [TestMethod]
    public void ExigirCambioDeContrasena_UsuarioNuevo_DebeCambiarContrasena()
    {
        Usuario usuario = CrearUsuario();

        usuario.ExigirCambioDeContrasena();

        Assert.IsTrue(usuario.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_NuevaValida_LaReemplazaYYaNoDebeCambiarla()
    {
        Usuario usuario = CrearUsuario();
        usuario.ExigirCambioDeContrasena();

        usuario.CambiarContrasena("NuevaClave2026");

        Assert.IsTrue(usuario.ContrasenaEsCorrecta("NuevaClave2026"));
        Assert.IsFalse(usuario.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_NuevaInvalida_LanzaExcepcion()
    {
        Usuario usuario = CrearUsuario();

        Assert.ThrowsException<DominioException>(() => usuario.CambiarContrasena("corta1A"));
    }

    [TestMethod]
    public void Constructor_DniInvalido_ElErrorIndicaElCampoDni()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() => CrearUsuario(dni: "1"));

        Assert.AreEqual("Dni", excepcion.Campo);
    }

    [TestMethod]
    public void Constructor_NombreInvalido_ElErrorIndicaElCampoNombre()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: "A"));

        Assert.AreEqual("Nombre", excepcion.Campo);
    }

    [TestMethod]
    public void Constructor_ApellidoInvalido_ElErrorIndicaElCampoApellido()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido: "A"));

        Assert.AreEqual("Apellido", excepcion.Campo);
    }

    [TestMethod]
    public void Constructor_CorreoInvalido_ElErrorIndicaElCampoCorreo()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "sinarroba"));

        Assert.AreEqual("Correo", excepcion.Campo);
    }

    [TestMethod]
    public void Constructor_ContrasenaInvalida_ElErrorIndicaElCampoContrasena()
    {
        DominioException excepcion = Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "corta"));

        Assert.AreEqual("Contrasena", excepcion.Campo);
    }

    [TestMethod]
    public void Constructor_DatosConEspaciosAlrededor_LosGuardaRecortados()
    {
        Usuario usuario = CrearUsuario(nombre: "  Felipe ", apellido: " Sosa  ", correo: " " + CorreoValido + " ");

        Assert.AreEqual(NombreValido, usuario.Nombre);
        Assert.AreEqual(ApellidoValido, usuario.Apellido);
        Assert.AreEqual(CorreoValido, usuario.Correo);
    }

    [TestMethod]
    public void Constructor_DatosValidos_NoQuedaEliminado()
    {
        Usuario usuario = CrearUsuario();

        Assert.IsFalse(usuario.EstaEliminado);
    }

    [TestMethod]
    public void ModificarDatos_DatosConEspaciosAlrededor_LosGuardaRecortados()
    {
        Usuario usuario = CrearUsuario();

        usuario.ModificarDatos("  Pedro ", " Suarez  ", " pedro@hotmail.com ");

        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("Suarez", usuario.Apellido);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void EstaPendienteDeActivacion_UsuarioRecienRegistrado_DevuelveTrue()
    {
        Usuario usuario = CrearUsuario();

        Assert.IsTrue(usuario.EstaPendienteDeActivacion());
    }

    [TestMethod]
    public void EstaPendienteDeActivacion_ConTipoAsignado_DevuelveFalse()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Estudiante());

        Assert.IsFalse(usuario.EstaPendienteDeActivacion());
    }

    [TestMethod]
    public void NombreCompleto_DatosValidos_DevuelveNombreYApellido()
    {
        Usuario usuario = CrearUsuario();

        Assert.AreEqual($"{NombreValido} {ApellidoValido}", usuario.NombreCompleto);
    }

    [TestMethod]
    public void TienePermiso_UsuarioSinPermisos_DevuelveFalse()
    {
        Usuario usuario = CrearUsuario();

        Assert.IsFalse(usuario.TienePermiso(Permiso.GestionarUsuarios));
    }

    [TestMethod]
    public void TienePermiso_PermisoDeSuTipo_DevuelveTrue()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Administrador());

        Assert.IsTrue(usuario.TienePermiso(Permiso.GestionarUsuarios));
    }

    [TestMethod]
    public void TieneTipo_OtraInstanciaConElMismoNombre_DevuelveTrue()
    {
        Usuario usuario = CrearUsuario();
        usuario.AsignarTipo(TipoUsuario.Docente());

        Assert.IsTrue(usuario.TieneTipo(TipoUsuario.Docente()));
    }

    private static Usuario CrearUsuario(string? nombre = NombreValido, string? apellido = ApellidoValido,
        string? dni = DniValido, string? correo = CorreoValido, string? contrasena = ContrasenaValida)
    {
        return new Usuario(dni!, nombre!, apellido!, correo!, contrasena!);
    }
}