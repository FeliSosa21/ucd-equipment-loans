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

    private static void ValidarFechaHora(DateTime fechaHora)
    {
        if (fechaHora == default)
        {
            throw new DominioException("El registro debe tener fecha y hora");
        }
    }
}
