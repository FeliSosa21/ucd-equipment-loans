namespace Data.Tests.Repositorios;

[TestClass]

public class RepositorioUsuarioTests
{
    [TestMethod]
    public void Agregar_UsuarioValido_SeGuarda()
    {
        Memoria memoria = new Memoria();
        
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
       
        Usuario usuario = new Usuario(dni:"12345672" , nombre: "Felipe", apellido:"Sosa",correo:"felipe@hotmail.com",contrasena:"VivaLaOrt2026");
        
        repositorio.Agregar(usuario);
        
        CollectionAssert.Contains(memoria.Usuarios,usuario);
    }
}