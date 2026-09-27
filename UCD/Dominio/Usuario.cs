namespace Dominio;

public class Usuario
{
    public string Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Correo { get; private set; }
    private string Contrasena { get; set; }
    public EstadoUsuario Estado { get; private set; }


    public Usuario(string dni, string nombre, string apellido, string correo, string contrasena)
    {
        Dni = dni;

        ValidarNombreOApellido(nombre, "nombre");
        Nombre = nombre;

        ValidarNombreOApellido(apellido, "apellido");
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

    private static bool esAscii(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'); 
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

        string[] partes = correo.Split('@');
        var parteLocal = partes[0];
        var dominio = partes[1];
        string[] partesDominio = dominio.Split('.');
        string tld = partesDominio[^1];
        
        if (partes.Length != 2)
        {
            throw new DominioException("El correo no puede tener 2 arrobas");
        }

        if (parteLocal == "")
        {
            throw new DominioException("La parte local del correo no puede estar vacia");
        }

        if (dominio == "")
        {
            throw new DominioException("El dominio del correo no puede estar vacio");
        }

        if (correo.Contains(" "))
        {
            throw new DominioException("El correo no puede tener espacios");
        }

        for (int i = 0; i < correo.Length; i++)
        {
            if (correo[i] == '@') break;

            if (EsCaracterEspecial(correo[i]) && EsCaracterEspecial(correo[i + 1]))
            {
                throw new DominioException("El correo no puede tener 2 caracteres especiales consecutivos");
            }
        }

        if (EsCaracterEspecial(parteLocal[0]) || EsCaracterEspecial(parteLocal[^1]))

        {
            throw new DominioException(
                "La parte local del correo no puede comenzar ni terminar con un caracter especial");
        }

        if (partesDominio.Length < 2)
        {
            throw new DominioException("El dominio del correo debe contener al menos un punto");
        }

        if (partesDominio.Contains(""))
        {
            throw new DominioException("El correo no puede tener 2 puntos");
        }

        for (int i = 0; i < parteLocal.Length; i++)
        {
            if (!esAscii(parteLocal[i]) && !EsCaracterEspecial(parteLocal[i]))
            {
                throw new DominioException("La parte local del correo no puede contener caracteres invalidos");
                
            }
        }
        
        for (int i = 0; i < dominio.Length; i++)
        {
            if (!esAscii(dominio[i]) && dominio[i] != '-' && dominio[i] != '.')
            {
                throw new DominioException("La dominio del correo no puede contener caracteres invalidos");
                
            }
        }

        if (tld.Length <2 || tld.Length > 63)
        {
            throw new DominioException("El largo del TlD debe estar entre 2 a 63 caracteres");
        }
        
        
        
    }
}