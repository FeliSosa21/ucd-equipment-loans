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
        
        validarNombre(nombre);
        Nombre = nombre;
        
        validarApellido(apellido);
        Apellido = apellido;
        
        Correo = correo;
        Contrasena = contrasena;
        Estado = EstadoUsuario.Activo;
    }

    public void validarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre no puede ser vacio");
        }
        
        if (nombre.Length < 2)
        {
            throw new DominioException("El nombre debe tener mas de 1 caracter");
        }

        if (nombre.Length > 50)
        {
            throw new DominioException("El nombre debe tener como maximo 50 caracteres");
        }
        
    }
    public void validarApellido(string apellido)
    {
        if (string.IsNullOrWhiteSpace(apellido))
        {
            throw new DominioException("El apellido no puede ser vacio");
        }

        if (apellido.Length < 2)
        {
            throw new DominioException("El apellido debe tener minimo 2 caracteres");
        }

        if (apellido.Length > 50)
        {
            throw new DominioException("El apellido debe tener maximo 50 caracteres");
        }
    }
    
    
}