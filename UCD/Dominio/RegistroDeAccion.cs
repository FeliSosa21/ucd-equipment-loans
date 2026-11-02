namespace Dominio;

public class RegistroDeAccion
{
    public Usuario? Responsable { get; }
    public DateTime FechaHora { get; }

    public RegistroDeAccion(Usuario? responsable, DateTime fechaHora)
    {
        Responsable = responsable;
        FechaHora = fechaHora;
    }
}
