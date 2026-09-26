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

    private static Usuario CrearUsuario(string nombre = NombreValido, string apellido = ApellidoValido,
        string dni = DniValido, string correo = CorreoValido, string contrasena = ContrasenaValida)
    {
        return new Usuario(dni, nombre, apellido, correo, contrasena); 
        
    }
    
}