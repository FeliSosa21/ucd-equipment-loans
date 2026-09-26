namespace Dominio.Tests;
[TestClass]

public sealed class UsuarioTests
{
    [TestMethod]
    public void CrearUsuario_DatosValidos_QuedaActivo()
    {
        string nombre = "Felipe";
        string apellido = "Sosa";
        string dni = "12345672";
        string correo = "felipeservetto@hotmail.com";
        string contrasena = "VivalaOrt2026";

        Usuario usuario = new Usuario(dni, nombre, apellido, correo, contrasena);
        
        Assert.AreEqual(EstadoUsuario.Activo, usuario.Estado);

    }
    
}