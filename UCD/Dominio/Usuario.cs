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

        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new DominioException("Correo no puede estar vacio");
        }

        if (!correo.Contains("@"))
        {
            throw new DominioException("Correo debe tener @");
        }

        string [] partes = correo.Split('@');
        if (partes.Length != 2 )
        {
            throw new DominioException("El correo no puede tener 2 arrobas");
        }
        
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