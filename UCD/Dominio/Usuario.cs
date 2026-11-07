namespace Dominio;

public class Usuario
{
    private const int LargoMinimoNombreOApellido = 2;
    public const int LargoMaximoNombreOApellido = 50;
    private const int LargoMinimoTld = 2;
    private const int LargoMaximoTld = 63;
    private const int LargoMinimoContrasena = 8;
    public const int LargoDni = 8;
    private static readonly int[] PesosDni = { 2, 9, 8, 7, 6, 3, 4 };

    public int Id { get; private set; }
    public string Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Correo { get; private set; }
    private string Contrasena { get; set; }
    public EstadoUsuario Estado { get; private set; }
    public bool Eliminado { get; private set; }
    public bool DebeCambiarContrasena { get; private set; }
    public TipoUsuario Tipo { get; private set; }
    public string NombreCompleto => $"{Nombre} {Apellido}";

    public Usuario(string dni, string nombre, string apellido, string correo, string contrasena)
    {
        dni = NormalizarDni(dni);
        nombre = Recortar(nombre);
        apellido = Recortar(apellido);
        correo = NormalizarCorreo(correo);
        ValidarDni(dni);
        ValidarNombreOApellido(nombre, nameof(Nombre));
        ValidarNombreOApellido(apellido, nameof(Apellido));
        ValidarCorreo(correo);
        ValidarFormatoContrasena(contrasena);

        Dni = dni;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Contrasena = contrasena;
        Estado = EstadoUsuario.Activo;
        DebeCambiarContrasena = false;
        Tipo = TipoUsuario.SinPermisos();
    }

    public bool ContrasenaEsCorrecta(string contrasena)
    {
        return Contrasena == contrasena;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return Tipo.TienePermiso(permiso);
    }

    public bool EstaPendienteDeActivacion()
    {
        return Tipo.NoTienePermisos();
    }

    public void ModificarDatos(string nombre, string apellido, string correo)
    {
        nombre = Recortar(nombre);
        apellido = Recortar(apellido);
        correo = NormalizarCorreo(correo);
        ValidarNombreOApellido(nombre, nameof(Nombre));
        ValidarNombreOApellido(apellido, nameof(Apellido));
        ValidarCorreo(correo);

        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
    }

    public void Desactivar()
    {
        Estado = EstadoUsuario.Desactivado;
    }

    public void Reactivar()
    {
        Estado = EstadoUsuario.Activo;
    }

    public void Eliminar()
    {
        Eliminado = true;
    }

    public void AsignarTipo(TipoUsuario tipo)
    {
        if (tipo is null)
        {
            throw new DominioException("El tipo de usuario no puede ser nulo");
        }

        Tipo = tipo;
    }

    public void ExigirCambioDeContrasena()
    {
        DebeCambiarContrasena = true;
    }

    public void CambiarContrasena(string nueva)
    {
        ValidarFormatoContrasena(nueva);

        Contrasena = nueva;
        DebeCambiarContrasena = false;
    }

    // Para buscar un correo como lo guarda Usuario (por ejemplo, al controlar que no se repita).
    public static string NormalizarCorreo(string? correo) => Recortar(correo);

    // Los espacios de alrededor no cuentan para el largo ni se guardan.
    private static string Recortar(string? texto)
    {
        return texto?.Trim() ?? string.Empty;
    }

    private static void ValidarNombreOApellido(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DominioException(campo, $"El {campo.ToLowerInvariant()} no puede estar vacio");
        }

        if (valor.Length < LargoMinimoNombreOApellido)
        {
            throw new DominioException(campo, $"El {campo.ToLowerInvariant()} debe tener al menos {LargoMinimoNombreOApellido} caracteres");
        }

