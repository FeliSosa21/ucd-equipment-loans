namespace Dominio;

public class TipoUsuario
{
    private readonly HashSet<Permiso> permisos;

    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(new HashSet<Permiso>());

    public static TipoUsuario Estudiante { get; } = new TipoUsuario(new HashSet<Permiso>
    {
        Permiso.ConsultarCatalogo, 
        Permiso.ConsultarDisponibilidad, 
        Permiso.SolicitarPrestamo,
        Permiso.ConsultarMisSolicitudes
    });

    public static TipoUsuario Docente { get; } = new TipoUsuario(new HashSet<Permiso>
    {
        Permiso.ConsultarCatalogo, 
        Permiso.ConsultarDisponibilidad, 
        Permiso.SolicitarPrestamo,
        Permiso.ConsultarMisSolicitudes, 
        Permiso.ConsultarHistorico
    });

    public static TipoUsuario FuncionarioCE { get; } = new TipoUsuario(new HashSet<Permiso>
    {
        Permiso.ConsultarCatalogo, 
        Permiso.GestionarUnidades, 
        Permiso.RealizarPrestamo, 
        Permiso.RegistrarDevolucion
    });

    public static TipoUsuario Supervisor { get; } = new TipoUsuario(new HashSet<Permiso>
    {
        Permiso.ConsultarCatalogo, 
        Permiso.ConsultarHistorico, 
        Permiso.GestionarCatalogo,
        Permiso.GestionarUnidades, 
        Permiso.EvaluarSolicitud, 
        Permiso.RegistrarDevolucion,
        Permiso.ConsultarPrestamos
    });

    public static TipoUsuario Administrador { get; } = new TipoUsuario(new HashSet<Permiso>(Enum.GetValues<Permiso>()));

private TipoUsuario(HashSet<Permiso> permisos)
    {
        this.permisos = permisos;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }
}