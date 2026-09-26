namespace Dominio;

public class TipoUsuario
{
    public static TipoUsuario SinPermisos { get; } = new TipoUsuario();
    public static TipoUsuario Estudiante { get; } = new TipoUsuario();

    private TipoUsuario()
    {
    }

    public bool TienePermiso(Permiso permiso)
    {
        return this == Estudiante;
    }
    
    
}