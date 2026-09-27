namespace Dominio;

public class TipoUsuario
{
    private readonly List<Permiso> permisos;
    
    public static TipoUsuario SinPermisos { get; } = new TipoUsuario(new List<Permiso>());
    public static TipoUsuario Estudiante { get; } = new TipoUsuario(new List<Permiso> { Permiso.ConsultarCatalogo, Permiso.ConsultarDisponibilidad, Permiso.SolicitarPrestamo, Permiso.ConsultarMisSolicitudes});
    public static TipoUsuario Docente { get; } = new TipoUsuario(new List<Permiso>{Permiso.ConsultarCatalogo, Permiso.ConsultarDisponibilidad, Permiso.SolicitarPrestamo, Permiso.ConsultarMisSolicitudes, Permiso.ConsultarHistorico});
    public static TipoUsuario Funcionario { get; } = new TipoUsuario(new List<Permiso>());

    private TipoUsuario(List<Permiso> permisos)
    {
        this.permisos = permisos;
    }

    public bool TienePermiso(Permiso permiso)
    {
        return permisos.Contains(permiso);
    }
}