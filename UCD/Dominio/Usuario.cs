namespace Dominio;

public class Usuario
{
    public string Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Correo { get; private set; }
    private string Contrasena { get;  set; }
    public EstadoUsuario Estado { get; private set; }
    

    public Usuario(string dni, string nombre, string apellido, string correo, string contrasena)
    {
        Dni = dni;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Contrasena = contrasena;
        Estado = EstadoUsuario.Activo;
    }
    
}