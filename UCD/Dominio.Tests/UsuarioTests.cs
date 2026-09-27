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
    public void CrearUsuario_NombreConUnCaracter_LanzaExcepcion()
    {
        
        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: "F"));
    }
    
    [TestMethod]
    public void CrearUsuario_NombreConMasDe50Caracteres_LanzaExcepcion()
    {
        string nombreExtenso = new string('A', 51);
        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: nombreExtenso));
    }
    
    [TestMethod]
    public void CrearUsuario_NombreVacioONulo_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(nombre: null));
    }

    [TestMethod]
    public void CrearUsuario_ApellidoNuloOVacio_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido: null));
    }

    [TestMethod]
    public void CrearUsuario_ApellidoConUnCaracter_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido:"S"));
    }
    [TestMethod]
    public void CrearUsuario_ApellidoCon51Caracteres_LanzaExcepcion()
    {
        string apellidoLargo = new string('A', 51);
        Assert.ThrowsException<DominioException>(() => CrearUsuario(apellido:apellidoLargo));
    }

    [TestMethod]
    public void CrearUsuario_CorreoNuloOVacio_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: null));
    }

    [TestMethod]
    public void CreaUsuario_CorreoSinArroba_LanzaExcepcion()
    {
        string correoSinArroba = "felipehotmail.com";
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo:correoSinArroba));
    }

    [TestMethod]
    public void CrearUsuario_CorreoCon2Arrobas_LanzaExcepcion()
    {
        string correoCon2Arroba = "feli@peho@tmail.com";
        Assert.ThrowsException<DominioException>(() =>CrearUsuario(correo: correoCon2Arroba));
        
    }

    [TestMethod]
    public void CrearUsuario_CorreoSinParteLocal_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>CrearUsuario(correo:"@hotmail.com"));
    }

    [TestMethod]
    public void CrearUsuario_CorreoConEspacios_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>CrearUsuario(correo:"fe lipe@hotmail.com"));
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
    public void CrearUsuario_CorreoConCaracteresConsecutivos_LanzaExcepcion(string correoEspecial)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correoEspecial));
    }

    [TestMethod]
    public void CrearUsuario_CorreoSinDominio_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "felipe@"));
    }
    [TestMethod]
    [DataRow(".feli@gmail.com")]
    [DataRow("felipe.@hotmail.com")]
    public void CrearUsuario_CorreoTerminaOComienzaEnCaracterEspecial(string correo)
    {
        Assert.ThrowsException<DominioException>(() =>CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrarUsuario_CorreoDominioSinPunt0_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: "felipe@hotmail"));
    }

    [TestMethod]
    [DataRow("felipe@hotmail..com")]
    [DataRow("felipe@hotmail.com.")]
    [DataRow("felipe@.hotmail.com")]
    public void CrearUsuario_CorreoConDosPuntos_LanzaExcepcion(string correo)
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
    [DataRow("feli.pe@hotmail.com")]
    [DataRow("feli_pe@hotmail.com")]
    [DataRow("feli-pe@hotmail.com")]
    [DataRow("f.e_l-i@hotmail.com")]
    [DataRow("felipe@mail.ort.edu.uy")]
    [DataRow("felipe@mi-dominio.com")]
    public void CrearUsuario_CorreoValidoConCaracteresEspeciales_SeCrea(string correo)
    {
        Usuario usuario = CrearUsuario(correo: correo);
        Assert.AreEqual(correo, usuario.Correo);
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
    [DataRow("felipe@dominio123.com")]
    [DataRow("felipe@a-b-c.com")]
    [DataRow("felipe@MAIL.COM")]
    public void CrearUsuario_CorreoValidoConCaracteresEspecialesEnDominio_SeCrea(string correo)
    {
        Usuario usuario = CrearUsuario(correo: correo);
        Assert.AreEqual(correo, usuario.Correo);
    }
    [DataRow("felipe@hotmail.c")]
    [DataRow("felipe@mail.ort.edu.u")]
    [TestMethod]
    public void CrearUsuario_ConTDLMuyCorto_LanzaExcepcion(string correo)
    {
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correo));
    }

    [TestMethod]
    public void CrearUsuario_ConTDLMuyCorto_SeCrea()
    {
        var correo = "felipe@hotmail.uy";
        Usuario usuario = CrearUsuario(correo:correo);
        Assert.AreEqual(correo, usuario.Correo);
    }
    
    [TestMethod]
    public void CrearUsuario_ConTLDMuyLargo_LanzaExcepcion()
    {
        string tld = new string('a', 64);
        string correoLargo = "felipe@hotmail." + tld;
        Assert.ThrowsException<DominioException>(() => CrearUsuario(correo: correoLargo));
    }
    
    


    private static Usuario CrearUsuario(string? nombre = NombreValido, string? apellido = ApellidoValido,
        string? dni = DniValido, string? correo = CorreoValido, string? contrasena = ContrasenaValida)
    {
        return new Usuario(dni!, nombre!, apellido!, correo!, contrasena!); 
    }

}
