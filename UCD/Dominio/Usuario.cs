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
        ValidarNombreOApellido(nombre, "nombre");
        ValidarNombreOApellido(apellido, "apellido");
        ValidarCorreo(correo);

        Dni = dni;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Contrasena = contrasena;
        Estado = EstadoUsuario.Activo;
    }

    private static void ValidarNombreOApellido(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DominioException($"El {campo} no puede ser vacio");
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

    private static void ValidarCorreo(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new DominioException("El correo no puede estar vacio");
        }

        if (correo.Contains(" "))
        {
            throw new DominioException("El correo no puede tener espacios");
        }

        if (!correo.Contains("@"))
        {
            throw new DominioException("El correo debe tener @");
        }

        string[] partes = correo.Split('@');

        if (partes.Length != 2)
        {
            throw new DominioException("El correo debe tener un solo @");
        }

        var parteLocal = partes[0];
        var dominio = partes[1];

        ValidarParteLocal(parteLocal);
        ValidarDominio(dominio);
    }

    private static void ValidarPartesDominio(string[] partesDominio)
    {
        if (partesDominio.Length < 2)
        {
            throw new DominioException("El dominio del correo debe contener al menos un punto");
        }

        if (partesDominio.Contains(""))
        {
            throw new DominioException(
                "El dominio del correo no puede empezar ni terminar con punto, ni tener puntos consecutivos");
        }
    }

    private static void ValidarDominio(string dominio)
    {
        if (dominio == "")
        {
            throw new DominioException("El dominio del correo no puede estar vacio");
        }

        for (int i = 0; i < dominio.Length; i++)
        {
            if (!EsLetraODigitoAscii(dominio[i]) && dominio[i] != '-' && dominio[i] != '.')
            {
                throw new DominioException("El dominio del correo no puede contener caracteres invalidos");
            }
        }

        string[] partesDominio = dominio.Split('.');
        ValidarPartesDominio(partesDominio);
        string tld = partesDominio[^1];
        

        if (tld.Length < 2 || tld.Length > 63)
        {
            throw new DominioException("El TLD del correo debe tener entre 2 y 63 caracteres");
        }

        if (!tld.Any(char.IsLetter))
        {
            throw new DominioException("El TLD del correo debe contener al menos una letra");
        }
    }

    private static void ValidarParteLocal(string parteLocal)
    {
        if (parteLocal == "")
        {
            throw new DominioException("La parte local del correo no puede estar vacia");
        }

        if (EsCaracterEspecial(parteLocal[0]) || EsCaracterEspecial(parteLocal[^1]))
        {
            throw new DominioException(
                "La parte local del correo no puede comenzar ni terminar con un caracter especial");
        }

        for (int i = 0; i < parteLocal.Length - 1; i++)
        {
            if (EsCaracterEspecial(parteLocal[i]) && EsCaracterEspecial(parteLocal[i + 1]))
            {
                throw new DominioException(
                    "La parte local del correo no puede tener caracteres especiales consecutivos");
            }
        }

        for (int i = 0; i < parteLocal.Length; i++)
        {
            if (!EsLetraODigitoAscii(parteLocal[i]) && !EsCaracterEspecial(parteLocal[i]))
            {
                throw new DominioException("La parte local del correo no puede contener caracteres invalidos");
            }
        }
    }

    private static bool EsCaracterEspecial(char c)
    {
        return c == '-' || c == '.' || c == '_';
    }

    private static bool EsLetraODigitoAscii(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
    }
}