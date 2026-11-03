namespace DTOs;

public class MovimientoDto
{
    public DateTime FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Solicitante { get; set; } = string.Empty;
    public string? Evaluador { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Unidad { get; set; }
}
