using System.Reflection;

namespace Dominio;

public class TipoUsuario
{
    public string Nombre { get; }
    private readonly int? MaxPrestamosConcurrentes;
    private readonly int? maxUnidadesPorPrestamo;
    private readonly HashSet<Permiso> permisos;

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
        return new []
        {
            SinPermisos,
            Estudiante,
            Docente,
            FuncionarioCE,
            Supervisor,
            Administrador
        };
    }

    public bool PuedeAgregarPrestamo(int prestamosVigentes)
    {
        return MaxPrestamosConcurrentes is null || prestamosVigentes < MaxPrestamosConcurrentes;
    }

    public bool PermiteUnidades(int cantidadSolicitada)
    {
        return maxUnidadesPorPrestamo is null || cantidadSolicitada <= maxUnidadesPorPrestamo;
    }
    private TipoUsuario(string nombre, int? maxPrestamosConcurrentes, int? maxUnidadesPorPrestamo, HashSet<Permiso> permisos)
    {
        this.Nombre = nombre;
        this.MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
        this.maxUnidadesPorPrestamo = maxUnidadesPorPrestamo;
        this.permisos = permisos;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }
}