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

        ValidarCorreo(correo);
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

    private static bool EsCaracterEspecial(char c)
    {
        return c == '-' || c == '.' || c == '_';
    }
    
    private static void ValidarCorreo(string correo)
    {
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

        if (partes[0] == "")
        {
            throw new DominioException("La parte local del correo no puede estar vacio");
        }

        if (partes[1] == "")
        {
            throw new DominioException("El dominio del correo  puede estar vacio");
        }

        if (correo.Contains(" "))
        {
            throw new DominioException("El correo no puede tener espacios");
        }

        for (int i = 0; i < correo.Length; i++)
        {
            if (correo[i] == '@') break;
                
            if (EsCaracterEspecial(correo[i]) && EsCaracterEspecial(correo[i+1]))
            {
                    throw new DominioException("El correo no puede tener 2 caracteres especiales consecutivos");
            }
        }
    }
    
    
}