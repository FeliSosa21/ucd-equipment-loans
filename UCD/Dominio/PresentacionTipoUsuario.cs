namespace Dominio;

public class PresentacionTipoUsuario
{
    public const int LargoMaximoDescripcion = 100;
    private const int KilobytesMaximosImagen = 500;
    private static readonly string[] PrefijosDeImagenAdmitidos =
    {
        "data:image/png;base64,",
        "data:image/jpeg;base64,"
    };

    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public string Imagen { get; private set; }

    public PresentacionTipoUsuario(string nombre, string descripcion, string imagen)
    {
        string descripcionRecortada = Recortar(descripcion);

        ValidarNombre(nombre);
        ValidarDescripcion(descripcionRecortada);
        ValidarImagen(imagen);

        Nombre = nombre;
        Descripcion = descripcionRecortada;
        Imagen = imagen;
    }

    private PresentacionTipoUsuario(string nombre)
    {
        Nombre = nombre;
        Descripcion = string.Empty;
        Imagen = string.Empty;
    }

    internal static PresentacionTipoUsuario SoloNombre(string nombre)
    {
        return new PresentacionTipoUsuario(nombre);
    }

    public void ModificarDescripcion(string descripcion)
    {
        string descripcionRecortada = Recortar(descripcion);

        ValidarDescripcion(descripcionRecortada);

        Descripcion = descripcionRecortada;
    }

    public void ModificarImagen(string imagen)
    {
        ValidarImagen(imagen);

        Imagen = imagen;
    }

    private static string Recortar(string texto)
    {
        return texto?.Trim() ?? string.Empty;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre no puede estar vacio");
        }
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DominioException("La descripcion no puede estar vacia");
        }

        if (descripcion.Length > LargoMaximoDescripcion)
        {
            throw new DominioException($"La descripcion debe tener como maximo {LargoMaximoDescripcion} caracteres");
        }
    }

    private static void ValidarImagen(string imagen)
    {
        ValidarFormatoDeImagen(imagen);

        int cantidadDeBytes = ContarBytesDeLaImagen(imagen);

        ValidarTamanoDeImagen(cantidadDeBytes);
    }

    private static void ValidarFormatoDeImagen(string imagen)
    {
        if (string.IsNullOrWhiteSpace(imagen))
        {
            throw new DominioException("La imagen no puede estar vacia");
        }

        if (!PrefijosDeImagenAdmitidos.Any(prefijo => imagen.StartsWith(prefijo, StringComparison.Ordinal)))
        {
            throw new DominioException("La imagen debe ser PNG o JPG");
        }
    }

    private static int ContarBytesDeLaImagen(string imagen)
    {
        string contenido = imagen.Substring(imagen.IndexOf(',') + 1);
        byte[] bytes = new byte[contenido.Length];

        if (contenido.Length == 0 || !Convert.TryFromBase64String(contenido, bytes, out int cantidadDeBytes))
        {
            throw new DominioException("La imagen no tiene un contenido base64 valido");
        }

        return cantidadDeBytes;
    }

    private static void ValidarTamanoDeImagen(int cantidadDeBytes)
    {
        if (cantidadDeBytes > KilobytesMaximosImagen * 1024)
        {
            throw new DominioException($"La imagen debe pesar como maximo {KilobytesMaximosImagen} KB");
        }
    }
}
