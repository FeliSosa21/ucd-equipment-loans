namespace Dominio;

public class TipoUsuario
{
    private const int LargoMaximoDescripcion = 100;
    private const int KilobytesMaximosImagen = 500;
    private static readonly string[] PrefijosDeImagenAdmitidos = { "data:image/png;base64,", "data:image/jpeg;base64," };

    private readonly int? maxPrestamosConcurrentes;
    private readonly int? maxUnidadesPorPrestamo;
    private readonly HashSet<Permiso> permisos;

    public string Nombre { get; }
    public string Descripcion { get; private set; } = string.Empty;
    public string Imagen { get; private set; } = string.Empty;
    public int? MaxUnidadesPorCatalogo { get; private set; }
    public int? MaxPrestamosConcurrentes { get; private set; }

    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(
        "Sin permisos",
        maxPrestamosConcurrentes: 0,
        maxUnidadesPorPrestamo: 0,
        new HashSet<Permiso>());

    public static TipoUsuario Estudiante { get; } = new TipoUsuario(
        "Estudiante",
        maxPrestamosConcurrentes: 2,
        maxUnidadesPorPrestamo: 2,
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
        maxUnidadesPorPrestamo: 40,
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
        maxUnidadesPorPrestamo: 0,
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
        maxUnidadesPorPrestamo: 0,
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
        maxUnidadesPorPrestamo: null,
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
        ValidarDescripcion(descripcion);
        ValidarImagen(imagen);
        ValidarMaxUnidadesPorCatalogo(maxUnidadesPorCatalogo);

        Nombre = nombre;
        Descripcion = descripcion;
        Imagen = imagen;
        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
        this.permisos = new HashSet<Permiso>(permisos);
    }

    private TipoUsuario(string nombre, int? maxPrestamosConcurrentes, int? maxUnidadesPorPrestamo, HashSet<Permiso> permisos)
    {
        Nombre = nombre;
        this.maxPrestamosConcurrentes = maxPrestamosConcurrentes;
        this.maxUnidadesPorPrestamo = maxUnidadesPorPrestamo;
        this.permisos = permisos;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }

    public bool PuedeAgregarPrestamo(int prestamosVigentes)
    {
        return maxPrestamosConcurrentes is null || prestamosVigentes < maxPrestamosConcurrentes;
    }

    public bool PermiteUnidades(int cantidadSolicitada)
    {
        return maxUnidadesPorPrestamo is null || cantidadSolicitada <= maxUnidadesPorPrestamo;
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

    private static void ValidarMaxUnidadesPorCatalogo(int? maxUnidadesPorCatalogo)
    {
        if (maxUnidadesPorCatalogo < 0)
        {
            throw new DominioException("El maximo de unidades por catalogo no puede ser negativo");
        }
    }
}
