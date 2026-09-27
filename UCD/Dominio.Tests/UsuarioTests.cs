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
    public void CrearUsuario_CorreoConCaracteresConsecutivos_LanzaExcepcion(string correo)
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
    public void CrearUsuario_ContrasenaNulaVaciaOEspaciones_LanzaExcepcion(string contrasena)
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
    public void CrearUsuario_ContrasenaSinMinusccula_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(contrasena: "HOLA4444"));
    }
    

    private static Usuario CrearUsuario(string? nombre = NombreValido, string? apellido = ApellidoValido,
        string? dni = DniValido, string? correo = CorreoValido, string? contrasena = ContrasenaValida)
    {
        return new Usuario(dni!, nombre!, apellido!, correo!, contrasena!);
    }
}