        if (valor.Length > LargoMaximoNombreOApellido)
        {
            throw new DominioException(campo, $"El {campo.ToLowerInvariant()} debe tener como maximo {LargoMaximoNombreOApellido} caracteres");
        }
    }

    private static void ValidarCorreo(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new DominioException(nameof(Correo), "El correo no puede estar vacio");
        }

        if (correo.Contains(" "))
        {
            throw new DominioException(nameof(Correo), "El correo no puede tener espacios");
        }

        if (!correo.Contains("@"))
        {
            throw new DominioException(nameof(Correo), "El correo debe tener @");
        }

        string[] partes = correo.Split('@');

        if (partes.Length != 2)
        {
            throw new DominioException(nameof(Correo), "El correo debe tener un solo @");
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
            throw new DominioException(nameof(Correo), "La parte local del correo no puede estar vacia");
        }

        if (EsCaracterEspecial(parteLocal[0]) || EsCaracterEspecial(parteLocal[^1]))
        {
            throw new DominioException(nameof(Correo),
                "La parte local del correo no puede comenzar ni terminar con un caracter especial");
        }

        for (int i = 0; i < parteLocal.Length - 1; i++)
        {
            if (EsCaracterEspecial(parteLocal[i]) && EsCaracterEspecial(parteLocal[i + 1]))
            {
                throw new DominioException(nameof(Correo),
                    "La parte local del correo no puede tener caracteres especiales consecutivos");
            }
        }

        foreach (char c in parteLocal)
        {
            if (!EsLetraODigitoAscii(c) && !EsCaracterEspecial(c))
            {
                throw new DominioException(nameof(Correo), "La parte local del correo no puede contener caracteres invalidos");
            }
        }
    }

    private static void ValidarDominio(string dominio)
    {
        if (string.IsNullOrEmpty(dominio))
        {
            throw new DominioException(nameof(Correo), "El dominio del correo no puede estar vacio");
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
                throw new DominioException(nameof(Correo), "El dominio del correo no puede contener caracteres invalidos");
            }
        }
    }

    private static void ValidarPartesDominio(string[] partesDominio)
    {
        if (partesDominio.Length < 2)
        {
            throw new DominioException(nameof(Correo), "El dominio del correo debe contener al menos un punto");
        }

        if (partesDominio.Contains(""))
        {
            throw new DominioException(nameof(Correo),
                "El dominio del correo no puede empezar ni terminar con punto, ni tener puntos consecutivos");
        }

        foreach (string parte in partesDominio)
        {
            if (parte.StartsWith("-") || parte.EndsWith("-"))
            {
                throw new DominioException(nameof(Correo),
                    "Las partes del dominio del correo no pueden comenzar ni terminar con guion");
            }
        }
    }

    private static void ValidarTld(string tld)
    {
        if (tld.Length < LargoMinimoTld || tld.Length > LargoMaximoTld)
        {
            throw new DominioException(nameof(Correo),
                $"El TLD del correo debe tener entre {LargoMinimoTld} y {LargoMaximoTld} caracteres");
        }

        if (!tld.Any(char.IsLetter))
        {
            throw new DominioException(nameof(Correo), "El TLD del correo debe contener al menos una letra");
        }

        foreach (char c in tld)
        {
            if (!EsLetraODigitoAscii(c))
            {
                throw new DominioException(nameof(Correo), "El TLD del correo solo puede contener letras y numeros");
            }
        }
    }

    private static void ValidarFormatoContrasena(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            throw new DominioException(nameof(Contrasena), "La contrasena no puede estar vacia");
        }

        if (contrasena.Length < LargoMinimoContrasena)
        {
            throw new DominioException(nameof(Contrasena), $"La contrasena debe tener al menos {LargoMinimoContrasena} caracteres");
        }

        if (!contrasena.Any(char.IsUpper))
        {
            throw new DominioException(nameof(Contrasena), "La contrasena debe tener al menos una mayuscula");
        }

        if (!contrasena.Any(char.IsLower))
        {
            throw new DominioException(nameof(Contrasena), "La contrasena debe tener al menos una minuscula");
        }

        if (!contrasena.Any(char.IsDigit))
        {
            throw new DominioException(nameof(Contrasena), "La contrasena debe tener al menos un numero");
        }
    }

    private static void ValidarDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            throw new DominioException(nameof(Dni), "El DNI no puede estar vacio");
        }

        foreach (char c in dni)
        {
            if (!char.IsAsciiDigit(c))
            {
                throw new DominioException(nameof(Dni), "El DNI solo puede contener numeros");
            }
        }

        if (dni.Length != LargoDni)
        {
            throw new DominioException(nameof(Dni), $"El DNI debe tener {LargoDni} digitos");
        }

        string todosCeros = new string('0', LargoDni);

        if (dni == todosCeros)
        {
            throw new DominioException(nameof(Dni), "El DNI no puede ser todo ceros");
        }

        if (!TieneDigitoVerificadorValido(dni))
        {
            throw new DominioException(nameof(Dni), "El digito verificador del DNI es incorrecto");
        }
    }

    private static bool TieneDigitoVerificadorValido(string dni)
    {
        int suma = 0;
        for (int i = 0; i < PesosDni.Length; i++)
        {
            suma += ValorDigito(dni[i]) * PesosDni[i];
        }

        int verificadorCalculado = (10 - suma % 10) % 10;
        int verificadorRecibido = ValorDigito(dni[^1]);

        return verificadorCalculado == verificadorRecibido;
    }

    private static string NormalizarDni(string dni)
    {
        if (!string.IsNullOrEmpty(dni) && dni.Length == LargoDni - 1)
        {
            return "0" + dni;
        }

        return dni;
    }

    private static int ValorDigito(char c)
    {
        return c - '0';
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