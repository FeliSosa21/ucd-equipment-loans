namespace Dominio;

public class RegistroDeAccion
{
    public Usuario? Responsable { get; private set; }
    public DateTime FechaHora { get; private set; }

    private RegistroDeAccion()
    {
    }

    private RegistroDeAccion(Usuario? responsable, DateTime fechaHora)
    {
        ValidarFechaHora(fechaHora);

        Responsable = responsable;
        FechaHora = fechaHora;
    }

    public static RegistroDeAccion DeUsuario(Usuario usuario, DateTime fechaHora)
    {
        if (usuario is null)
        {
            throw new DominioException("El registro debe tener el usuario que hizo la accion");
        }

        return new RegistroDeAccion(usuario, fechaHora);
    }

    public static RegistroDeAccion DelSistema(DateTime fechaHora)
    {
        return new RegistroDeAccion(null, fechaHora);
    }

    private static void ValidarFechaHora(DateTime fechaHora)
    {
        if (fechaHora == default)
        {
            throw new DominioException("El registro debe tener fecha y hora");
        }
    }
}
