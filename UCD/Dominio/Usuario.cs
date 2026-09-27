namespace Dominio;

public class Usuario
{
    private const int LargoMinimoNombreOApellido = 2;
    private const int LargoMaximoNombreOApellido = 50;
    private const int LargoMinimoTld = 2;
    private const int LargoMaximoTld = 63;

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
        ValidarFormatoContrasena(contrasena);
        
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

        if (valor.Length < LargoMinimoNombreOApellido)
        {
            throw new DominioException($"El {campo} debe tener al menos {LargoMinimoNombreOApellido} caracteres");
        }

        if (valor.Length > LargoMaximoNombreOApellido)
        {
            throw new DominioException($"El {campo} debe tener como maximo {LargoMaximoNombreOApellido} caracteres");
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

        string parteLocal = partes[0];
        string dominio = partes[1];

        ValidarParteLocal(parteLocal);
        ValidarDominio(dominio);
    }

    private static void ValidarParteLocal(string parteLocal)
    {
        if (string.IsNullOrEmpty(parteLocal))
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

        foreach (char c in parteLocal)
        {
            if (!EsLetraODigitoAscii(c) && !EsCaracterEspecial(c))
            {
                throw new DominioException("La parte local del correo no puede contener caracteres invalidos");
            }
        }
    }

    private static void ValidarDominio(string dominio)
    {
        if (string.IsNullOrEmpty(dominio))
        {
            throw new DominioException("El dominio del correo no puede estar vacio");
        }

        ValidarCaracteresDominio(dominio);

        string[] partesDominio = dominio.Split('.');

        ValidarPartesDominio(partesDominio);

        string tld = partesDominio[^1];

        ValidarTld(tld);
    }

    private static void ValidarCaracteresDominio(string dominio)
    {
        foreach (char c in dominio)
        {
            if (!EsLetraODigitoAscii(c) && c != '-' && c != '.')
            {
                throw new DominioException("El dominio del correo no puede contener caracteres invalidos");
            }
        }
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

        foreach (string parte in partesDominio)
        {
            if (parte.StartsWith("-") || parte.EndsWith("-"))
            {
                throw new DominioException(
                    "Las partes del dominio del correo no pueden comenzar ni terminar con guion");
            }
        }
    }

    private static void ValidarTld(string tld)
    {
        if (tld.Length < LargoMinimoTld || tld.Length > LargoMaximoTld)
        {
            throw new DominioException(
                $"El TLD del correo debe tener entre {LargoMinimoTld} y {LargoMaximoTld} caracteres");
        }

        if (!tld.Any(char.IsLetter))
        {
            throw new DominioException("El TLD del correo debe contener al menos una letra");
        }

        foreach (char c in tld)
        {
            if (!EsLetraODigitoAscii(c))
            {
                throw new DominioException("El TLD del correo solo puede contener letras y numeros");
            }
        }
    }

    private static void ValidarFormatoContrasena(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            throw new DominioException("La contrasena no puede ser vacia");
        }

        if (contrasena.Length < 8)
        {
            throw new DominioException("La contrasena debe tener mas de  8 caracteres");
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
