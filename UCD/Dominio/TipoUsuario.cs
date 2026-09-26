namespace Dominio;

public class TipoUsuario
{
    public static TipoUsuario SinPermisos { get; } = new TipoUsuario();

    private TipoUsuario()
    {
    }

    public bool TienePermiso(Permiso permiso)
    {
        return false;
    }
    
    
}