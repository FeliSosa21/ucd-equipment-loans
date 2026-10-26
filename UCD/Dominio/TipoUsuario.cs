namespace Dominio;

public class TipoUsuario
{
    private const int LargoMaximoDescripcion = 100;
    private const int KilobytesMaximosImagen = 500;
    private static readonly string[] PrefijosDeImagenAdmitidos = { "data:image/png;base64,", "data:image/jpeg;base64," };

    private readonly HashSet<Permiso> permisos;

    public string Nombre { get; }
    public string Descripcion { get; private set; } = string.Empty;
    public string Imagen { get; private set; } = string.Empty;
    public int? MaxUnidadesPorCatalogo { get; private set; }
    public int? MaxPrestamosConcurrentes { get; private set; }

    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(
        "Sin permisos",
        maxPrestamosConcurrentes: 0,
        maxUnidadesPorCatalogo: 0,
        new HashSet<Permiso>());

    public static TipoUsuario Estudiante { get; } = new TipoUsuario(
        "Estudiante",
        maxPrestamosConcurrentes: 2,
        maxUnidadesPorCatalogo: 2,
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarDisponibilidad,
            Permiso.SolicitarPrestamo,
            Permiso.ConsultarMisSolicitudes
        });

    public static TipoUsuario Docente { get; } = new TipoUsuario(
        "Docente",
        maxPrestamosConcurrentes: 3,
        maxUnidadesPorCatalogo: 40,
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarDisponibilidad,
            Permiso.SolicitarPrestamo,
            Permiso.ConsultarMisSolicitudes,
            Permiso.ConsultarHistorico
        });

    public static TipoUsuario FuncionarioCE { get; } = new TipoUsuario(
        "Funcionario del centro de equipamiento",
        maxPrestamosConcurrentes: 0,
        maxUnidadesPorCatalogo: 0,
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.GestionarUnidades,
            Permiso.RealizarPrestamo,
            Permiso.RegistrarDevolucion
        });

    public static TipoUsuario Supervisor { get; } = new TipoUsuario(
        "Supervisor",
        maxPrestamosConcurrentes: 0,
        maxUnidadesPorCatalogo: 0,
        new HashSet<Permiso>
        {
            Permiso.ConsultarCatalogo,
            Permiso.ConsultarHistorico,
            Permiso.GestionarCatalogo,
            Permiso.GestionarUnidades,
            Permiso.EvaluarSolicitud,
            Permiso.RegistrarDevolucion,
            Permiso.ConsultarPrestamos
        });

    public static TipoUsuario Administrador { get; } = new TipoUsuario(
        "Administrador",
        maxPrestamosConcurrentes: null,
        maxUnidadesPorCatalogo: null,
        new HashSet<Permiso>(Enum.GetValues<Permiso>()));

    public static IReadOnlyList<TipoUsuario> Todos()
    {
        return new[]
        {
            SinPermisos,
            Estudiante,
            Docente,
            FuncionarioCE,
            Supervisor,
            Administrador
        };
    }

    public TipoUsuario(string nombre, string descripcion, string imagen, int? maxUnidadesPorCatalogo,
        int? maxPrestamosConcurrentes, IEnumerable<Permiso> permisos)
    {
        string descripcionRecortada = Recortar(descripcion);

        ValidarNombre(nombre);
        ValidarDescripcion(descripcionRecortada);
        ValidarImagen(imagen);
        ValidarTope(maxUnidadesPorCatalogo, "maximo de unidades por catalogo");
        ValidarTope(maxPrestamosConcurrentes, "maximo de prestamos concurrentes");

        Nombre = nombre;
        Descripcion = descripcionRecortada;
        Imagen = imagen;
        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
        this.permisos = new HashSet<Permiso>(permisos);
    }

    private TipoUsuario(string nombre, int? maxPrestamosConcurrentes, int? maxUnidadesPorCatalogo, HashSet<Permiso> permisos)
    {
        Nombre = nombre;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        this.permisos = permisos;
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

    public void ModificarTopes(int maxUnidadesPorCatalogo, int maxPrestamosConcurrentes)
    {
        if (MaxUnidadesPorCatalogo is null && MaxPrestamosConcurrentes is null)
        {
            throw new DominioException("Un tipo de usuario sin limites no admite configurar topes");
        }

        ValidarTope(maxUnidadesPorCatalogo, "maximo de unidades por catalogo");
        ValidarTope(maxPrestamosConcurrentes, "maximo de prestamos concurrentes");

        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }

    public bool PuedeAgregarPrestamo(int prestamosVigentes)
    {
        return MaxPrestamosConcurrentes is null || prestamosVigentes < MaxPrestamosConcurrentes;
    }

    public bool PermiteUnidades(int cantidadSolicitada)
    {
        return MaxUnidadesPorCatalogo is null || cantidadSolicitada <= MaxUnidadesPorCatalogo;
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

    private static void ValidarTope(int? tope, string campo)
    {
        if (tope < 0)
        {
            throw new DominioException($"El {campo} no puede ser negativo");
        }
    }
}
