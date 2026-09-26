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
        
        ValidarNombreOApellido(nombre,"nombre");
        Nombre = nombre;
        
        ValidarNombreOApellido(apellido,"apellido");
        Apellido = apellido;
        
        Correo = correo;
        Contrasena = contrasena;
        Estado = EstadoUsuario.Activo;
    }

    
    private static void ValidarNombreOApellido(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DominioException($"El {campo} no puede ser vacío");
        }
        
        if (valor.Length < 2)
        {
            throw new DominioException($"El {campo} debe tener mas de 1 caracter");
        }

        if (valor.Length > 50)
        {
            throw new DominioException($"El {campo} debe tener como maximo 50 caracteres");
        }
        
    }
        
    
    
    
}