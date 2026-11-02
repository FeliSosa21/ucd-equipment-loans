namespace Dominio;

public class RegistroDeAccion
{
    public Usuario? Responsable { get; }
    public DateTime FechaHora { get; }

    public RegistroDeAccion(Usuario? responsable, DateTime fechaHora)
    {
        ValidarFechaHora(fechaHora);

        Responsable = responsable;
        FechaHora = fechaHora;
    }

    public static RegistroDeAccion DeUsuario(Usuario usuario, DateTime fechaHora)
    {
        throw new NotImplementedException();
    }

    public static RegistroDeAccion DelSistema(DateTime fechaHora)
    {
        throw new NotImplementedException();
    }

    private static void ValidarFechaHora(DateTime fechaHora)
    {
        if (fechaHora == default)
        {
            throw new DominioException("El registro debe tener fecha y hora");
        }
    }
}
