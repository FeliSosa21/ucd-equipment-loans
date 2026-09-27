namespace Dominio;

public class TipoUsuario
{
    private readonly List<Permiso> permisos;

    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(new List<Permiso>());

    public static TipoUsuario Estudiante { get; } = new TipoUsuario(new List<Permiso>
    {
        Permiso.ConsultarCatalogo, Permiso.ConsultarDisponibilidad, Permiso.SolicitarPrestamo,
        Permiso.ConsultarMisSolicitudes
    });

    public static TipoUsuario Docente { get; } = new TipoUsuario(new List<Permiso>
    {
        Permiso.ConsultarCatalogo, Permiso.ConsultarDisponibilidad, Permiso.SolicitarPrestamo,
        Permiso.ConsultarMisSolicitudes, Permiso.ConsultarHistorico
    });

    public static TipoUsuario FuncionarioCE { get; } = new TipoUsuario(new List<Permiso>
    {
        Permiso.ConsultarCatalogo, Permiso.GestionarUnidades, Permiso.RealizarPrestamo, 
        Permiso.RegistrarDevolucion
    });

    public static TipoUsuario Supervisor { get; } = new TipoUsuario(new List<Permiso>
    {
        Permiso.ConsultarCatalogo, Permiso.ConsultarHistorico, Permiso.GestionarCatalogo,
        Permiso.GestionarUnidades, Permiso.EvaluarSolicitud, Permiso.RegistrarDevolucion,
        Permiso.ConsultarPrestamos
    });

private TipoUsuario(List<Permiso> permisos)
    {
        this.permisos = permisos;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }
}