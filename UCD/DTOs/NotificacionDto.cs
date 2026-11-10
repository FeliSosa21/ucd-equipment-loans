namespace DTOs;

public class NotificacionDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
