namespace Dominio;

public class TipoUsuario
{
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
}